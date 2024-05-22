using FASTTAPI.Enumerations;

namespace FASTTAPI.Controllers
{
    public class GetAirportFlightsRequestParameters
    {
        public string? Airline { get; set; }
        public string? Airport { get; set; }
        public Disposition.Type DispositionType { get; set; } = Disposition.Type.None;
        public DateTime DateTimeFrom { get; set; }
        public DateTime DateTimeTo { get; set; }
        public string? NextDataPageUrl { get; set; }
    }
}
