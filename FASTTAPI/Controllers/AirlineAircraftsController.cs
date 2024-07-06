using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.PostgresSqlRepositories;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("Statistics")]
    public class StatisticsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public StatisticsController(IConfiguration config)
        {
            _configuration = config;
        }

        [HttpGet]
        public async Task<List<AirlineAircraft>> Get()
        {
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                return new List<AirlineAircraft>();
                //return _airlineAircraftPostgresSqlRepository.GetAirlineAircrafts(connection);
                connection.Close();
            }
        }

       
    }
}
