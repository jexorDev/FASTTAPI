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

        public int InsertFlight(Flight flight, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            string sql = @"
UPDATE flights
 SET stale = true
WHERE 
 flight_number = @flight_number
AND 
 airline = @airline
AND
 disposition = @disposition
AND
 scheduled::DATE = @scheduled::DATE;

INSERT INTO flights
(
 disposition
,flight_number
,airline
,status
,scheduled
,estimated
,actual
,airport_gate
,airport_code
,codeshares
,stale
,aircraft_type
)
VALUES
(
 @disposition
,@flight_number
,@airline
,@status
,@scheduled
,@estimated
,@actual
,@airport_gate
,@airport_code
,@codeshares
,FALSE
,@aircraft_type
) RETURNING pk;
";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Transaction = trans;
                command.Parameters.AddWithValue("@disposition", flight.Disposition);
                command.Parameters.AddWithValue("@flight_number", flight.FlightNumber ?? "");
                command.Parameters.AddWithValue("@airline", flight.Airline ?? "");
                command.Parameters.AddWithValue("@status", flight.Status);
                command.Parameters.AddWithValue("@codeshares", flight.HasCodesharePartners);
                command.Parameters.AddWithValue("@aircraft_type", flight.AircraftType ?? "");

                if (flight.DateTimeScheduled.HasValue)
                {
                    command.Parameters.AddWithValue("@scheduled", flight.DateTimeScheduled.Value);

                }
                else
                {
                    command.Parameters.AddWithValue("@scheduled", DBNull.Value);

                }

                if (flight.DateTimeEstimated.HasValue)
                {
                    command.Parameters.AddWithValue("@estimated", flight.DateTimeEstimated.Value);

                }
                else
                {
                    command.Parameters.AddWithValue("@estimated", DBNull.Value);

                }

                if (flight.DateTimeActual.HasValue)
                {
                    command.Parameters.AddWithValue("@actual", flight.DateTimeActual.Value);

                }
                else
                {
                    command.Parameters.AddWithValue("@actual", DBNull.Value);

                }
                command.Parameters.AddWithValue("@airport_gate", flight.Gate ?? "");
                command.Parameters.AddWithValue("@airport_code", flight.CityAirportCode?? "");
                                
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
    }
}
