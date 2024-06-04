using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.PostgresSqlRepositories;
using FASTTAPI.Enumerations;
using FASTTAPI.Models;
using FASTTAPI.Models.FlightAware;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Npgsql;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("FlightData")]
    public class FlightDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FlightDataController> _logger;
        private readonly FlightsPostgresSqlRepository _flightSqlRepository;
        private readonly FlightCodesharePartnersPostgresSqlRepository _flightCodesharePartnerSqlRepository;
        private readonly AirportPostgresSqlRepository _airportSqlRepository;

        public FlightDataController(ILogger<FlightDataController> logger, IConfiguration config)
        {
            _configuration = config;
            _logger = logger;
            _flightSqlRepository = new FlightsPostgresSqlRepository();
            _flightCodesharePartnerSqlRepository = new FlightCodesharePartnersPostgresSqlRepository();
            _airportSqlRepository = new AirportPostgresSqlRepository();
        }

        [HttpGet]
        public async Task<List<BaseAirportFlightModel>> Get([FromQuery] Disposition.Type dispositionType, DateTime fromDateTime, DateTime toDateTime, string? flightNumber, string? airline, string? city, bool? includeCodesharePartners)
        {
            AirlineRegistry.GetAirlines();
            var flights = new List<BaseAirportFlightModel>();

            var airlineCode = string.Empty;
            var airportCode = string.Empty;

            if (!string.IsNullOrWhiteSpace(airline))
            {
                var convertedAirline = AirlineRegistry.FindAirline(airline);
                if (convertedAirline != null)
                {
                    airlineCode = convertedAirline.IataCode;
                }
            }

            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();

                if (!string.IsNullOrWhiteSpace(city))
                {
                    var airports = _airportSqlRepository.GetAirports(connection);
                    airportCode = AirportFinder.FindAirport(airports, city)?.Code;
                }

                foreach (var flight in _flightSqlRepository.GetFlights(connection, dispositionType, fromDateTime, toDateTime, flightNumber ?? "", airlineCode, airportCode ?? "", includeCodesharePartners ?? false))
                {
                    var codesharePartners = _flightCodesharePartnerSqlRepository.GetCodesharePartners(flight.Pk, connection);
                    var convertedCodesharePartners = codesharePartners.Distinct().Select(partner => AirlineRegistry.FindAirline(partner));

                    //TODO: make shared code for this
                    var flightModel = new BaseAirportFlightModel
                    {
                        FlightNumber = flight.FlightNumber,
                        Status = flight.Status,
                        CodesharePartners = convertedCodesharePartners.Where(partner => partner != null).Select(partner => partner.Name).ToList(),
                        AirportGate = flight.Gate,
                        ScheduledArrivalTime = flight.DateTimeScheduled,
                        ScheduledDepartureTime = flight.DateTimeScheduled,
                        EstimatedArrivalTime = flight.DateTimeEstimated,
                        EstimatedDepartureTime = flight.DateTimeEstimated,
                        ActualArrivalTime = flight.DateTimeActual,
                        ActualDepartureTime = flight.DateTimeActual,
                        CityName = flight.CityName,
                        CityCode = flight.CityAirportCode,
                        CityAirportName = flight.CityAirportName,
                        LastUpdated = (flight.DateTimeModified ?? flight.DateTimeCreated).ToLocalTime()
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
        public async Task<string> Populate([FromBody] PostFlightDataBody requestBody)
        {
            if (string.Compare(requestBody.AdminPassword, _configuration["AdminPassword"].ToString(), false) != 0) return "Invalid password";

            var status = "connecting to database";

            try
            {
                using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
                {
                    NpgsqlTransaction transaction = null;

                    var needToWait = false;
                    connection.Open();
                    

                    if (requestBody.Arrived)
                    {
                        status = "populating tables with arrived flights";
                        if (needToWait) Thread.Sleep(60000);                   
                        transaction = connection.BeginTransaction();
                        await PopulateFlightTable(requestBody.FromDateTime, requestBody.ToDateTime, "arrivals", connection, transaction);
                        transaction.Commit();
                        needToWait = true;
                    }
                    if (requestBody.ScheduledArriving)
                    {
                        status = "populating tables with scheduled arriving flights";
                        if (needToWait) Thread.Sleep(60000);
                        transaction = connection.BeginTransaction();
                        await PopulateFlightTable(requestBody.FromDateTime, requestBody.ToDateTime, "scheduled_arrivals", connection, transaction);
                        transaction.Commit();
                        needToWait = true;
                    }
                    if (requestBody.Departed)
                    {
                        status = "populating tables with departed flights";
                        if (needToWait) Thread.Sleep(60000);
                        Thread.Sleep(60000);
                        transaction = connection.BeginTransaction();
                        await PopulateFlightTable(requestBody.FromDateTime, requestBody.ToDateTime, "departures", connection, transaction);
                        transaction.Commit();
                        needToWait = true;
                    }
                    if (requestBody.ScheduledDeparting)
                    {
                        status = "populating tables with scheduled departing flights";
                        if (needToWait) Thread.Sleep(60000);
                        Thread.Sleep(60000);
                        transaction = connection.BeginTransaction();
                        await PopulateFlightTable(requestBody.FromDateTime, requestBody.ToDateTime, "scheduled_departures", connection, transaction);
                        transaction.Commit();
                        needToWait = true;
                    }

                    status = "Successfully completed populating tables";

                }
            }
            catch (Exception ex)
            {
                status = $"ERROR while {status}: {ex.Message}";
                _logger.LogError(ex.Message, ex);
            }

            return status;
        }

        private async Task PopulateFlightTable(
            DateTime fromDateTime, 
            DateTime toDateTime, 
            string resource, 
            NpgsqlConnection conn,
            NpgsqlTransaction trans)
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
                        InsertFlights(flightAwareResponse, conn, trans);
                    }

                    cursor = flightAwareResponse?.links?.next;

                } while (!string.IsNullOrWhiteSpace(cursor));
            }
        }

        private void InsertFlights(FlightAwareAirportFlightsResponseObject flightAwareResponse, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            var airports = _airportSqlRepository.GetAirports(conn);

            if (flightAwareResponse.arrivals != null)
            {
                foreach (var arrival in flightAwareResponse.arrivals)
                {
                    if (airports.Find(airport => string.Compare(airport.Code, arrival.origin.code_iata) == 0) == null)
                    {
                        var airport = new Airport
                        {
                            Code = arrival.origin.code_iata,
                            Name = arrival.origin.name,
                            CityName = arrival.origin.city
                        };
                        _airportSqlRepository.InsertAirport(conn, trans, airport);
                        airports.Add(airport);
                    }

                    var flight = new Flight
                    {
                        Disposition = true,
                        FlightNumber = arrival.flight_number,
                        Status = arrival.status,
                        Airline = arrival.operator_iata,
                        DateTimeScheduled = arrival.scheduled_in,
                        DateTimeEstimated = arrival.estimated_in,
                        DateTimeActual = arrival.actual_in,
                        Gate = arrival.gate_destination,
                        CityName = arrival.origin.city,
                        CityAirportCode = arrival.origin.code_iata,
                        CityAirportName = arrival.origin.name,
                        DateTimeCreated = DateTime.UtcNow,
                        HasCodesharePartners = arrival.codeshares_iata.Any()
                    };

                    var pk  = _flightSqlRepository.InsertFlight(flight, conn, trans);
                    
                    foreach (var codesharePartner in arrival.codeshares_iata)
                    {
                        _flightCodesharePartnerSqlRepository.InsertCodesharePartner(conn, trans, pk, codesharePartner);
                    }
                }
            }


            if (flightAwareResponse.scheduled_arrivals != null)
            {
                foreach (var arrival in flightAwareResponse.scheduled_arrivals)
                {
                    if (airports.Find(airport => string.Compare(airport.Code, arrival.origin.code_iata) == 0) == null)
                    {
                        var airport = new Airport
                        {
                            Code = arrival.origin.code_iata,
                            Name = arrival.origin.name,
                            CityName = arrival.origin.city
                        };
                        _airportSqlRepository.InsertAirport(conn, trans, airport);
                        airports.Add(airport);
                    }

                    var flight = new Flight
                    {
                        Disposition = true,
                        FlightNumber = arrival.flight_number,
                        Status = arrival.status,
                        Airline = arrival.operator_iata,
                        DateTimeScheduled = arrival.scheduled_in,
                        DateTimeEstimated = arrival.estimated_in,
                        DateTimeActual = arrival.actual_in,
                        Gate = arrival.gate_destination,
                        CityName = arrival.origin.city,
                        CityAirportCode = arrival.origin.code_iata,
                        CityAirportName = arrival.origin.name,
                        DateTimeCreated = DateTime.UtcNow,
                        HasCodesharePartners = arrival.codeshares_iata.Any()
                    };

                    var pk = _flightSqlRepository.InsertFlight(flight, conn, trans);

                    foreach (var codesharePartner in arrival.codeshares_iata)
                    {
                        _flightCodesharePartnerSqlRepository.InsertCodesharePartner(conn, trans, pk, codesharePartner);
                    }
                }
            }

            if (flightAwareResponse.departures != null)
            {
                foreach (var departure in flightAwareResponse.departures)
                {
                    if (airports.Find(airport => string.Compare(airport.Code, departure.destination.code_iata) == 0) == null)
                    {
                        var airport = new Airport
                        {
                            Code = departure.destination.code_iata,
                            Name = departure.destination.name,
                            CityName = departure.destination.city
                        };
                        _airportSqlRepository.InsertAirport(conn, trans, airport);
                        airports.Add(airport);
                    }

                    var flight = new Flight
                    {
                        Disposition = false,
                        FlightNumber = departure.flight_number,
                        Status = departure.status,
                        Airline = departure.operator_iata,
                        DateTimeScheduled = departure.scheduled_out,
                        DateTimeEstimated = departure.estimated_out,
                        DateTimeActual = departure.actual_out,
                        Gate = departure.gate_origin,
                        CityName = departure.destination.city,
                        CityAirportCode = departure.destination.code_iata,
                        CityAirportName = departure.destination.name,
                        DateTimeCreated = DateTime.UtcNow,
                        HasCodesharePartners = departure.codeshares_iata.Any()
                    };

                    var pk = _flightSqlRepository.InsertFlight(flight, conn, trans);

                    foreach (var codesharePartner in departure.codeshares_iata)
                    {
                        _flightCodesharePartnerSqlRepository.InsertCodesharePartner(conn, trans, pk, codesharePartner);
                    }
                }
            }

            if (flightAwareResponse.scheduled_departures != null)
            {
                foreach (var departure in flightAwareResponse.scheduled_departures)
                {
                    if (airports.Find(airport => string.Compare(airport.Code, departure.destination.code_iata) == 0) == null)
                    {
                        var airport = new Airport
                        {
                            Code = departure.destination.code_iata,
                            Name = departure.destination.name,
                            CityName = departure.destination.city
                        };
                        _airportSqlRepository.InsertAirport(conn, trans, airport);
                        airports.Add(airport);
                      
                    }

                    var flight = new Flight
                    {
                        Disposition = false,
                        FlightNumber = departure.flight_number,
                        Status = departure.status,
                        Airline = departure.operator_iata,
                        DateTimeScheduled = departure.scheduled_out,
                        DateTimeEstimated = departure.estimated_out,
                        DateTimeActual = departure.actual_out,
                        Gate = departure.gate_origin,
                        CityName = departure.destination?.city ?? "",
                        CityAirportCode = departure.destination?.code_iata ?? "",
                        CityAirportName = departure.destination?.name ?? "",
                        DateTimeCreated = DateTime.UtcNow,
                        HasCodesharePartners = departure.codeshares_iata.Any()
                    };

                    var pk = _flightSqlRepository.InsertFlight(flight, conn, trans);

                    foreach (var codesharePartner in departure.codeshares_iata)
                    {
                        _flightCodesharePartnerSqlRepository.InsertCodesharePartner(conn, trans, pk, codesharePartner);
                    }
                }
            }
        }
    }
}