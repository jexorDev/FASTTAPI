namespace FASTTAPI.Models.Responses.Alerts
{
    public class AirlineAlertModel
    {
        public string Airline { get; set; }
        public string AirlineCode { get; set; }
        public bool Disposition { get; set; }
        public string FlightNumber {get; set; }
        public string CityAirportCode { get; set; } 
        public string CityAirportName { get; set; }
        public DateTime ExpectedDateTime { get; set; }
    }
}
