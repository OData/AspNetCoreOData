//-----------------------------------------------------------------------------
// <copyright file="CustomersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.EntityFrameworkCore;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v1
{
    [ODataRouteComponent("v1")]
    public class CustomersController : ControllerBase
    {
        private MyDataContext _context;

        public CustomersController(MyDataContext context)
        {
            _context = context;
            if (_context.Customers.Count() == 0)
            {
                IList<Customer> customers = GetCustomers();

                foreach (var customer in customers)
                {
                    _context.Customers.Add(customer);
                }

                _context.SaveChanges();
            }
        }

        // For example: http://localhost:5000/v1/customers?$apply=groupby((Name), aggregate($count as count))&$orderby=name desc
        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(GetCustomers());
        }

        [HttpGet]
        [EnableQuery]
        public Customer Get(int key)
        {
            // Be noted: without the NoTracking setting, the query for $select=HomeAddress with throw exception:
            // A tracking query projects owned entity without corresponding owner in result. Owned entities cannot be tracked without their owner...
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return new Customer
            {
                Id = key,
                Name = "Name + " + key
            };
        }

        [HttpPost]
        public IActionResult Post([FromBody] Customer newCustomer)
        {
            return Ok();
        }

        [HttpPost]
        public string RateByName(int key, [FromODataBody] string name, [FromODataBody] int age)
        {
            return key + name + ": " + age;
        }

        [HttpPost]
        [EnableQuery]
        public IActionResult BoundAction(int key, ODataActionParameters parameters)
        {
            return Ok($"BoundAction of Customers with key {key} : {System.Text.Json.JsonSerializer.Serialize(parameters)}");
        }

        private static IList<Customer> GetCustomers()
        {
            return new List<Customer>
            {
                new Customer
                {
                    Id = 1,
                    Name = "Jonier",
                    FavoriteColor = Color.Red,
                    HomeAddress = new Address { City = "Redmond", Street = "156 AVE NE" },
                    Amount = 3,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "Redmond", Street = "256 AVE NE" },
                        new Address { City = "Redd", Street = "56 AVE NE" },
                    },
                },
                new Customer
                {
                    Id = 2,
                    Name = "Sam",
                    FavoriteColor = Color.Blue,
                    HomeAddress = new CnAddress { City = "Bellevue", Street = "Main St NE", Postcode = "201100" },
                    Amount = 6,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "Red4ond", Street = "456 AVE NE" },
                        new Address { City = "Re4d", Street = "51 NE" },
                    },
                },
                new Customer
                {
                    Id = 3,
                    Name = "Peter",
                    FavoriteColor = Color.Green,
                    HomeAddress = new UsAddress { City = "Hollewye", Street = "Main St NE", Zipcode = "98029" },
                    Amount = 4,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "R4mond", Street = "546 NE" },
                        new Address { City = "R4d", Street = "546 AVE" },
                    },
                },
                new Customer
                {
                    Id = 4,
                    Name = "Sam",
                    FavoriteColor = Color.Red,
                    HomeAddress = new UsAddress { City = "Banff", Street = "183 St NE", Zipcode = "111" },
                    Amount = 5,
                    FavoriteAddresses = new List<Address>
                    {
                        new Address { City = "R4m11ond", Street = "116 NE" },
                        new Address { City = "Jesper", Street = "5416 AVE" },
                    }
                }
            };
        }
    }
}
