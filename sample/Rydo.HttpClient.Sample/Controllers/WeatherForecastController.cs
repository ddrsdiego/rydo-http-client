namespace Rydo.HttpClient.Sample.Controllers
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IHttpServiceRequesterFactory _factory;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IHttpServiceRequesterFactory factory,
            ILogger<WeatherForecastController> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            var request = _factory.CreateRequestFor("play-customers-get");
            var response = await request.WithParameters("0e18e1fc").GetAsync<PlayCustomer>();

            if (response.StatusCode == HttpStatusCode.OK)
                return Ok(response.Result);
            
            return NotFound();
        }
    }
}