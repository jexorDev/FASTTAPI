using FASTTAPI.DataLayer.DataTransferObjects;
using Npgsql;

namespace FASTTAPI.DataLayer.PostgresSqlRepositories
{
    public class StatisticsPostgresSqlRepository
    {
        public List<PaxVolumeHour> GetHourlyPassengerVolume(
            DateTime fromDate,
            DateTime toDate,
            NpgsqlConnection conn)
        {
            List<PaxVolumeHour> hourlyPassengerVolumeList = new List<PaxVolumeHour>();

            for (int i = 0; i < 24; i++)
            {
                hourlyPassengerVolumeList.Add(new PaxVolumeHour
                {
                    Hour = i
                });
            }

            string sql = @"
select
extract(hour from f.scheduled) hourutc 
,f.disposition 
	,count(*) flight_count
	,sum(aa.pax_count) pax_count
from flights f
left outer join airline_aircraft aa 
on
	f.aircraft_type  = aa.aircraft_type 
and 	
	f.airline = aa.airline
WHERE f.scheduled BETWEEN @FromDate AND @ToDate 
and 
	f.stale = false 
group by  
extract(hour from f.scheduled) 
,f.disposition ";

            using (NpgsqlCommand command = new NpgsqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@FromDate", fromDate);
                command.Parameters.AddWithValue("@ToDate", toDate);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var currentHour = int.Parse(reader["hourutc"].ToString());
                        var currentHourPassengerVolume = hourlyPassengerVolumeList.FirstOrDefault(h => h.Hour == currentHour);
                        if (currentHourPassengerVolume == null) continue;
                        
                        if (bool.Parse(reader["disposition"].ToString()))
                        {
                            currentHourPassengerVolume.ArrivingFlights = int.Parse(reader["flight_count"].ToString());
                            currentHourPassengerVolume.ArrivingPassengers = int.Parse(reader["pax_count"].ToString());
                        }
                        else
                        {
                            currentHourPassengerVolume.DepartingFlights = int.Parse(reader["flight_count"].ToString());
                            currentHourPassengerVolume.DepartingPassengers = int.Parse(reader["pax_count"].ToString());
                        }
                    }
                }
            }

            return hourlyPassengerVolumeList;
        }
    }
}
