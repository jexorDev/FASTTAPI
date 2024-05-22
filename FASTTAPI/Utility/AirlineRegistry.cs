using FASTTAPI.Models;
using Newtonsoft.Json;
using System.Reflection;

namespace FASTTAPI.Utility
{
    public class AirlineRegistry
    {
        private static List<Airline> _airlines;

        public static List<Airline> GetAirlines()
        {
            if (_airlines == null)
            {
                using (StreamReader reader = new StreamReader(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"DataLayer\Static\AirlineRegistry.json")))
                {
                    _airlines = JsonConvert.DeserializeObject<List<Airline>>(reader.ReadToEnd());
                }
            }

            return _airlines ?? new List<Airline>();
        }

        public static Airline? FindAirline(string keyword)
        {
            keyword = keyword.Trim();

            foreach (Airline airline in GetAirlines())
            {
                if (keyword.Length == 2)
                {
                    if (string.Compare(keyword, airline.IataCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return airline;
                    }
                    continue;
                }

                if (keyword.Length == 3)
                {
                    if (string.Compare(keyword, airline.IcaoCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return airline;
                    }
                    continue;
                }

                if (string.Compare(keyword.Replace(" ", ""), airline.Name.Replace(" ", ""),StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return airline;
                }
            }

            return null;
        }
    }
}
