using Aumerial.Data.Nti;

namespace CLcommandAPI.DataBase
{
    public class DbConnectionService
    {

        public NTiConnection conn;

        public DbConnectionService(IConfiguration config)
        {
            string connectionString = Environment.GetEnvironmentVariable("CSTR")!;
            conn = new NTiConnection(connectionString);
        }
    }
}
