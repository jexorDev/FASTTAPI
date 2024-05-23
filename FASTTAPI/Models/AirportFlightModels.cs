namespace FASTTAPI.Models
{
    public enum DispositionType
    {
        Arrival,
        Departure
    }

    public class BaseAirportFlightModel
    {
        public DispositionType Disposition { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AirlineIdentifier { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public List<string> CodesharePartners { get; set; } = new List<string>();
        public string FlightNumber { get; set; } = string.Empty;
        public string AirportGate { get; set; } = string.Empty;
        public DateTime? ScheduledDepartureTime { get; set; }
        public DateTime? EstimatedDepartureTime { get; set; }
        public DateTime? ActualDepartureTime { get; set; }
        public DateTime? ScheduledArrivalTime { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        public DateTime? ActualArrivalTime { get; set; }
        public string CityCode { get; set; } = string.Empty;    
        public string CityName { get; set; } = string.Empty;
        public string CityAirportName { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }

    }

    
}
