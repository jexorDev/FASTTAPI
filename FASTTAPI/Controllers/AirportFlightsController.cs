using FASTTAPI.DataLayer.SqlRepositories;
using FASTTAPI.Enumerations;
using FASTTAPI.Models;
using FASTTAPI.Models.FlightAware;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Web;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("AirportFlights")]
    public class AirportFlightsController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<AirportFlightsController> _logger;
        private readonly FlightsSqlRepository _flightSqlRepository;            

        public AirportFlightsController(ILogger<AirportFlightsController> logger, IConfiguration config)
        {
            _configuration = config;
            _logger = logger;
            _flightSqlRepository = new FlightsSqlRepository();
        }     

        [HttpGet]
        public async Task<GetResponseBody> Get([FromQuery] GetAirportFlightsRequestParameters parameters)
        {
            var nextDataPageUrlDecoded = HttpUtility.UrlDecode(parameters.NextDataPageUrl);

            Airline? airline = AirlineRegistry.FindAirline(parameters?.Airline ?? ""); 
            var apiEndpoint = string.Empty;
            
            //FlightAware API doesn't provide filtering by airport, so have to use cached data in db
            var useDatabaseCache = !string.IsNullOrWhiteSpace(parameters?.Airport);

            if (Disposition.Type.ScheduledArriving.Equals(parameters?.DispositionType))
            {
                apiEndpoint = "scheduled_arrivals";
            }
            else if (Disposition.Type.ScheduledDepartures.Equals(parameters?.DispositionType))
            {
                apiEndpoint = "scheduled_departures";
            }
            else if (Disposition.Type.Arrived.Equals(parameters?.DispositionType))
            {
                apiEndpoint = "arrivals";
            }
            else if (Disposition.Type.Departed.Equals(parameters?.DispositionType))
            {
                apiEndpoint = "departures";
            }

            var response = new GetResponseBody();

            if (string.IsNullOrWhiteSpace(parameters?.NextDataPageUrl) && Disposition.Type.None.Equals(parameters?.DispositionType)) return response;

            if (useDatabaseCache)
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
                {
                    connection.Open();

                    foreach (var flight in _flightSqlRepository.GetFlights(connection, parameters?.DispositionType ?? Disposition.Type.None, parameters.DateTimeFrom, parameters.DateTimeTo, (airline?.IataCode ?? ""), parameters.Airport ?? "", false))
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

                        response.Results.Add(flightModel);
                        
                    }

                    connection.Close();
                }                

                return response;
            }            

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Apikey", _configuration["FlightAwareKey"]);
                string endpoint = string.Empty;

                if (string.IsNullOrWhiteSpace(nextDataPageUrlDecoded))
                {
                    endpoint = $"/airports/{_configuration["AirportCode"]}/flights/{apiEndpoint}?{FlightAwareApi.BuildFlightAwareQueryString(parameters.DateTimeFrom, parameters.DateTimeTo, airline?.IcaoCode ?? "")}";
                }
                else
                {
                    endpoint = nextDataPageUrlDecoded;
                }

                var apiRequestQuery = $"{FlightAwareApi.BaseUri}{endpoint}";
                var flightAwareResponseObject = await client.GetAsync(apiRequestQuery);
                var flightAwareResponseBody = flightAwareResponseObject.Content.ReadAsStringAsync().Result;

                if (flightAwareResponseBody == null) return response;

                FlightAwareAirportFlightsResponseObject flightAwareResponse = JsonConvert.DeserializeObject<FlightAwareAirportFlightsResponseObject>(flightAwareResponseBody);
                
                if (flightAwareResponse != null)
                {
                    if (flightAwareResponse.scheduled_arrivals != null)
                    {
                        foreach (var arrival in flightAwareResponse.scheduled_arrivals)
                        {
                            response.Results.Add(new BaseAirportFlightModel
                            {
                                Status = arrival.status,
                                Disposition = DispositionType.Arrival,
                                FlightNumber = arrival.flight_number,
                                AirportGate = arrival.gate_destination,
                                AirlineIdentifier = arrival.operator_icao,
                                AirlineName = FlightAwareApi.GetAirlineWithCodesharePartners(arrival.operator_icao, arrival.codeshares), 
                                ScheduledDepartureTime = arrival.scheduled_out,
                                EstimatedDepartureTime = arrival.estimated_out,
                                ActualDepartureTime = arrival.actual_out,
                                ScheduledArrivalTime = arrival.scheduled_in,
                                EstimatedArrivalTime = arrival.estimated_in,
                                ActualArrivalTime = arrival.actual_in,
                                CityCode = arrival.origin.code_iata,
                                CityName = arrival.origin.city,
                                CityAirportName = arrival.origin.name,
                            });
                        }
                    }

                    if (flightAwareResponse.arrivals != null)
                    {
                        foreach (var arrival in flightAwareResponse.arrivals)
                        {
                            response.Results.Add(new BaseAirportFlightModel
                            {
                                Status = arrival.status,
                                Disposition = DispositionType.Arrival,
                                FlightNumber = arrival.flight_number,
                                AirportGate = arrival.gate_destination,
                                AirlineIdentifier = arrival.operator_icao,
                                AirlineName = FlightAwareApi.GetAirlineWithCodesharePartners(arrival.operator_icao, arrival.codeshares),
                                ScheduledDepartureTime = arrival.scheduled_out,
                                EstimatedDepartureTime = arrival.estimated_out,
                                ActualDepartureTime = arrival.actual_out,
                                ScheduledArrivalTime = arrival.scheduled_in,
                                EstimatedArrivalTime = arrival.estimated_in,
                                ActualArrivalTime = arrival.actual_in,
                                CityCode = arrival.origin.code_iata,
                                CityName = arrival.origin.city,
                                CityAirportName = arrival.origin.name,
                            });
                        }
                    }

                    if (flightAwareResponse.scheduled_departures != null)
                    {
                        foreach (var arrival in flightAwareResponse.scheduled_departures)
                        {
                            response.Results.Add(new BaseAirportFlightModel
                            {
                                Status = arrival.status,
                                Disposition = DispositionType.Departure,
                                FlightNumber = arrival.flight_number,
                                AirportGate = arrival.gate_origin,
                                AirlineIdentifier = arrival.operator_icao,
                                AirlineName = FlightAwareApi.GetAirlineWithCodesharePartners(arrival.operator_icao, arrival.codeshares),
                                ScheduledDepartureTime = arrival.scheduled_out,
                                EstimatedDepartureTime = arrival.estimated_out,
                                ActualDepartureTime = arrival.actual_out,
                                ScheduledArrivalTime = arrival.scheduled_in,
                                EstimatedArrivalTime = arrival.estimated_in,
                                ActualArrivalTime = arrival.actual_in,
                                CityCode = arrival.destination.code_iata,
                                CityName = arrival.destination.city,
                                CityAirportName = arrival.destination.name,
                            });

                        }
                    }

                    if (flightAwareResponse.departures != null)
                    {
                        foreach (var arrival in flightAwareResponse.departures)
                        {
                            response.Results.Add(new BaseAirportFlightModel
                            {
                                Status = arrival.status,
                                Disposition = DispositionType.Departure,
                                FlightNumber = arrival.flight_number,
                                AirportGate = arrival.gate_origin,
                                AirlineIdentifier = arrival.operator_icao,
                                AirlineName = FlightAwareApi.GetAirlineWithCodesharePartners(arrival.operator_icao, arrival.codeshares),
                                ScheduledDepartureTime = arrival.scheduled_out,
                                EstimatedDepartureTime = arrival.estimated_out,
                                ActualDepartureTime = arrival.actual_out,
                                ScheduledArrivalTime = arrival.scheduled_in,
                                EstimatedArrivalTime = arrival.estimated_in,
                                ActualArrivalTime = arrival.actual_in,
                                CityCode = arrival.destination.code_iata,
                                CityName = arrival.destination.city,
                                CityAirportName = arrival.destination.name,
                            });

                        }
                    }

                    if (flightAwareResponse.links != null)
                    {
                        response.NextDataPageUrl = HttpUtility.UrlEncode(flightAwareResponse.links.next);
                    }
                }

                //TODO: remove when done debugging
                response.GeneratedApiQuery = apiRequestQuery;
                response.RawData = flightAwareResponseBody;

                return response;
            }
        }       
    }
}