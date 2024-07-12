using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.PostgresSqlRepositories;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("Airlines")]
    public class AirlineController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;
        private readonly AirlinePostgresSqlRepository _airlinePostgresSqlRepository;

        public AirlineController(IConfiguration config)
        {
            _configuration = config;
            _airlinePostgresSqlRepository = new AirlinePostgresSqlRepository();
        }

        [HttpGet]
        public async Task<List<Airline>> Get()
        {           
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                return _airlinePostgresSqlRepository.GetAirlines(connection);
                connection.Close();
            }           
        }

        [HttpPut]
        public async Task Update(AirlinePutBody body)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();

                foreach (var airlineToUpdate in body.Airlines)
                {
                    _airlinePostgresSqlRepository.UpdateAirline(airlineToUpdate, connection);
                }

                connection.Close();
            }
        }
             
    }
}