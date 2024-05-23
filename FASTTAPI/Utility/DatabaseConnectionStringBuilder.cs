using  Microsoft.Data.SqlClient;

namespace FASTTAPI.Utility
{
    public class DatabaseConnectionStringBuilder
    {
        private static SqlConnectionStringBuilder? _sqlConnectionStringBuilder;
        public static string GetSqlConnectionString(IConfiguration config)
        {
            if (_sqlConnectionStringBuilder == null)
            {
                _sqlConnectionStringBuilder = new SqlConnectionStringBuilder();

                _sqlConnectionStringBuilder.Encrypt = true;

                _sqlConnectionStringBuilder.DataSource = config["FASTTDatabaseConnection_Server"];
                _sqlConnectionStringBuilder.UserID = config["FASTTDatabaseConnection_Username"];
                _sqlConnectionStringBuilder.Password = config["FASTTDatabaseConnection_Password"];
                _sqlConnectionStringBuilder.InitialCatalog = config["FASTTDatabaseConnection_Database"];
            }

            return _sqlConnectionStringBuilder.ToString();
        }
    }
}
