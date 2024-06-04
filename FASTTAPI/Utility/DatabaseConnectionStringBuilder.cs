using  Microsoft.Data.SqlClient;

namespace FASTTAPI.Utility
{
    public class DatabaseConnectionStringBuilder
    {
        private static SqlConnectionStringBuilder? _sqlConnectionStringBuilder;
        public static string GetSqlConnectionString(IConfiguration config)
        {
            return $"Host={config["FASTTDatabaseConnection_Server"]};Username={config["FASTTDatabaseConnection_Username"]};Password={config["FASTTDatabaseConnection_Password"]};Database={config["FASTTDatabaseConnection_Database"]}";
        }
    }
}
