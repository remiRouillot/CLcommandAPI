using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using ClosedXML.Excel;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Expressions;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace CLcommandAPI.Services
{
    public class JournalService
    {

        public readonly NTiConnection _conn;

        public JournalService(DbConnectionService dbConnection)
        {
            _conn = dbConnection.conn;
        }

        public IEnumerable<dynamic> GetAllJournals()
        {
            try
            {
                _conn.Open();
                string sql = $"SELECT JOURNAL_NAME, JOURNAL_LIBRARY FROM QSYS2.JOURNAL_INFO";

                var journals = _conn.Query<dynamic>(sql).ToList();
                return journals;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des journaux : {ex.Message}");
                return Enumerable.Empty<dynamic>();
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }

        public IEnumerable<dynamic> ListAndFilterByJournalName(string journalName, string journalLib)
        {
            string outPutFileJobs = "TEMPLIB";
            string outPutFileName = "DSPJNL";

            try
            {
                _conn.Open();
                string clCommand = $"DSPJRN JRN({journalLib}/{journalName}) OUTPUT(*OUTFILE) OUTFILFMT(*TYPE5) OUTFILE({outPutFileJobs}/{outPutFileName}) OUTMBR(*FIRST *REPLACE)";
                _conn.ExecuteClCommand(clCommand);

                string sql = $@"SELECT 
                    JOENTL AS ""Longueur Entrée"",
                    JOSEQN AS ""Numéro Séquence"",
                    JOCODE AS ""Code Journal"",
                    JOENTT AS ""Type Entrée"",
                    JOTSTP AS ""Horodatage"",
                    JOJOB AS ""Nom Job"",
                    JOUSER AS ""Nom Utilisateur"",
                    JONBR AS ""Numéro Job"",
                    JOPGM AS ""Nom Programme"",
                    JOPGMLIB AS ""Bibliothèque Programme"",
                    JOOBJ AS ""Nom Objet"",
                    JOLIB AS ""Bibliothèque Objet"",
                    JOMBR AS ""Membre Objet"",
                    JOCTRR AS ""Compteur Entrée"",
                    JOFLAG AS ""Indicateurs"",
                    JOCCID AS ""ID Corrélation"",
                    JORCV AS ""Récepteur Journal"",
                    JORCVLIB AS ""Bibliothèque Récepteur"",
                    JOARM AS ""Numéro Armement"",
                    JOTHD AS ""Thread ID"",
                    JOTHDX AS ""Extension Thread ID"",
                    JORPORT AS ""Port Réseau"",
                    JORADR AS ""Adresse Réseau"",
                    JOLUW AS ""Unité Travail Logique"",
                    JOXID AS ""ID Transaction""
                    FROM {outPutFileJobs}.{outPutFileName}";
                return _conn.Query<dynamic>(sql).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération : {ex.Message}");
                return Enumerable.Empty<dynamic>();
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }

        //public byte[] ExportJournalsDetailsToExcell(IEnumerable<dynamic> journalsDetails)
        //{
        //    using (var workbook = new XLWorkbook())
        //    {

        //    }
        //}
    }
}
