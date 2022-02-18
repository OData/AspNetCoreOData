//-----------------------------------------------------------------------------
// <copyright file="AccountsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private static IList<Account> accounts;

        static AccountsController()
        {
            string[] names = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            Random rd = new();

            string[] startingGuid = new string[]
            {
                "51496043-D2BB-4940-B579-3A94E8B7C773",
                "240A32E6-5301-425E-A700-729CDD46CD42",
                "5726E1F8-70BD-43E0-BE96-185D7163EF3D",
                "305FCB7B-923F-4281-9163-A7B007EB7841",
                "4B5445B4-E794-4ABE-BCF3-D663D3B8B45F",
                "094863F1-7F5A-486E-A194-E87CED20D27E",
                "A7BD97F8-C69A-4AA3-88A3-30B615C4C182",
                "4D49FB7F-E631-4FD4-A62E-6D654705CE63",
                "19C865CE-989E-41D5-A7E7-775B4E967CCB",
                "1398B2AC-E9A9-4F86-A991-4078AC565F91"
            };

            accounts = new List<Account>();
            for (int i = 1; i <= names.Length; i++)
            {
                int num = rd.Next(20, 200);
                Account a = new Account
                {
                    AccountId = new Guid(startingGuid[i-1]),
                    Name = names[i - 1],
                    HomeAddress = new Address
                    {
                        Street = $"Road No{num}",
                        City = $"City No{num}"
                    },
                    AccountInfo = new AccountInfo
                    {
                        Id = i,
                        Balance = (rd.NextDouble() + 1.0) * 100
                    }
                };

                accounts.Add(a);
            }
        }

        private readonly ILogger<WeatherForecastController> _logger;

        public AccountsController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(ODataQueryOptions<Account> queryOptions)
        {
            var querable = accounts.AsQueryable<Account>();
            var finalQuery = queryOptions.ApplyTo(querable);
            return Ok(finalQuery);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id, ODataQueryOptions<Account> queryOptions)
        {
            var accountQuery = accounts.Where(c => c.AccountId == id);
            if (!accountQuery.Any())
            {
                return NotFound();
            }

            var finalQuery = queryOptions.ApplyTo(accountQuery.AsQueryable<Account>()) as IQueryable<dynamic>;
            var result = finalQuery.FirstOrDefault();

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
