using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.Enumerations;
using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class FlightsPostgresSqlRepository
    {
        public List<Flight> GetFlights(
          NpgsqlConnection conn,
          Disposition.Type disposition,
          DateTime fromDate,
          DateTime toDate,
          string flightNumber,
          string airline,
          string city,
          bool includeCodesharePartners)
        {
            var flights = new List<Flight>();
            string sql = @"
SELECT 
 pk
,disposition
,flight_number
,airline
,status
,scheduled
,estimated
,actual
,airport_gate
,airport_code
,created
,stale
,airports.name as airport_name
,airports.city_name as airport_city_name
FROM flights
LEFT OUTER JOIN 
 airports
ON
 airports.code = flights.airport_code
";
            var filterString = string.Empty;
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@FromDate", fromDate);
                command.Parameters.AddWithValue("@ToDate", toDate);
                filterString = "WHERE scheduled BETWEEN @FromDate AND @ToDate AND stale = FALSE ";

                if (Disposition.Type.ScheduledArriving.Equals(disposition) || Disposition.Type.Arrived.Equals(disposition))
                {
                    command.Parameters.AddWithValue("@disposition", true);
                    filterString += "AND disposition = @disposition ";
                }
                else if (Disposition.Type.ScheduledDepartures.Equals(disposition) || Disposition.Type.Departed.Equals(disposition))
                {
                    command.Parameters.AddWithValue("@disposition", false);
                    filterString += "AND disposition = @disposition ";
                }

                if (!string.IsNullOrWhiteSpace(flightNumber))
                {
                    command.Parameters.AddWithValue("@flight_number", flightNumber);
                    filterString += "AND flight_number = @flight_number ";
                }

                if (!string.IsNullOrWhiteSpace(airline))
                {
                    command.Parameters.AddWithValue("@airline", airline);
                    filterString += "AND (airline = @airline ";

                    if (includeCodesharePartners)
                    {
                        filterString += @"
    OR EXISTS (SELECT *
               FROM flight_codeshare_partners
               WHERE SUBSTRING(flight_codeshare_partners.codeshare_id, 1, 2) = @airline
                 AND flight_codeshare_partners.flight_pk = flights.pk
              ) ";

                    }

                    filterString += ") ";
                }

                if (!string.IsNullOrWhiteSpace(city))
                {
                    command.Parameters.AddWithValue("@CityAirportCode", city);
                    filterString += "AND airport_code = @CityAirportCode ";
                }

                command.CommandText += filterString;
                command.CommandText += " ORDER BY scheduled ASC ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var flight = new Flight
                        {
                            Pk = int.Parse(reader["PK"].ToString()),
                            Disposition = bool.Parse(reader["disposition"].ToString()),
                            FlightNumber = reader["flight_number"].ToString().Trim(),
                            Status = reader["status"].ToString().Trim(),
                            Airline = reader["airline"].ToString().Trim(),
                            Gate = reader["airport_gate"].ToString(),
                            CityName = reader["airport_city_name"].ToString(),
                            CityAirportName = reader["airport_name"].ToString(),
                            CityAirportCode = reader["airport_code"].ToString(),
                            DateTimeCreated = DateTime.Parse(reader["created"].ToString())
                        };

                        DateTime parsedTime;

                        if (DateTime.TryParse(reader["scheduled"].ToString(), out parsedTime))
                        {
                            //TODO: Why is the to local time needed when it's not when pulling directly from the FA API?
                            flight.DateTimeScheduled = parsedTime.ToLocalTime();
                        }
                        if (DateTime.TryParse(reader["estimated"].ToString(), out parsedTime))
                        {
                            flight.DateTimeEstimated = parsedTime.ToLocalTime();
                        }
                        if (DateTime.TryParse(reader["actual"].ToString(), out parsedTime))
                        {
                            flight.DateTimeActual = parsedTime.ToLocalTime();
                        }

                        flights.Add(flight);
                    }
                }
            }

            return flights;
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
