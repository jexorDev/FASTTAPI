namespace FASTTAPI.Utility
{
    public class FlightAwareApi
    {
        public const string BaseUri = "https://aeroapi.flightaware.com/aeroapi";

        public static string BuildFlightAwareQueryString(
           DateTime start,
           DateTime end,
           string airline)
        {
            var queryStringList = new List<string>();

            queryStringList.Add($"start={start:s}");
            queryStringList.Add($"end={end:s}");
            

            if (!string.IsNullOrWhiteSpace(airline))
            {
                queryStringList.Add($"airline={airline}");
            }
            else
            {
                queryStringList.Add("type=Airline");
            }

            return string.Join("&", queryStringList);

        }

        public static string GetAirlineWithCodesharePartners(string airline, List<string> codesharePartners)
        {
            try
            {
                var convertedAirline = AirlineRegistry.FindAirline(airline)?.Name;
                var convertedCodesharePartners = new List<string>();

                foreach (var codeshareParter in codesharePartners)
                {
                    var convertedCodesharePartner = AirlineRegistry.FindAirline(codeshareParter)?.Name;
                    if (!string.IsNullOrWhiteSpace(convertedCodesharePartner))
                    {
                        convertedCodesharePartners.Add(convertedCodesharePartner);
                    }
                }

                return String.Join(" | ", new List<string>() { convertedAirline ?? "" }.Concat(convertedCodesharePartners));

            }
            catch
            {
                return string.Empty;
            }

        }
    }
}
