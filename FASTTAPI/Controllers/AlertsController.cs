using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.SqlRepositories;
using FASTTAPI.Enumerations;
using FASTTAPI.Models;
using FASTTAPI.Models.Responses.Alerts;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("Alerts")]
    public class AlertsController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<AirportFlightsController> _logger;
        private readonly FlightsSqlRepository _flightSqlRepository;

        public AlertsController(ILogger<AirportFlightsController> logger, IConfiguration config)
        {
            _configuration = config;
            _logger = logger;
            _flightSqlRepository = new FlightsSqlRepository();
        }

        [HttpGet]
        public async Task<List<AirlineAlertModel>> Get(DateTime fromDateTime, DateTime toDateTime)
        {
            var alerts = new List<AirlineAlertModel>();
            var flights = new List<Flight>();
            var problemAirlines = AirlineRegistry.GetAirlines().Where(airline => airline.CommonProblems != null);

            using (SqlConnection connection = new SqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                flights = _flightSqlRepository.GetFlights(connection, Disposition.Type.None, fromDateTime, toDateTime, "", "", "", false);
                connection.Close();
            }

            foreach (var flight in flights)
            {
                var problemAirline = problemAirlines.FirstOrDefault(x => x.IataCode.Equals(flight.Airline));
                if (problemAirline == null) continue;

                var airline = AirlineRegistry.FindAirline(flight.Airline);
                
                alerts.Add(new AirlineAlertModel
                {
                    Airline = airline?.Name ?? flight.Airline,
                    AirlineCode = airline?.IcaoCode ?? flight.Airline,
                    Disposition = flight.Disposition,
                    FlightNumber = flight.FlightNumber,
                    CityAirportCode = flight.CityAirportCode,
                    CityAirportName = flight.CityAirportName,
                    ExpectedDateTime = (flight.DateTimeEstimated ?? flight.DateTimeScheduled).Value.ToLocalTime() 
                });

            }

            return alerts;
        }
             
    }
}