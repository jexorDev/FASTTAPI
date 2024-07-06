using FASTTAPI.DataLayer.DataTransferObjects;
using FASTTAPI.DataLayer.PostgresSqlRepositories;
using FASTTAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("AirlineAircrafts")]
    public class AirlineAircraftsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AirlineAircraftsPostgresSqlRepository _airlineAircraftPostgresSqlRepository;

        public AirlineAircraftsController(IConfiguration config)
        {
            _configuration = config;
            _airlineAircraftPostgresSqlRepository = new AirlineAircraftsPostgresSqlRepository();
        }

        [HttpGet]
        public async Task<List<AirlineAircraft>> Get()
        {
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                return _airlineAircraftPostgresSqlRepository.GetAirlineAircrafts(connection);
                connection.Close();
            }
        }

        [HttpPut]
        public async Task Put([FromBody] AirlineAircraftPutBody body)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                NpgsqlTransaction trans = connection.BeginTransaction();
                foreach (var airlineAircraft in body.AirlineAircrafts)
                {
                    _airlineAircraftPostgresSqlRepository.UpdateAirlineAircraft(airlineAircraft, connection, trans);
                }
                trans.Commit();
                connection.Close();
            }
        }
    }
}
