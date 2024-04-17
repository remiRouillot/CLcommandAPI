using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Expressions;
using System.Data;
using System.Text.RegularExpressions;

namespace CLcommandAPI.Services
{
    public class LibrariesService
    {

        public readonly NTiConnection _conn;

        public LibrariesService(DbConnectionService dbConnection)
        {
            _conn = dbConnection.conn;
        }

        public string CreateLibrary(string libraryName)
        {
            // Vérifier validité nom bibliothèque (alphanumérique uniquement)
            libraryName = libraryName.ToUpper();

            if (!Regex.IsMatch(libraryName, @"^[a-zA-Z0-9]+$"))
            {
                return "Nom de bibliothèque invalide.";
            }

            try
            {
                _conn.Open();
                string clCommand = $"CRTLIB LIB({libraryName})";
                _conn.ExecuteClCommand(clCommand);

                return $"Bibliothèque {libraryName} créée avec succès.";
            }
            catch (Exception ex)
            {
                return $"erreur lors de la création de la bibliothèque :" + ex.Message;
            }
            finally
            {
                _conn.Close();
            }
        }

        public string DeleteLibrary(string libraryName)
        {

            libraryName = libraryName.ToUpper();

            if (!Regex.IsMatch(libraryName, @"^[a-zA-Z0-9]+$"))
            {
                return "Nom de bibliothèque invalide.";
            }


            try
            {
                _conn.Open();

                int objectCount = GetObjectCountInLibraryBeforeDelete(libraryName);

                if (objectCount == 0)
                {
                    string clCommand = $"DLTLIB LIB({libraryName})";
                    _conn.ExecuteClCommand(clCommand);
                    return $"Bibliothèque {libraryName} supprimée avec succès";
                }
                else
                {
                    return $"La bibliothèque {libraryName} n'est pas vide. Suppression annulée";
                }
            }
            catch (Exception ex)
            {
                return $"Erreur lors de la tentative de suppression de la bibliothèque {libraryName}: {ex.Message}";
            }
            finally
            {
                _conn.Close();
            }
        }


        public IEnumerable<dynamic> GetLibraryName()
        {
            try
            {
                _conn.Open();
                string sql = $"SELECT SCHEMA_NAME FROM QSYS2.SYSSCHEMAS";

                var liBraryName = _conn.Query<dynamic>(sql).ToList();
                return liBraryName;
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

        // Methode pour fitrer par Bibliothèque, type d'objet et champs spécifiques

        // Bibliothèque : nom de bibliothèque
        // Type d'objt : LIB, PGM, FILE, etc...
        // Champs spécifique :
                // ODLBNM ( Nom de la bibliothèque où se trouve l'objet),
                // ODOBNM ( Nom de l'objet (dans ce cas, le nom de la bibliothèque).)
                // ODOBTP ( Type d'objet (*LIB indique qu'il s'agit d'une bibliothèque).)
                // ODOBAT ( Attribut de l'objet, tel que PROD (production), TEST (test), etc)
                // ODOBOW ( Propriétaire de l'objet)
                // etc.....

        public IEnumerable<dynamic> ListAndFilterLibraryObjectsFields(string libraryName, string[] fields)
        {
            string outPutFileLibrary = "TEMPLIB";
            string outPutFileName = "OUTFILE";

            // Liste des champs pour filtrage
            var validFields = new List<string> { "ODLBNM", "ODOBNM", "ODOBTP", "ODOBSZ", "ODOBAT", "ODOBOW", "ODCRTU", "ODCRTS" }; 
            var selectedFields = fields.Where(field => validFields.Contains(field)).ToList();
            var fieldsToQuery = selectedFields.Any() ? string.Join(", ", selectedFields) : "*"; // Sélectionne tous les champs si aucun spécifié


            try
            {
                _conn.Open();
                // Exécution cmd DSPOBJD avec paramètres
                string clCommand = $"DSPOBJD OBJ({libraryName}/*ALL) OBJTYPE(*ALL) OUTPUT(*OUTFILE) OUTFILE({outPutFileLibrary}/{outPutFileName}) OUTMBR(*FIRST *REPLACE)";
                _conn.ExecuteClCommand(clCommand);

                string sql = $"SELECT {fieldsToQuery} FROM {outPutFileLibrary}.{outPutFileName}";
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


        //METHODE ANNEXES

        // Récupérer le nb d'objet dans une librairie pour s'assurer qu'elle soit bien vide avant sa suppression
        public int GetObjectCountInLibraryBeforeDelete(string libraryName)
        {
            int objectCount = 0;
            try
            {
                string sql = $"SELECT COUNT(*) FROM QSYS2.SYSTABLES WHERE TABLE_SCHEMA = @LibraryName"; //@LibraryName est un paramètre nommé pr prevenir injection SQL
                objectCount = _conn.Query<int>(sql, new { LibraryName = libraryName }).FirstOrDefault(); // récupère le premier résultat comme int
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution de la requête SQL: {ex.Message}");
            }

            return objectCount; // Retourne nombre d'objets trouvé
        }
    }
}


