using FASTTAPI.Models;

namespace FASTTAPI.Controllers
{
    public class GetResponseBody
    {
        public string NextDataPageUrl { get; set; } = string.Empty;
        public List<BaseAirportFlightModel> Results { get; set; } = new List<BaseAirportFlightModel>();
        public string GeneratedApiQuery { get; set; } = string.Empty;
        public string RawData { get; set; } = string.Empty;
    }
}
