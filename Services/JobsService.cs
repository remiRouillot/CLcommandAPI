using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Expressions;
using System.Data;
using System.Text.RegularExpressions;


namespace CLcommandAPI.Services
{
    public class JobsService
    {

        public readonly NTiConnection _conn;

        public JobsService(DbConnectionService dbConnection)
        {
            _conn = dbConnection.conn;
        }
        public IEnumerable<dynamic> GetActiveJobs(string[] selectedFields)
        {

            var validFields = new List<string> {
                "ORDINAL_POSITION",
                "JOB_NAME",
                "JOB_NAME_SHORT",
                "JOB_USER",
                "JOB_NUMBER",
                "INTERNAL_JOB_ID",
                "SUBSYSTEM",
                "SUBSYSTEM_LIBRARY_NAME",
                "AUTHORIZATION_NAME",
                "JOB_TYPE",
                "FUNCTION_TYPE",
                "FUNCTION",
                "JOB_STATUS",
                "MEMORY_POOL",
                "RUN_PRIORITY",
                "THREAD_COUNT",
                "TEMPORARY_STORAGE",
                "CPU_TIME",
                "TOTAL_DISK_IO_COUNT",
                "ELAPSED_INTERACTION_COUNT",
                "ELAPSED_TOTAL_RESPONSE_TIME",
                "ELAPSED_TOTAL_DISK_IO_COUNT",
                "ELAPSED_ASYNC_DISK_IO_COUNT",
                "ELAPSED_SYNC_DISK_IO_COUNT",
                "ELAPSED_CPU_PERCENTAGE",
                "ELAPSED_CPU_TIME",
                "ELAPSED_PAGE_FAULT_COUNT",
                "JOB_END_REASON",
                "SERVER_TYPE",
                "ELAPSED_TIME",
                "JOB_DESCRIPTION_LIBRARY",
                "JOB_DESCRIPTION",
                "JOB_QUEUE_LIBRARY",
                "JOB_QUEUE",
                "OUTPUT_QUEUE_LIBRARY",
                "OUTPUT_QUEUE",
            };
            var fieldsToQuery = selectedFields.Length > 0 ? string.Join(", ", selectedFields) : "*"; // Sélectionne tous les champs si aucun spécifié

            try
            {
                _conn.Open();
                string serviceSql = $"SELECT {fieldsToQuery} FROM TABLE (QSYS2.ACTIVE_JOB_INFO())";
                var activeJobs = _conn.Query<dynamic>(serviceSql).ToList();

                return activeJobs;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des travaux actifs : {ex.Message}");
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

        //Mettre en attente un travail
        public void HoldJob(string jobName)
        {
            try
            {
                _conn.Open();
                string clCommand = $"HLDJOB JOB({jobName})";
                _conn.ExecuteClCommand(clCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise en attente du travail : {ex.Message}");
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

        // Reprendre un travail

        public void ReleaseJob(string jobName)
        {
            try
            {
                _conn.Open();
                string clCommand = $"RLSJOB JOB({jobName})";
                _conn.ExecuteClCommand(clCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la reprise du travail : {ex.Message}");
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


        // Arrêter un job complet 


        public void EndJob(string jobName, string option = "*IMMED")
        {
            try
            {
                _conn.Open();
                string clCommand = $"ENDJOB JOB({jobName}) OPTION({option})";
                _conn.ExecuteClCommand(clCommand);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'arrêt du job {jobName}: {ex.Message}");
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


        // Afficher message d'erreur en fonction d'un job particulier

        public IEnumerable<dynamic> GetJobMessageByJobName(string jobName)
        {
            try
            {
                _conn.Open();
                string sql = $@"SELECT * FROM TABLE(QSYS2.MESSAGE_QUEUE_INFO()) WHERE FROM_JOB LIKE '{jobName}'";
                var result = _conn.Query<dynamic>(sql).ToList();

                if (result.Any()) // Vérifie si liste contient éléments
                {
                    return result;
                }
                else
                {
                    // Si aucun message n'est trouvé, retourne une liste vide
                    Console.WriteLine($"Aucun message trouvé pour le job {jobName}.");
                    return Enumerable.Empty<dynamic>();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des messages pour le job {jobName} : {ex.Message}");
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
