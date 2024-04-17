using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Expressions;
using System.Data;
using System.Text.RegularExpressions;

namespace CLcommandAPI.Services
{
    public class UserService
    {

        private readonly NTiConnection _conn;

        public UserService(DbConnectionService dbConnection)
        {
            _conn = dbConnection.conn;
        }

        public IEnumerable<dynamic> ListAndFilterUserByName(string userName)
        {

            string outPutFileJobs = "TEMPLIB";
            string outPutFileName = "LISTUSER";

            //Def d'une liste de champs à retourner
            var fields = new List<string> { "UPUPRF AS Username", "UPTEXT AS Description", "UPSTAT AS Status", "UPPWEX AS PasswordExpired", "UPCRTD AS CreationDate", "UPCHGD AS LastChangedDate", "UPPWCD AS LastPasswordChangeDate", "UPSPAU AS SpecialAuthorities", "UPUID AS UserID", "UPGID AS GroupID" };
            var fieldsToQuery = fields.Any() ? string.Join(", ", fields) : "*"; // si field vide, tt est selectionné

            try
            {
                _conn.Open();
                string userFilter = userName.ToUpper() == "*ALL" ? "*ALL" : userName; // si UserName = *ALL true, sinon on affiche le nom recup
                string clCommand = $"DSPUSRPRF USRPRF({userFilter}) OUTPUT(*OUTFILE) OUTFILE({outPutFileJobs}/{outPutFileName})";
                _conn.ExecuteClCommand(clCommand);

                string sql = $"SELECT {fieldsToQuery} FROM {outPutFileJobs}.{outPutFileName}";
                return _conn.Query<dynamic>(sql).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de récupération des USERS : {ex.Message}");
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
    }
}
