using FASTTAPI.DataLayer.DataTransferObjects;

namespace FASTTAPI.Utility
{
    public class AirportFinder
    {
        public static List<Airport> FindAirports(List<Airport> airports, string searchTerm)
        {
            List<Airport> matchingAirports = new List<Airport>();

            if (searchTerm.Trim().Length == 3)
            {
                Airport? airport = airports.First(airport => string.Compare(airport.Code, searchTerm, true) == 0);
                if (airport != null)
                {
                    matchingAirports.Add(airport);
                }

                return matchingAirports;
            }

            matchingAirports.AddRange(airports.Where(airport => string.Compare(airport.Name, searchTerm, true) == 0));
            matchingAirports.AddRange(airports.Where(airport => string.Compare(airport.CityName, searchTerm, true) == 0));

            return matchingAirports;
        }

        public static Airport? FindAirport(List<Airport> airports, string searchTerm)
        {
            return FindAirports(airports, searchTerm).FirstOrDefault();
        }
    }
}
