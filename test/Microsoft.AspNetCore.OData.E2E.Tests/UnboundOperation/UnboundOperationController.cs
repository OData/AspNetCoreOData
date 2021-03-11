// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UnboundOperation
{
    public class ConventionCustomersController : ODataController
    {
        private static IList<ConventionCustomer> _customers = null;

        private IList<ConventionCustomer> InitCustomers()
        {
            IList<ConventionCustomer> customers = Enumerable.Range(1, 10).Select(i =>
            new ConventionCustomer
            {
                ID = 400 + i,
                Name = "Name " + i,
                Address = new ConventionAddress()
                {
                    Street = "Street " + i,
                    City = "City " + i,
                    ZipCode = (201100 + i).ToString()
                },
                Orders = Enumerable.Range(1, i).Select(j =>
                new ConventionOrder
                {
                    ID = j,
                    OrderName = "OrderName " + j,
                    Price = j,
                    OrderGuid = Guid.Empty
                }).ToList()
            }).ToList();

            return customers;
        }

        public ConventionCustomersController()
        {
            if (_customers == null)
            {
                _customers = InitCustomers();
            }
        }

        public IList<ConventionCustomer> Customers => _customers;

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IActionResult Get()
        {
            return Ok(_customers.AsQueryable());
        }

        // It's a top level function without parameters
        [EnableQuery]
        [HttpGet("odata/GetAllConventionCustomers()")]
        [HttpGet("odata/GetAllConventionCustomersImport()")]
        public IEnumerable<ConventionCustomer> GetAllConventionCustomers()
        {
            return _customers;
        }

        [EnableQuery]
        [HttpGet("odata/GetAllConventionCustomersImport(CustomerName={customerName})")]
        [HttpGet("odata/GetAllConventionCustomersImport(CustomerName={customerName})/$count")]
        // [FromODataUri] can not be deleted within below line, or the value of OrderName will be enclosed by single quote mark('). 
        public IEnumerable<ConventionCustomer> GetAllConventionCustomers([FromODataUri]String CustomerName)
        {
            IEnumerable<ConventionCustomer> customers = _customers.Where(c => c.Name.Contains(CustomerName));
            return customers;
        }

        // It's a top level function with one parameter]
        [HttpGet("odata/GetConventionCustomerById(CustomerId={CustomerId})")]
        [HttpGet("odata/GetConventionCustomerByIdImport(CustomerId={CustomerId})")]
        public ConventionCustomer GetConventionCustomerById(int CustomerId)
        {
            return _customers.Where(c => c.ID == CustomerId).FirstOrDefault();
        }

        [HttpGet("odata/GetConventionCustomerNameByIdImport(CustomerId={CustomerId})")]
        [HttpGet("odata/GetConventionCustomerByIdImport(CustomerId={CustomerId})/Name")]
        public String GetConventionCustomerNameById([FromODataUri]int CustomerId)
        {
            return _customers.Where(c => c.ID == CustomerId).FirstOrDefault().Name;
        }

        [HttpGet("odata/GetConventionOrderByCustomerIdAndOrderName(CustomerId={CustomerId},OrderName={OrderName})")]
        [HttpGet("odata/GetConventionOrderByCustomerIdAndOrderNameImport(CustomerId={CustomerId},OrderName={OrderName})")]
        public ConventionOrder GetConventionOrderByCustomerIdAndOrderName(int CustomerId, [FromODataUri]string OrderName)
        {
            ConventionCustomer customer = _customers.Where(c => c.ID == CustomerId).FirstOrDefault();
            return customer.Orders.Where(o => o.OrderName == OrderName).FirstOrDefault();
        }

        [HttpGet("odata/AdvancedFunction(nums={numbers},genders={genders},location={address},addresses={addresses},customer={customer},customers={customers})")]
        public bool AdvancedFunction([FromODataUri]IEnumerable<int> numbers,
            [FromODataUri]IEnumerable<ConventionGender> genders,
            [FromODataUri]ConventionAddress address, [FromODataUri]IEnumerable<ConventionAddress> addresses,
            [FromODataUri]ConventionCustomer customer, [FromODataUri]IEnumerable<ConventionCustomer> customers)
        {
            Assert.Equal(new[] {1, 2, 3}, numbers);
            Assert.Equal(new[] {ConventionGender.Male, ConventionGender.Female}, genders);

            IEnumerable<ConventionAddress> newAddress = addresses.Concat(new[] {address});
            Assert.Equal(2, newAddress.Count());
            foreach (ConventionAddress addr in newAddress)
            {
                Assert.Equal("Zi Xin Rd.", addr.Street);
                Assert.Equal("Shanghai", addr.City);
                Assert.Equal("2001100", addr.ZipCode);
            }

            IEnumerable<ConventionCustomer> newCustomers = customers.Concat(new[] { customer});
            Assert.Equal(2, newCustomers.Count());
            foreach (ConventionCustomer cust in newCustomers)
            {
                Assert.Equal(7, cust.ID);
                Assert.Equal("Tony", cust.Name);
                Assert.Null(cust.Address);
            }

            return true;
        }

        [EnableQuery]
        [HttpGet("odata/GetDefinedGenders()")]
        [HttpGet("odata/GetDefinedGenders()/$count")]
        public IActionResult GetDefinedGenders()
        {
            IList<ConventionGender> genders = new List<ConventionGender>();
            genders.Add(ConventionGender.Male);
            genders.Add(ConventionGender.Female);
            return Ok(genders);
        }

        [EnableQuery]
        [HttpPost("odata/UpdateAddress")]
        [HttpPost("odata/UpdateAddressImport")]
        public IActionResult UpdateAddress([FromBody]ODataUntypedActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var id = (int)parameters["ID"];
            var address = parameters["Address"] as EdmComplexObject;
            var conventionAddress = new ConventionAddress();
            object temp = null;
            if (address.TryGetPropertyValue("Street", out temp))
            {
                conventionAddress.Street = temp.ToString();
                Assert.Equal("Street 11", conventionAddress.Street);
            }
            if (address.TryGetPropertyValue("City", out temp))
            {
                conventionAddress.City = temp.ToString();
            }
            if (address.TryGetPropertyValue("ZipCode", out temp))
            {
                conventionAddress.ZipCode = temp.ToString();
            }

            // In real scenario, we should update the original data, but for test, let's create a new Database
            var customers = InitCustomers();
            ConventionCustomer customer = customers.Where(c => c.ID == id).FirstOrDefault();
            customer.Address = conventionAddress;
            return Ok(customers);
        }

        /*
        [HttpPost("odata/CreateCustomer")]
        [HttpPost("odata/CreateCustomerImport")]
        public IActionResult CreateCustomer(ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var conventionCustomer =(ConventionCustomer) parameters["value"];
            conventionCustomer.ID = _customers.Count() + 1;
            _customers.Add(conventionCustomer);
            return Ok(_customers);
        }
         * */

        [HttpPost("odata/AdvancedAction")]
        public IActionResult AdvancedAction([FromBody]ODataActionParameters parameters)
        {
            Assert.NotNull(parameters);
            Assert.Equal(new[] { 4, 5, 6 }, parameters["nums"] as IEnumerable<int>);
            Assert.Equal(new[] { ConventionGender.Male, ConventionGender.Female }, parameters["genders"] as IEnumerable<ConventionGender>);

            IList<ConventionAddress> newAddress = (parameters["addresses"] as IEnumerable<ConventionAddress>).ToList();
            Assert.Single(newAddress);
            foreach (ConventionAddress addr in newAddress.Concat(new[] {parameters["location"]}))
            {
                Assert.Equal("NY Rd.", addr.Street);
                Assert.Equal("Redmond", addr.City);
                Assert.Equal("9011", addr.ZipCode);
            }

            IList<ConventionCustomer> newCustomers = (parameters["customers"] as IEnumerable<ConventionCustomer>).ToList();
            Assert.Single(newAddress);
            foreach (ConventionCustomer cust in newCustomers.Concat(new[] { parameters["customer"] }))
            {
                Assert.Equal(8, cust.ID);
                Assert.Equal("Mike", cust.Name);
                Assert.Null(cust.Address);
            }

            return Ok();
        }
    }
}
