// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Scenarios
{
    // Controller
    public class SimpleOpenCustomersController : ODataController
    {
        [EnableQuery]
        public IQueryable<SimpleOpenCustomer> Get()
        {
            return CreateCustomers().AsQueryable();
        }

        public IActionResult Get(int key)
        {
            IList<SimpleOpenCustomer> customers = CreateCustomers();
            SimpleOpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            if (customer.CustomerProperties == null)
            {
                customer.CustomerProperties = new Dictionary<string, object>();
                customer.CustomerProperties.Add("Token", new Guid("2C1F450A-A2A7-4FE1-A25D-4D9332FC0694"));
                IList<int> lists = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
                customer.CustomerProperties.Add("MyList", lists);
            }

            return Ok(customer);
        }

        public IActionResult GetAddress(int key)
        {
            IList<SimpleOpenCustomer> customers = CreateCustomers();
            SimpleOpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Address);
        }

        public IActionResult PostSimpleOpenCustomer([FromBody] SimpleOpenCustomer customer)
        {
            // Verify there is a string dynamic property
            object countryValue;
            customer.CustomerProperties.TryGetValue("Place", out countryValue);
            Assert.NotNull(countryValue);
            Assert.Equal(typeof(String), countryValue.GetType());
            Assert.Equal("My Dynamic Place", countryValue);

            // Verify there is a Guid dynamic property
            object tokenValue;
            customer.CustomerProperties.TryGetValue("Token", out tokenValue);
            Assert.NotNull(tokenValue);
            Assert.Equal(typeof(Guid), tokenValue.GetType());
            Assert.Equal(new Guid("2c1f450a-a2a7-4fe1-a25d-4d9332fc0694"), tokenValue);

            // Verify there is an dynamic collection property
            object value;
            customer.CustomerProperties.TryGetValue("DoubleList", out value);
            Assert.NotNull(value);
            List<double> doubleValues = Assert.IsType<List<double>>(value);
            Assert.Equal(new[] { 5.5, 4.4, 3.3 }, doubleValues);

            // special test cases to test the complex type property value is null.
            if (customer.CustomerId == 99)
            {
                Assert.Null(customer.Address);
            }

            return Ok();
        }

        public IActionResult Patch(int key, Delta<SimpleOpenCustomer> patch)
        {
            IList<SimpleOpenCustomer> customers = CreateCustomers();
            SimpleOpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            patch.Patch(customer);

            // As normal, return Updated(customer) for *PATCH*.
            // But, Here returns the *Ok* to test the payload
            return Ok(customer);
        }

        public IActionResult Put(int key, [FromBody] SimpleOpenCustomer changedCustomer)
        {
            Assert.Equal(99, changedCustomer.CustomerId);
            Assert.Equal("ChangedName", changedCustomer.Name);
            Assert.Null(changedCustomer.Address);
            Assert.Null(changedCustomer.Website);
            Assert.Single(changedCustomer.CustomerProperties);
            return Updated(changedCustomer); // Updated(customer);
        }

        private static IList<SimpleOpenCustomer> CreateCustomers()
        {
            int[] IntValues = { 200, 100, 300, 0, 400 };
            IList<SimpleOpenCustomer> customers = Enumerable.Range(0, 5).Select(i =>
                new SimpleOpenCustomer
                {
                    CustomerId = i,
                    Name = "FirstName " + i,
                    Address = new SimpleOpenAddress
                    {
                        Street = "Street " + i,
                        City = "City " + i,
                        Properties = new Dictionary<string, object> { { "IntProp", IntValues[i] } }
                    },
                    Website = "WebSite #" + i
                }).ToList();

            customers[2].CustomerProperties = new Dictionary<string, object>
            {
                {"Token", new Guid("2C1F450A-A2A7-4FE1-A25D-4D9332FC0694")},
                {"IntList", new List<int> { 1, 2, 3, 4, 5, 6, 7 }},
            };

            customers[4].CustomerProperties = new Dictionary<string, object>
            {
                {"Token", new Guid("A6A594ED-375B-424E-AC0A-945D89CF7B9B")},
                {"IntList", new List<int> { 1, 2, 3, 4, 5, 6, 7 }},
            };

            SimpleOpenAddress address = new SimpleOpenAddress
            {
                Street = "SubStreet",
                City = "City"
            };
            customers[3].CustomerProperties = new Dictionary<string, object> { { "ComplexList", new[] { address, address } } };

            SimpleVipCustomer vipCustomer = new SimpleVipCustomer
            {
                CustomerId = 9,
                Name = "VipCustomer",
                Address = new SimpleOpenAddress
                {
                    Street = "Vip Street ",
                    City = "Vip City ",
                },
                VipNum = "99-001",
                CustomerProperties = new Dictionary<string, object>
                {
                    { "ListProp", IntValues },
                    { "DateList", new[] { Date.MinValue, Date.MaxValue } },
                    { "Receipt", null }
                }
            };

            customers.Add(vipCustomer);
            return customers;
        }

    }

}