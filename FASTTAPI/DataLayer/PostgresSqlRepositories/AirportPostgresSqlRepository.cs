using FASTTAPI.DataLayer.DataTransferObjects;
using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class AirportPostgresSqlRepository
    {
        public List<Airport> GetAirports(NpgsqlConnection conn)
        {
            var airports = new List<Airport>();
            string sql = @"
SELECT 
 code
,name
,city_name
FROM airports
";
            using (var command = new NpgsqlCommand(sql, conn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        airports.Add(new Airport
                        {
                            Code = reader["code"].ToString(),
                            Name = reader["name"].ToString(),
                            CityName = reader["city_name"].ToString()
                        });
                    }
                }
            }

            return airports;
        }

        public void InsertAirport(NpgsqlConnection conn, NpgsqlTransaction trans, Airport airport)
        {
            string sql = @"
INSERT INTO airports 
(
 code
,name
,city_name
)
VALUES
(
 @code
,@name
,@city_name
);";
            using (var command = new NpgsqlCommand(sql, conn))
            {
                command.Transaction = trans;

                command.Parameters.AddWithValue("@code", airport.Code);
                command.Parameters.AddWithValue("@name", airport.Name);
                command.Parameters.AddWithValue("@city_name", airport.CityName);

                command.ExecuteNonQuery();
            }
        }
    }
}
