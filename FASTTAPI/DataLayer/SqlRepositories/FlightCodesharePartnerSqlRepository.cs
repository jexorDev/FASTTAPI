using Microsoft.Data.SqlClient;

namespace FASTTAPI.DataLayer.SqlRepositories
{
    public class FlightCodesharePartnerSqlRepository
    {
        public List<string> GetCodesharePartners(int flightPk, SqlConnection conn)
        {
            var partners = new List<string>();
            string sql = @"
SELECT 
 SUBSTRING(CodeshareID, 1, 2) AS CodeshareID
FROM FlightCodesharePartners
WHERE FlightPK = @FlightPK
";
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@FlightPK", flightPk);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        partners.Add(reader["CodeshareID"].ToString());
                    }
                }
            }

            return partners;
        }

        public void InsertCodesharePartner(SqlConnection conn, SqlTransaction trans, int flightPk, string codesharePartner)
        {
            string sql = @"
INSERT INTO FlightCodesharePartners 
(
 FlightPK
,CodeshareID
)
VALUES
(
 @FlightPK
,@CodeshareID
);";
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.Transaction = trans;
                command.Parameters.AddWithValue("@FlightPK", flightPk);
                command.Parameters.AddWithValue("@CodeshareID", codesharePartner);

                command.ExecuteNonQuery();
            }
        }
    }
}
