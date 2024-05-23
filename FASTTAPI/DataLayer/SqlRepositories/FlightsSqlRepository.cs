using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.Enumerations;
using Microsoft.Data.SqlClient;

namespace FASTTAPI.DataLayer.SqlRepositories
{
    public class FlightsSqlRepository
    {
        public List<Flight> GetFlights(
            SqlConnection conn, 
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
 PK
,Disposition
,FlightNumber
,Airline
,DateTimeScheduled
,DateTimeEstimated
,DateTimeActual
,Gate
,CityName
,CityAirportName
,CityAirportCode
,DateTimeCreated
,DateTimeUpdated
FROM Flights
";
            var filterString = string.Empty;
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@FromDate",fromDate);
                command.Parameters.AddWithValue("@ToDate", toDate);
                filterString = "WHERE DateTimeScheduled BETWEEN @FromDate AND @ToDate ";

                if (Disposition.Type.ScheduledArriving.Equals(disposition) || Disposition.Type.Arrived.Equals(disposition))
                {
                    command.Parameters.AddWithValue("@Disposition", 1);
                    filterString += "AND Disposition = @Disposition ";
                }
                else if (Disposition.Type.ScheduledDepartures.Equals(disposition) || Disposition.Type.Departed.Equals(disposition))
                {
                    command.Parameters.AddWithValue("@Disposition", 0);
                    filterString += "AND Disposition = @Disposition ";
                }

                if (!string.IsNullOrWhiteSpace(flightNumber))
                {
                    command.Parameters.AddWithValue("@FlightNumber", flightNumber);
                    filterString += "AND FlightNumber = @FlightNumber ";
                }

                if (!string.IsNullOrWhiteSpace(airline))
                {
                    command.Parameters.AddWithValue("@Airline", airline);
                    filterString += "AND (Airline = @Airline ";

                    if (includeCodesharePartners)
                    {
                        command.Parameters.AddWithValue("@CodeshareID", airline);
                        filterString += @"
    OR EXISTS (SELECT *
               FROM FlightCodeSharePartners
               WHERE SUBSTRING(CodeshareID, 1, 2) = @CodeshareID
                 AND FlightPK = Flights.PK
              ) ";

                    }

                    filterString += ") ";
                }

                if (!string.IsNullOrWhiteSpace(city))
                {
                    command.Parameters.AddWithValue("@CityAirportCode", city);
                    filterString += "AND CityAirportCode = @CityAirportCode ";
                }

                command.CommandText += filterString;

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var flight = new Flight
                        {
                            Pk = int.Parse(reader["PK"].ToString()),
                            Disposition = bool.Parse(reader["Disposition"].ToString()),
                            FlightNumber = reader["FlightNumber"].ToString().Trim(),
                            Airline = reader["Airline"].ToString().Trim(),
                            Gate = reader["Gate"].ToString(),
                            CityName = reader["CityName"].ToString(),
                            CityAirportName = reader["CityAirportName"].ToString(),
                            CityAirportCode = reader["CityAirportCode"].ToString(),
                            DateTimeCreated = DateTime.Parse(reader["DateTimeCreated"].ToString())                            
                        };

                        DateTime parsedTime;

                        if (DateTime.TryParse(reader["DateTimeScheduled"].ToString(), out parsedTime))
                        {
                            //TODO: Why is the to local time needed when it's not when pulling directly from the FA API?
                            flight.DateTimeScheduled = parsedTime.ToLocalTime();
                        }
                        if (DateTime.TryParse(reader["DateTimeEstimated"].ToString(), out parsedTime))
                        {
                            flight.DateTimeEstimated = parsedTime.ToLocalTime();
                        }
                        if (DateTime.TryParse(reader["DateTimeActual"].ToString(), out parsedTime))
                        {
                            flight.DateTimeActual = parsedTime.ToLocalTime();
                        }
                        if (DateTime.TryParse(reader["DateTimeUpdated"].ToString(), out parsedTime))
                        {
                            flight.DateTimeModified = parsedTime;
                        }

                        flights.Add(flight);
                    }
                }
            }

            return flights;
        }

        public int InsertFlight(Flight flight, SqlConnection conn, SqlTransaction trans)
        {
            string sql = @"
INSERT INTO Flights 
(
 Disposition
,FlightNumber
,Airline
,DateTimeScheduled
,DateTimeEstimated
,DateTimeActual
,Gate
,CityName
,CityAirportName
,CityAirportCode
,DateTimeCreated
)
VALUES
(
 @Disposition
,@FlightNumber
,@Airline
,@DateTimeScheduled
,@DateTimeEstimated
,@DateTimeActual
,@Gate
,@CityName
,@CityAirportName
,@CityAirportCode
,@DateTimeCreated
);
SELECT SCOPE_IDENTITY();
";
            using (SqlCommand command = new SqlCommand(sql, conn))
            {
                command.Transaction = trans;
                command.Parameters.AddWithValue("@Disposition", flight.Disposition);
                command.Parameters.AddWithValue("@FlightNumber", flight.FlightNumber ?? "");
                command.Parameters.AddWithValue("@Airline", flight.Airline ?? "");
                if (flight.DateTimeScheduled.HasValue)
                {
                    command.Parameters.AddWithValue("@DateTimeScheduled", flight.DateTimeScheduled.Value);

                }
                else
                {
                    command.Parameters.AddWithValue("@DateTimeScheduled", DBNull.Value);

                }

                if (flight.DateTimeEstimated.HasValue)
                {
                    command.Parameters.AddWithValue("@DateTimeEstimated", flight.DateTimeEstimated.Value);

                }
                else
                {
                    command.Parameters.AddWithValue("@DateTimeEstimated", DBNull.Value);

                }

                if (flight.DateTimeActual.HasValue)
                {
                    command.Parameters.AddWithValue("@DateTimeActual", flight.DateTimeActual.Value);

                }
                else
                {
                    command.Parameters.AddWithValue("@DateTimeActual", DBNull.Value);

                }
                command.Parameters.AddWithValue("@Gate", flight.Gate ?? "");
                command.Parameters.AddWithValue("@CityName", flight.CityName ?? "");
                command.Parameters.AddWithValue("@CityAirportName", flight.CityAirportName ?? "");
                command.Parameters.AddWithValue("@CityAirportCode", flight.CityAirportCode ?? "");
                command.Parameters.AddWithValue("@DateTimeCreated", DateTime.UtcNow);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }       
    }
}
