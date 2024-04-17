using System;
using System.Data;
using System.Xml.Serialization;
using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLcommandAPI.Services
{
    // Service pour interagir avec l'API système IBM i
    public class ClcommandServiceAPi
    {
        private readonly NTiConnection _conn;
        public ClcommandServiceAPi(DbConnectionService dbConnection)
        {
            _conn = dbConnection.conn;
        }


        // CMD = cpyf
        public async Task<CommandDetails> GetCommandDetailsAsync(string commandName, string commandLibrary = "*LIBL")
        {
            string cmdName = commandName;
            string cmdLib = commandLibrary;
            bool requiredParametersOnly = false;

            CommandDetails commandDetails = null; // Déclaration à l'extérieur du bloc try

            try
            {
                await _conn.OpenAsync();

                var parms = new List<NTiProgramParameter>()
                {
                    new NTiProgramParameter(cmdName, 10).Append(cmdLib, 10),
                    new NTiProgramParameter(8),
                    new NTiProgramParameter("DEST0100", 8),
                    new NTiProgramParameter("", 8),
                    new NTiProgramParameter(requiredParametersOnly ? "CMDD0100" : "CMDD0200", 8),
                    new NTiProgramParameter("", 8),
                };

                await Task.Run(() => _conn.CallProgram("QSYS", "QCDRCMDD", parms));

                var ll = parms[3].GetInt(4);
                parms[1] = new NTiProgramParameter(ll);
                parms[3] = new NTiProgramParameter("", ll).AsOutput();

                await Task.Run(() => _conn.CallProgram("QSYS", "QCDRCMDD", parms));

                ll = parms[3].GetInt(0);
                string xmlData = parms[3].GetString(8, ll, 1208);


                try
                {
                    var commandData = DeserializeXml(xmlData);
                    commandDetails = TransformToCommandDetails(commandData, commandName, commandLibrary); // Affectation à l'intérieur du bloc try
                }
                catch (InvalidOperationException ex)
                {
                    throw new Exception("Erreur lors de la désérialisation du XML : " + ex.InnerException?.Message, ex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des données de QCDCLCMD : {ex.Message}");
                throw;
            }
            finally
            {
                if (_conn != null && _conn.State == ConnectionState.Open)
                {
                    await _conn.CloseAsync();
                }
            }

            return commandDetails; // Retourner en dehors du bloc try
        }

        private QcdCLCmd DeserializeXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(QcdCLCmd));
            using (StringReader reader = new StringReader(xml))
            {
                return (QcdCLCmd)serializer.Deserialize(reader);
            }
        }

        // methode pr transformer donnée déserialisée en objet
        private CommandDetails TransformToCommandDetails(QcdCLCmd commandData, string commandName, string commandLibrary)
        {
            var currentCommand = commandData.Cmds.First();
            var detailedCommandInformation = new CommandDetails
            {
                Name = currentCommand.CmdName,
                Library = currentCommand.CmdLib,
                CCSID = currentCommand.CCSID,
                ExecBatch = currentCommand.ExecBatch,
                Parameters = currentCommand.Parms.Select(parameter => new CommandParameter
                {
                    Keyword = parameter.Kwd,
                    Type = parameter.Type,
                    DefaultValue = parameter.DefaultValue,
                    MinimumValue = parameter.Min,
                    MaximumValue = parameter.Max,
                    PromptText = parameter.Prompt,
                    Qualifiers = parameter.Quals?.Select(qualifier => new Qualification
                    {
                        QualifierType = qualifier.Type,
                        Length = qualifier.Len
                    }).ToList() ?? new List<Qualification>()
                }).ToList(),
                Dependencies = currentCommand.Deps?.Select(dependency => new Dependency
                {
                    ControlKeyword = dependency.CtlKwd,
                    MessageID = dependency.MsgID,
                    DependentParameters = dependency.DepParms.Select(dependentParameter => new DependentParameter
                    {
                        DependentKeyword = dependentParameter.Kwd
                    }).ToList()
                }).ToList() ?? new List<Dependency>()
            };

            return detailedCommandInformation;
        }
    }


    // Modèles pr XML à partir du schéma doc
    [XmlRoot("QcdCLCmd")]
    public class QcdCLCmd

    //Contient liste commande
    {
        [XmlElement("Cmd")]
        public List<Cmd> Cmds { get; set; }
    }




    // Représente commande (nom, lib, CCSID, attributs, paramètres et dépendances)
    public class Cmd
    {
        [XmlAttribute("CmdName")]
        public string CmdName { get; set; }

        [XmlAttribute("CmdLib")]
        public string CmdLib { get; set; }

        [XmlAttribute("CCSID")]
        public string CCSID { get; set; }

        [XmlAttribute("Prompt")]
        public string Prompt { get; set; }

        [XmlAttribute("ExecBatch")]
        public string ExecBatch { get; set; }

        [XmlElement("Parm")]
        public List<Parm> Parms { get; set; } = new List<Parm>();

        [XmlElement("Dep")]
        public List<Dep> Deps { get; set; } = new List<Dep>();
    }

    // Détaille paramètre commande (motclé, type, valeur par default, prompt)
    public class Parm
    {
        [XmlAttribute("Kwd")]
        public string Kwd { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("Dft")]
        public string DefaultValue { get; set; }

        [XmlAttribute("Min")]
        public string Min { get; set; }

        [XmlAttribute("Max")]
        public string Max { get; set; }

        [XmlAttribute("Prompt")]
        public string Prompt { get; set; }

        [XmlElement("Qual")]
        public List<Qual> Quals { get; set; }

        [XmlElement("SpcVal")]
        public SpcVal SpcVal { get; set; } 

        [XmlElement("SngVal")]
        public SngVal SngVal { get; set; }

        [XmlElement("Elem")]
        public List<Elem> Elems { get; set; }
    }

    public class Elem
    {
        [XmlAttribute("PromptMsgID")]
        public string PromptMsgID { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("Min")]
        public string Min { get; set; }

        [XmlAttribute("Max")]
        public string Max { get; set; }

        [XmlElement("SpcVal")]
        public SpcVal SpcVal { get; set; }

        [XmlElement("SngVal")]
        public SngVal SngVal { get; set; }

    }

    //Qualification pr paramètre (type et longueur)
    public class Qual
    {
        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("Len")]
        public string Len { get; set; }
    }

    //Dépendance paramètre (id message pr expliquer la dépendance)
    public class Dep
    {
        [XmlAttribute("NbrTrue")]
        public string NbrTrue { get; set; }

        [XmlAttribute("NbrTrueRel")]
        public string NbrTrueRel { get; set; }

        [XmlAttribute("CmpVal")]
        public string CmpVal { get; set; }

        [XmlAttribute("CtlKwdRel")]
        public string CtlKwdRel { get; set; }
        [XmlAttribute("CtlKwd")]
        public string CtlKwd { get; set; }

        [XmlAttribute("MsgID")]
        public string MsgID { get; set; }

        [XmlElement("DepParm")]
        public List<DepParm> DepParms { get; set; }
    }

    //paramètre spécifique 
    public class DepParm
    {
        [XmlAttribute("Kwd")]
        public string Kwd { get; set; }

        [XmlAttribute("Rel")]
        public string Relation { get; set; }

        [XmlAttribute("CmpVal")]
        public string CompareValue { get; set; }

        [XmlAttribute("CmpKwd")]
        public string CompareKeyword { get; set; }
    }


    /// <summary>
    /// ////////////////////////////////////////////////////////////////
    /// </summary>
    /// 
    // Classes détails commande après transformation XML
    public class CommandDetails
    {
        public string Name { get; set; }
        public string Library { get; set; }
        public List<CommandParameter> Parameters { get; set; } = new List<CommandParameter>();
        public string CCSID { get; set; }
        public string ExecBatch { get; set; }

        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();
    }

    // paramètre de la cmd avec détails
    public class CommandParameter
    {
        public string Keyword { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string MinimumValue { get; set; }
        public string MaximumValue { get; set; }
        public string PromptText { get; set; }
        public List<Qualification> Qualifiers { get; set; } = new List<Qualification>();
    }

    //Qualification apliquée à un paramètre
    public class Qualification
    {
        public string QualifierType { get; set; }
        public string Length { get; set; }
    }

    public class Dependency
    {
        public string ControlKeyword { get; set; }
        public string MessageID { get; set; }
        public List<DependentParameter> DependentParameters { get; set; } = new List<DependentParameter>();
    }

    public class DependentParameter
    {
        public string DependentKeyword { get; set; }
    }

    public class SpcVal
    {
        [XmlElement("Value")]
        public List<Value> Values { get; set; }
    }

    public class SngVal
    {
        [XmlElement("Value")]
        public List<Value> Values { get; set; }
    }

    public class Value
    {
        [XmlAttribute("Val")]
        public string Val { get; set; }

        [XmlAttribute("MapTo")]
        public string MapTo { get; set; }
    }

}

