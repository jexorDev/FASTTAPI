using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class FlightCodesharePartnersPostgresSqlRepository
    {
        public List<string> GetCodesharePartners(int flightPk, NpgsqlConnection conn)
        {
            var partners = new List<string>();
            string sql = @"
SELECT 
 SUBSTRING(codeshare_id, 1, 2) AS CodeshareID
FROM flight_codeshare_partners
WHERE flight_pk = @flight_pk
";
            using (var command = new NpgsqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@flight_pk", flightPk);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        partners.Add(reader["CodeshareID"].ToString());
                    }
                }
            }

            return partners;
        }

        public void InsertCodesharePartner(NpgsqlConnection conn, NpgsqlTransaction trans, int flightPk, string codesharePartner)
        {
            string sql = @"
INSERT INTO flight_codeshare_partners 
(
 flight_pk
,codeshare_id
)
VALUES
(
 @FlightPK
,@CodeshareID
);";
            using (var command = new NpgsqlCommand(sql, conn))
            {
                command.Transaction = trans;
                command.Parameters.AddWithValue("@FlightPK", flightPk);
                command.Parameters.AddWithValue("@CodeshareID", codesharePartner);

                command.ExecuteNonQuery();
            }
        }
    }
}
