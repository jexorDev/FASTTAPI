using FASTTAPI.DataLayer.DataTransferObjects;

namespace FASTTAPI.Utility
{
    public class AirlineFinder
    {
        public static Airline? FindAirline(string keyword, List<Airline> airlines)
        {
            keyword = keyword.Trim();

            foreach (Airline airline in airlines)
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
