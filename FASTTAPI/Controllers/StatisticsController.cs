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
        private readonly StatisticsPostgresSqlRepository _statisticsPostgresSqlRepository;

        public StatisticsController(IConfiguration config)
        {
            _configuration = config;
            _statisticsPostgresSqlRepository = new StatisticsPostgresSqlRepository();
        }

        [HttpGet]
        public async Task<List<PaxVolumeHour>> Get([FromQuery] DateTime fromDateTime, DateTime toDateTime)
        {
            using (var connection = new NpgsqlConnection(DatabaseConnectionStringBuilder.GetSqlConnectionString(_configuration)))
            {
                connection.Open();
                return _statisticsPostgresSqlRepository.GetHourlyPassengerVolume(fromDateTime, toDateTime, connection);
                connection.Close();
            }
        }

       
    }
}
