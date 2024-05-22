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

                _sqlConnectionStringBuilder.DataSource = config["FASTTAPIAPIDatabaseConnection_Server"];
                _sqlConnectionStringBuilder.UserID = config["FASTTAPIAPIDatabaseConnection_Username"];
                _sqlConnectionStringBuilder.Password = config["FASTTAPIAPIDatabaseConnection_Password"];
                _sqlConnectionStringBuilder.InitialCatalog = config["FASTTAPIAPIDatabaseConnection_Database"];
            }

            return _sqlConnectionStringBuilder.ToString();
        }
    }
}
