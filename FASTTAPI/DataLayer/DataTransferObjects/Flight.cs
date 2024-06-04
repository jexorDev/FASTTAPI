namespace FASTTAPI.DataLayer.DataTransferObjects
{
    public class Flight
    {
        public int Pk { get; set; }        
        public bool Disposition { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Airline { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DateTimeScheduled { get; set; } 
        public DateTime? DateTimeEstimated { get; set; } 
        public DateTime? DateTimeActual { get; set; }
        public string Gate { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string CityAirportName { get; set; } = string.Empty;
        public string CityAirportCode { get; set; } = string.Empty;
        public DateTime DateTimeCreated { get; set; }
        public DateTime? DateTimeModified { get; set; }
        public bool HasCodesharePartners { get; set; }
    }
}
