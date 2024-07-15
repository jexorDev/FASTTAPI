using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.PostgresSqlRepositories;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("FailedFlights")]
    public class FailedFlightsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly FailedFlightsPostgresSqlRepository _failedFlightsRepository;

        public FailedFlightsController(IConfiguration config)
        {
            _configuration = config;
            _failedFlightsRepository = new FailedFlightsPostgresSqlRepository();
        }

        [HttpGet]
        public async Task<List<FailedFlight>> Get()
        {
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                return _failedFlightsRepository.GetFailedFlights(connection);
                connection.Close();
            }
        }      
    }
}
