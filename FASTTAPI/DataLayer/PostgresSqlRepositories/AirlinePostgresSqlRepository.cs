using FASTTAPI.DataLayer.DataTransferObjects;
using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class AirlinePostgresSqlRepository
    {
        public List<Airline> GetAirlines(NpgsqlConnection conn)
        {
            var airlines = new List<Airline>();

            const string sql = @"
SELECT
     icao_code
    ,iata_code
    ,name
    ,hide
FROM
    airlines
ORDER BY
    iata_code
";
            using (var cmd = new NpgsqlCommand(sql, conn))
            { 
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        airlines.Add(new Airline
                        {
                            IataCode = reader["iata_code"].ToString(),
                            IcaoCode = reader["icao_code"].ToString(),
                            Name = reader["name"].ToString(),
                            Hide = bool.Parse(reader["hide"].ToString())
                        });
                    }

                }
            }

            return airlines;
        }

        public void InsertAirline(Airline airline, NpgsqlTransaction trans, NpgsqlConnection conn)
        {
            const string sql = @"
INSERT INTO
    airlines
(
     icao_code
    ,iata_code
    ,name
    ,hide
)
VALUES
(
     @icao_code
    ,@iata_code
    ,@name
    ,@hide
)";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Transaction = trans;

                cmd.Parameters.AddWithValue("@icao_code", airline.IcaoCode);
                cmd.Parameters.AddWithValue("@iata_code", airline.IataCode);
                cmd.Parameters.AddWithValue("@name", airline.Name);
                cmd.Parameters.AddWithValue("@hide", airline.Hide);

                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateAirline(Airline airline, NpgsqlConnection conn)
        {
            const string sql = @"
UPDATE
    airlines
SET
     icao_code = @icao_code    
    ,name = @name
    ,hide = @hide
WHERE
    iata_code = @iata_code
";
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@icao_code", airline.IcaoCode);
                cmd.Parameters.AddWithValue("@iata_code", airline.IataCode);
                cmd.Parameters.AddWithValue("@name", airline.Name);
                cmd.Parameters.AddWithValue("@hide", airline.Hide);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
