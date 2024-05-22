using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.SqlRepositories;
using FASTTAPI.Enumerations;
using FASTTAPI.Models;
using FASTTAPI.Models.FlightAware;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("RawFlightData")]
    public class RawFlightDataController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<RawFlightDataController> _logger;
        private readonly FlightsSqlRepository _flightSqlRepository;

        public RawFlightDataController(ILogger<RawFlightDataController> logger, IConfiguration config)
        {
            _configuration = config;
            _logger = logger;
            _flightSqlRepository = new FlightsSqlRepository();
        }

        [HttpGet]
        public async Task<List<BaseAirportFlightModel>> Get([FromQuery] Disposition.Type dispositionType, DateTime fromDateTime, DateTime toDateTime, string? airline, string? city, bool? includeCodesharePartners)
        {
            Utility.AirlineRegistry.GetAirlines();
            var flights = new List<BaseAirportFlightModel>();

            var airlineCode = string.Empty;

            if (!string.IsNullOrWhiteSpace(airline))
            {
                var convertedAirline = AirlineRegistry.FindAirline(airline);
                if (convertedAirline != null)
                {
                    airlineCode = convertedAirline.IataCode;
                }
            }

            using (SqlConnection connection = new SqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();

                foreach (var flight in _flightSqlRepository.GetFlights(connection, dispositionType, fromDateTime, toDateTime, airlineCode, city ?? "", includeCodesharePartners ?? false))
                {
                    //TODO: make shared code for this
                    var flightModel = new BaseAirportFlightModel
                    {
                        FlightNumber = flight.FlightNumber,
                        AirportGate = flight.Gate,
                        ScheduledArrivalTime = flight.DateTimeScheduled,
                        ScheduledDepartureTime = flight.DateTimeScheduled,
                        EstimatedArrivalTime = flight.DateTimeEstimated,
                        EstimatedDepartureTime = flight.DateTimeEstimated,
                        ActualArrivalTime = flight.DateTimeActual,
                        ActualDepartureTime = flight.DateTimeActual,
                        CityName = flight.CityName,
                        CityCode = flight.CityAirportCode,
                        CityAirportName = flight.CityAirportName
                    };

                    var flightAirline = AirlineRegistry.FindAirline(flight.Airline);

                    if (flightAirline != null)
                    {
                        flightModel.AirlineName = flightAirline.Name;
                        flightModel.AirlineIdentifier = flightAirline.IcaoCode;
                    }
                    else
                    {
                        flightModel.AirlineName = flight.Airline;
                        flightModel.AirlineIdentifier = flight.Airline;
                    }

                    flights.Add(flightModel);

                }

                connection.Close();
            }

            return flights;
        }

        [HttpPost]
        public async Task<string> Populate(
            string adminPassword,
            bool forToday, 
            bool forTomorrow, 
            bool arrived, 
            bool scheduledArrivals, 
            bool departed, 
            bool scheduledDeparted)
        {
            if (string.Compare(adminPassword, _configuration["AdminPassword"].ToString(), false) != 0) return "Invalid password";

            var status = "";

            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
                {
                    connection.Open();

                    DateTime fromDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0).ToUniversalTime();
                    DateTime toDateTime = fromDateTime.AddHours(24).AddSeconds(-1);

                    if (forToday)
                    {
                        status = "populating tables for today";
                        await PopulateFlightTables(fromDateTime, toDateTime, arrived, scheduledArrivals, departed, scheduledDeparted, connection);
                    }

                    if (forTomorrow)
                    {
                        status = "populating tables for tomorrow";

                        fromDateTime = fromDateTime.AddDays(1); 
                        toDateTime = toDateTime.AddDays(1);

                        await PopulateFlightTables(fromDateTime, toDateTime, arrived, scheduledArrivals, departed, scheduledDeparted, connection);
                    }

                    connection.Close();
                }

                status = "Successfully completed populating tables";

            }
            catch (Exception ex)
            {
                status = $"ERROR while {status}: {ex.Message}";
            }

            return status;
        }

        private async Task PopulateFlightTables(
            DateTime fromDateTime, 
            DateTime toDateTime, 
            bool arrived, 
            bool scheduledArrivals, 
            bool departed, 
            bool scheduledDeparted,
            SqlConnection connection)
        {
            await PopulateFlightTable(fromDateTime, toDateTime, "arrivals", connection);
            Thread.Sleep(60000);
            await PopulateFlightTable(fromDateTime, toDateTime, "scheduled_arrivals", connection);
            Thread.Sleep(60000);
            await PopulateFlightTable(fromDateTime, toDateTime, "departures", connection);
            Thread.Sleep(60000);
            await PopulateFlightTable(fromDateTime, toDateTime, "scheduled_departures", connection);
        }

        private async Task PopulateFlightTable(
            DateTime fromDateTime, 
            DateTime toDateTime, 
            string resource, 
            SqlConnection conn)
        {
            using (HttpClient client = new HttpClient())
            {
                int count = 0;
                var restOfUrl = string.Empty;
                var cursor = string.Empty;
                client.DefaultRequestHeaders.Add("X-Apikey", _configuration["FlightAwareKey"]);

                do
                {
                    if (string.IsNullOrWhiteSpace(cursor))
                    {
                        restOfUrl = $"/airports/{_configuration["AirportCode"]}/flights/{resource}?type=Airline&start={DateConversions.GetFormattedISODateTime(fromDateTime)}&end={DateConversions.GetFormattedISODateTime(toDateTime)}";
                    }
                    else
                    {
                        restOfUrl = cursor;
                    }

                    if (count > 9)
                    {
                        Thread.Sleep(60000);
                        count = 0;
                    }
                    var flightAwareResponseObject = await client.GetAsync(FlightAwareApi.BaseUri + restOfUrl);
                    count++;
                    var flightAwareResponseBody = flightAwareResponseObject.Content.ReadAsStringAsync().Result;

                    if (flightAwareResponseBody == null) return;

                    FlightAwareAirportFlightsResponseObject flightAwareResponse = JsonConvert.DeserializeObject<FlightAwareAirportFlightsResponseObject>(flightAwareResponseBody);

                    if (flightAwareResponse != null)
                    {
                        InsertFlights(flightAwareResponse, conn);
                    }

                    cursor = flightAwareResponse?.links?.next;

                } while (!string.IsNullOrWhiteSpace(cursor));
            }
        }

        private void InsertFlights(FlightAwareAirportFlightsResponseObject flightAwareResponse, SqlConnection conn)
        {
            if (flightAwareResponse.arrivals != null)
            {
                foreach (var arrival in flightAwareResponse.arrivals)
                {
                    var flight = new Flight
                    {
                        Disposition = true,
                        FlightNumber = arrival.flight_number,
                        Airline = arrival.operator_iata,
                        DateTimeScheduled = arrival.scheduled_in,
                        DateTimeEstimated = arrival.estimated_in,
                        DateTimeActual = arrival.actual_in,
                        Gate = arrival.gate_destination,
                        CityName = arrival.origin.city,
                        CityAirportCode = arrival.origin.code_iata,
                        CityAirportName = arrival.origin.name,
                        DateTimeCreated = DateTime.UtcNow
                    };

                    var pk  = _flightSqlRepository.InsertFlight(flight, conn);
                    
                    foreach (var codesharePartner in arrival.codeshares_iata)
                    {
                        _flightSqlRepository.InsertCodesharePartner(conn, pk, codesharePartner);
                    }
                }
            }


            if (flightAwareResponse.scheduled_arrivals != null)
            {
                foreach (var arrival in flightAwareResponse.scheduled_arrivals)
                {
                    var flight = new Flight
                    {
                        Disposition = true,
                        FlightNumber = arrival.flight_number,
                        Airline = arrival.operator_iata,
                        DateTimeScheduled = arrival.scheduled_in,
                        DateTimeEstimated = arrival.estimated_in,
                        DateTimeActual = arrival.actual_in,
                        Gate = arrival.gate_destination,
                        CityName = arrival.origin.city,
                        CityAirportCode = arrival.origin.code_iata,
                        CityAirportName = arrival.origin.name,
                        DateTimeCreated = DateTime.UtcNow
                    };

                    var pk = _flightSqlRepository.InsertFlight(flight, conn);

                    foreach (var codesharePartner in arrival.codeshares_iata)
                    {
                        _flightSqlRepository.InsertCodesharePartner(conn, pk, codesharePartner);
                    }
                }
            }

            if (flightAwareResponse.departures != null)
            {
                foreach (var departure in flightAwareResponse.departures)
                {
                    var flight = new Flight
                    {
                        Disposition = false,
                        FlightNumber = departure.flight_number,
                        Airline = departure.operator_iata,
                        DateTimeScheduled = departure.scheduled_out,
                        DateTimeEstimated = departure.estimated_out,
                        DateTimeActual = departure.actual_out,
                        Gate = departure.gate_origin,
                        CityName = departure.destination.city,
                        CityAirportCode = departure.destination.code_iata,
                        CityAirportName = departure.destination.name,
                        DateTimeCreated = DateTime.UtcNow
                    };

                    var pk = _flightSqlRepository.InsertFlight(flight, conn);

                    foreach (var codesharePartner in departure.codeshares_iata)
                    {
                        _flightSqlRepository.InsertCodesharePartner(conn, pk, codesharePartner);
                    }
                }
            }

            if (flightAwareResponse.scheduled_departures != null)
            {
                foreach (var departure in flightAwareResponse.scheduled_departures)
                {
                    var flight = new Flight
                    {
                        Disposition = false,
                        FlightNumber = departure.flight_number,
                        Airline = departure.operator_iata,
                        DateTimeScheduled = departure.scheduled_out,
                        DateTimeEstimated = departure.estimated_out,
                        DateTimeActual = departure.actual_out,
                        Gate = departure.gate_origin,
                        CityName = departure.destination?.city ?? "",
                        CityAirportCode = departure.destination?.code_iata ?? "",
                        CityAirportName = departure.destination?.name ?? "",
                        DateTimeCreated = DateTime.UtcNow
                    };

                    var pk = _flightSqlRepository.InsertFlight(flight, conn);

                    foreach (var codesharePartner in departure.codeshares_iata)
                    {
                        _flightSqlRepository.InsertCodesharePartner(conn, pk, codesharePartner);
                    }
                }
            }
        }
    }
}