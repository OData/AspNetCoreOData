//-----------------------------------------------------------------------------
// <copyright file="WeatherForecastController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ODataNewtonsoftJsonSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IOptions<MvcNewtonsoftJsonOptions> _options;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptions<MvcNewtonsoftJsonOptions> options)
        {
            _logger = logger;
            _options = options;
        }

        // For example: http://localhost:12197/weatherforecast?$select=TemperatureC  (IIS)
        // For example: http://localhost:5000/weatherforecast?$select=TemperatureC  (Non-IIS)
        // Be noted: since we register "WeatherForecast" as one entity set name.
        // OData conventional routing will build a new route for this method.
        // If you send "$odata" request, you will see this method has three routes:
        // 1) ~/odata/WeatherForecast
        // 2) ~/odata/WeatherForecast/$count
        // 3) ~/WeatherForecast
        // Try it and you will get different payload.
        [HttpGet]
        [EnableQuery]
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
}
