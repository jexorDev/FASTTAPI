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
,0
);
";
            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@raw_flight_data", rawFlightData);
                command.Parameters.AddWithValue("@error", error);                
                                
                return command.ExecuteNonQuery();
            }
        }
    }
}
