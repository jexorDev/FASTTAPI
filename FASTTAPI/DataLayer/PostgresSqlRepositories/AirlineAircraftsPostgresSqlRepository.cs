using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.Enumerations;
using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class AirlineAircraftsPostgresSqlRepository
    {
        public void PopulateFromFlights(NpgsqlConnection conn)
        {
            string sql = @"
INSERT INTO 
    airline_aircraft
(
    airline
    ,aircraft_type
)
SELECT 
    f.airline
    ,f.aircraft_type
FROM
    flights f
WHERE 
    aircraft_type IS NOT NULL
AND
    NOT EXISTS(SELECT
                *
               FROM
                airline_aircraft aa
                WHERE 
                aa.airline = f.airline 
                AND
                aa.aircraft_type = f.aircraft_type
                )
GROUP BY
    airline
    ,aircraft_type
";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.ExecuteNonQuery();
            }
        }

        public void UpdateAirlineAircraft(AirlineAircraft airlineAircraft, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string sql = @"
UPDATE 
    airline_aircraft
SET
    pax_count = @pax_count
WHERE
    airline = @airline
AND
    aircraft_type = @aircraft_type
";

            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@pax_count", airlineAircraft.SeatCount);
                command.Parameters.AddWithValue("@airline", airlineAircraft.AirlineId);
                command.Parameters.AddWithValue("@aircraft_type", airlineAircraft.AircraftType);

                command.ExecuteNonQuery();
            }
        }

        public List<AirlineAircraft> GetAirlineAircrafts(NpgsqlConnection conn)
        {
            List<AirlineAircraft> airlineAircrafts = new List<AirlineAircraft>();

            string sql = @"
SELECT
    airline
    ,aircraft_type
    ,pax_count
FROM
    airline_aircraft
";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        airlineAircrafts.Add(new AirlineAircraft
                        {
                            AirlineId = reader["airline"].ToString(),
                            AircraftType = reader["aircraft_type"].ToString(),
                            SeatCount = int.Parse(reader["pax_count"].ToString())
                        });
                    }
                }
                
            }

            return airlineAircrafts;
        }
    }
}
