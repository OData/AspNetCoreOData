using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataDynamicSample.Controllers
{
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [MyODataModelProvider("v{version}")]
        [HttpGet("v{version}/WeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }

    public class MyODataModelProvider : ODataModelProviderAttribute
    {

        public MyODataModelProvider(string prefix)
            : base(prefix)
        { }

        public override IEdmModel GetEdmModel(ODataModelContext context)
        {
            var builder = new ODataConventionModelBuilder();
            builder.Singleton<WeatherForecast>("WeatherForecast");
            return builder.GetEdmModel();
        }
    }
}
