using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using Dapper;
using System.Data;
using System.Runtime.InteropServices.JavaScript;
using System.Text.RegularExpressions;
using ClosedXML;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;

namespace CLcommandAPI.Services
{
    public class MessageQueueService
    {
        public readonly NTiConnection _conn;

        public MessageQueueService(DbConnectionService dbConnection)
        {
            _conn = dbConnection.conn;
        }

        public IEnumerable<dynamic> GetMessageQueueInfo(string queueLibrary = "QSYS", string queueName = "QSYSOPR", string filterType = "")
        {
            var validMessageTypes = new HashSet<string> { "COMPLETION", "INFORMATIONAL", "INQUIRY", "REPLY" };

            if (!string.IsNullOrEmpty(filterType) && !validMessageTypes.Contains(filterType.ToUpper()))
            {
                //leve exceptioneet et empeche requete avec filtre invalide
                throw new ArgumentException("Type de filtre non valide.");
            }

            string sql = $@"
                SELECT * 
                FROM TABLE(QSYS2.MESSAGE_QUEUE_INFO(
                    QUEUE_LIBRARY => @QueueLibrary, 
                    QUEUE_NAME => @QueueName))
                    {(!string.IsNullOrEmpty(filterType) ? "WHERE MESSAGE_TYPE = @FilterType" : "")}";
            // Si filterType est fourni, requête qui filtre par filtertype, sinon sélectionne tout
            try
            {
                _conn.Open();
                // Création objet anonyme pr passer paramètres
                var queryParameters = new { QueueLibrary = queueLibrary, QueueName = queueName, FilterType = filterType };
                var messages = _conn.Query<dynamic>(sql, queryParameters).ToList();
                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des messages de la file d'attente : {ex.Message}");
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


        public IEnumerable<dynamic> GetInquiryMessages()
        {
            try
            {
                _conn.Open();
                string sql = @"
            SELECT 
                MSGQ_LIB, MSGQ_NAME, MSGID, MSG_TYPE, MSG_SUBTYP, MSG_TEXT, SEVERITY, MSG_TIME, 
                A.ASSOC_KEY, FROM_USER, FROM_JOB, FROM_PGM, MSGF_LIB, MSGF_NAME, MSG_TOKENS, 
                MSG_TEXT2, hex(message_key) as MSGKEY 
            FROM
                (SELECT * FROM qsys2.message_queue_info WHERE ASSOC_KEY is null AND MSGQ_NAME='QSYSOPR') as A 
            LEFT JOIN
                (SELECT ASSOC_KEY FROM qsys2.message_queue_info WHERE ASSOC_KEY is not null AND MSGQ_NAME='QSYSOPR') as B 
                ON A.message_key=B.ASSOC_KEY 
            WHERE 
                MSG_TYPE='INQUIRY' and B.ASSOC_KEY is null";
                return _conn.Query<dynamic>(sql).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des messages en attente de réponse: {ex.Message}");
                return new List<dynamic>();
            }
            finally
            {
                _conn.Close();
            }
        }


        public void ReplyToInquiryMessage(string messageKey, string reply)
        {


            var validReplies = new HashSet<string> { "I", "C" };
            if (!validReplies.Contains(reply))
            {
                throw new ArgumentException("Type de réponse non valide.");
            }

            try
            {
                _conn.Open();
                string clCommand = $"SNDRPY MSGKEY(x'{messageKey}') MSGQ(QSYSOPR) RPY('{reply}') RMV(*NO)";
                _conn.ExecuteClCommand(clCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de la réponse au message : {ex.Message}");
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }

        public IEnumerable<dynamic> GetHistoryLogInfo(int daysAgo)
        {
            try
            {
                string sql = $@"SELECT
                    MESSAGE_ID, 
                    MESSAGE_TYPE, 
                    MESSAGE_TIMESTAMP, 
                    FROM_USER, FROM_JOB, 
                    FROM_PROGRAM, 
                    MESSAGE_LIBRARY, 
                    MESSAGE_FILE, 
                    MESSAGE_TEXT, 
                    MESSAGE_SECOND_LEVEL_TEXT
                    FROM TABLE(QSYS2.HISTORY_LOG_INFO(TIMESTAMP_FORMAT(CHAR(CURRENT DATE - {daysAgo} DAYS), 'YYYY-MM-DD')))
                ";
                var logs = _conn.Query<dynamic>(sql).AsList();
                return logs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'historique: {ex.Message}");
                return new List<dynamic>();
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }

        public string SendBreakMessage(string messageText, string messageQueue = "*ALLWS", string messageType = "*INFO")
        {
            if (string.IsNullOrEmpty(messageText) || messageText.Length > 512)
            {
                throw new ArgumentException("le texte du message est invalide");
            }

            if (!new HashSet<string> { "*ALLWS", "*USRPRF" }.Contains(messageQueue.ToUpper()))
            {
                throw new ArgumentException("La file d'attente de message cible est invalide.");
            }

            try
            {
                _conn.Open();
                string clCommand = $"SNDBRKMSG MSG('{messageText}') TOMSGQ({messageQueue}) MSGTYPE({messageType})";
                _conn.ExecuteClCommand(clCommand);
                return "Message envoyé avec succès.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi du message de rupture : {ex.Message}");
                return $"Erreur lors de l'envoi du message : {ex.Message}";
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                {
                    _conn.Close();
                }
            }
        }


        public byte[] ExportMessagesToExcel(IEnumerable<dynamic> messages)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Messages");
                worksheet.Cell(1, 1).Value = "MESSAGE_ID";
                worksheet.Cell(1, 2).Value = "MESSAGE_TYPE";
                worksheet.Cell(1, 3).Value = "SEVERITY";
                worksheet.Cell(1, 4).Value = "MESSAGE_TIMESTAMP";
                worksheet.Cell(1, 5).Value = "FROM_USER";
                worksheet.Cell(1, 6).Value = "FROM_JOB";
                worksheet.Cell(1, 7).Value = "FROM_PROGRAM";
                worksheet.Cell(1, 8).Value = "MESSAGE_FILE_LIBRARY";
                worksheet.Cell(1, 9).Value = "MESSAGE_FILE_NAME";
                worksheet.Cell(1, 10).Value = "MESSAGE_TEXT";
                worksheet.Cell(1, 11).Value = "MESSAGE_SECOND_LEVEL_TEXT";
                int currentRow = 2; // début ligne 2 pr laisser entête libre

                foreach (var msg in messages)
                {
                    worksheet.Cell(currentRow, 1).Value = msg.MESSAGE_ID ?? ""; // tag helper pr données nulles
                    worksheet.Cell(currentRow, 2).Value = msg.MESSAGE_TYPE ?? "";
                    worksheet.Cell(currentRow, 3).Value = msg.SEVERITY ?? "";
                    worksheet.Cell(currentRow, 4).Value = msg.MESSAGE_TIMESTAMP ?? "";
                    worksheet.Cell(currentRow, 5).Value = msg.FROM_USER ?? "";
                    worksheet.Cell(currentRow, 6).Value = msg.FROM_JOB ?? "";
                    worksheet.Cell(currentRow, 7).Value = msg.FROM_PROGRAM ?? "";
                    worksheet.Cell(currentRow, 8).Value = msg.MESSAGE_FILE_LIBRARY ?? "";
                    worksheet.Cell(currentRow, 9).Value = msg.MESSAGE_FILE_NAME ?? "";
                    worksheet.Cell(currentRow, 10).Value = msg.MESSAGE_TEXT ?? "";
                    worksheet.Cell(currentRow, 11).Value = msg.MESSAGE_SECOND_LEVEL_TEXT ?? "";
                    currentRow++;
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
