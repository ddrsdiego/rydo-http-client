namespace Rydo.HttpClient.Sample.Controllers.v1
{
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiController]
    [Route($"api/{Versions}/customers/positions")]
    public class CustomerPositionController : ControllerBase
    {
        private const string Versions = "v1";
        private readonly IHttpServiceRequesterFactory _factory;
        
        public CustomerPositionController(IHttpServiceRequesterFactory factory)
        {
            _factory = factory;
        }

        [HttpGet("{accountNumber}")]
        public async Task<IActionResult> GetPositionConsolidated(string accountNumber)
        {
            const string customerConsolidatePosition = "customer-consolidate-position";
            
            var request = _factory.CreateRequestFor(customerConsolidatePosition);
            
            var response = await request
                .WithParameters(accountNumber)
                .GetAsync<CustomerPositionConsolidated>();

            if (response.StatusCode == HttpStatusCode.OK)
                return Ok(response.Result);
            
            return NotFound();
        }
    }
}