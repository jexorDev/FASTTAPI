using FASTTAPI.DataLayer.DataTransferObjects;
using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class FailedFlightsPostgresSqlRepository
    {
        public int InsertFailedFlight(string rawFlightData, string error, NpgsqlConnection conn)
        {
            string sql = @"
INSERT INTO failed_flights
(
 attempt_timestamp
,raw_flight_data
,error
,fixed
)
VALUES
(
 CURRENT_TIMESTAMP
,@raw_flight_data
,@error
,FALSE
);
";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@raw_flight_data", rawFlightData);
                command.Parameters.AddWithValue("@error", error);                
                                
                return command.ExecuteNonQuery();
            }
        }

        public List<FailedFlight> GetFailedFlights(NpgsqlConnection conn)
        {
            List<FailedFlight> failedFlights = new List<FailedFlight>();    

            string sql = @"
SELECT
    attempt_timestamp
    ,raw_flight_data
    ,error
    ,fixed
FROM
    failed_flights
";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        failedFlights.Add(new FailedFlight
                        {
                            Timestamp = DateTime.Parse(reader["attempt_timestamp"].ToString()),
                            SerializedFlightInfo = reader["raw_flight_data"].ToString(),
                            ErrorMessage = reader["error"].ToString(),
                            IsFixed = bool.Parse(reader["fixed"].ToString())
                        });
                    }
                }
            }

            return failedFlights;
        }

    }
}
