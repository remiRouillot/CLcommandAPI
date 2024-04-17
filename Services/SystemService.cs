using Aumerial.Data.Nti;
using CLcommandAPI.DataBase;
using Dapper;
using System.Data;

namespace CLcommandAPI.Services
{
    public class SystemService
    {

        private readonly NTiConnection _conn;

        public SystemService(DbConnectionService dbconnexion)
        {
            _conn = dbconnexion.conn;
        }
        
        public IEnumerable<dynamic> GetInformationSystem()
        {
            try
            {
                _conn.Open();
                string serviceSql = $@"
                    SELECT TOTAL_JOBS_IN_SYSTEM,
                        MAXIMUM_JOBS_IN_SYSTEM,
                        ACTIVE_JOBS_IN_SYSTEM,
                        INTERACTIVE_JOBS_IN_SYSTEM,
                        ELAPSED_TIME,
                        ELAPSED_CPU_USED,
                        CONFIGURED_CPUS,
                        CURRENT_CPU_CAPACITY,
                        AVERAGE_CPU_RATE,
                        AVERAGE_CPU_UTILIZATION,
                        MINIMUM_CPU_UTILIZATION,
                        MAXIMUM_CPU_UTILIZATION,
                        MAIN_STORAGE_SIZE,
                        SYSTEM_ASP_STORAGE,
                        TOTAL_AUXILIARY_STORAGE,
                        SYSTEM_ASP_USED,
                        CURRENT_TEMPORARY_STORAGE,
                        MAXIMUM_TEMPORARY_STORAGE_USED,
                        PERMANENT_ADDRESS_RATE,
                        TEMPORARY_ADDRESS_RATE,
                        TEMPORARY_256MB_SEGMENTS,
                        TEMPORARY_4GB_SEGMENTS,
                        PERMANENT_256MB_SEGMENTS,
                        PERMANENT_4GB_SEGMENTS,
                        JOBQ_JOB_TABLE_ENTRIES,
                        OUTQ_JOB_TABLE_ENTRIES,
                        HOST_NAME,
                        PARTITION_ID,
                        NUMBER_OF_PARTITIONS,
                        ACTIVE_THREADS_IN_SYSTEM,
                        RESTRICTED_STATE,
                        PARTITION_NAME,
                        PARTITION_GROUP_ID,
                        DEFINED_MEMORY,
                        MINIMUM_MEMORY,
                        MAXIMUM_MEMORY,
                        MEMORY_INCREMENT,
                        DEDICATED_PROCESSORS,
                        PHYSICAL_PROCESSORS,
                        MAXIMUM_PHYSICAL_PROCESSORS,
                        DEFINED_VIRTUAL_PROCESSORS,
                        VIRTUAL_PROCESSORS,
                        MINIMUM_VIRTUAL_PROCESSORS,
                        MAXIMUM_VIRTUAL_PROCESSORS,
                        DEFINED_PROCESSING_CAPACITY,
                        PROCESSING_CAPACITY,
                        UNALLOCATED_PROCESSING_CAPACITY,
                        MINIMUM_REQUIRED_PROCESSING_CAPACITY,
                        MAXIMUM_LICENSED_PROCESSING_CAPACITY,
                        MINIMUM_PROCESSING_CAPACITY,
                        MAXIMUM_PROCESSING_CAPACITY,
                        PROCESSING_CAPACITY_INCREMENT,
                        MINIMUM_INTERACTIVE_CAPACITY,
                        MAXIMUM_INTERACTIVE_CAPACITY,
                        DEFINED_VARIABLE_CAPACITY_WEIGHT,
                        VARIABLE_CAPACITY_WEIGHT,
                        UNALLOCATED_VARIABLE_CAPACITY_WEIGHT,
                        HARDWARE_MULTITHREADING,
                        BOUND_HARDWARE_THREADS,
                        THREADS_PER_PROCESSOR,
                        DISPATCH_LATENCY,
                        DISPATCH_WHEEL_ROTATION_TIME,
                        TOTAL_CPU_TIME,
                        INTERACTIVE_CPU_TIME,
                        INTERACTIVE_CPU_TIME_ABOVE_THRESHOLD 
                    FROM TABLE (QSYS2.SYSTEM_STATUS())
                    ";
                var systemInfos = _conn.Query<dynamic>(serviceSql).ToList();
                return systemInfos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des informations systemes : {ex.Message}");
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
