using Microsoft.AspNetCore.Mvc;

namespace FASTTAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProblemAirlinesReportController : ControllerBase
    {
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<AirportFlightsController> _logger;      

        public ProblemAirlinesReportController(ILogger<AirportFlightsController> logger, IConfiguration config)
        {
            _configuration = config;
            _logger = logger;
        }

        [HttpGet(Name = "GetProblemAirlinesReport")]
        public async Task<GetResponseBody> Get()
        {
            return null;
        }
             
    }
}