//-----------------------------------------------------------------------------
// <copyright file="AlternateKeysController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AlternateKeys
{
    [Route("odata")]
    public class CustomersController : ODataController
    {
        public IActionResult Get(int key)
        {
            foreach (var customer in AlternateKeysDataSource.Customers)
            {
                object value;
                if (customer.TryGetPropertyValue("ID", out value))
                {
                    int intKey = (int)value;
                    if (key == intKey)
                    {
                        return Ok(customer);
                    }
                }
            }

            return NotFound();
        }

        // alternate key: SSN
        // why set Order = -2 (any number less than 0)? it is because 'Get' method has 'catch-all' template, we should move this template ahead
        // Small order goes first.
        // We can also leave order value unset, same as 'Get' method and 'PatchCustomerBySSN' method without setting the order value.
        // Without setting the order value makes all routes with same order value and catch-all goes latter
        [HttpGet("Customers(SSN={ssn})", Order = -2)]
        public IActionResult GetCustomerBySSN(string ssn)
        {
            // for special test
            if (ssn == "special-SSN")
            {
                return Ok(ssn);
            }

            foreach (var customer in AlternateKeysDataSource.Customers)
            {
                object value;
                if (customer.TryGetPropertyValue("SSN", out value))
                {
                    string stringKey = (string)value;
                    if (ssn == stringKey)
                    {
                        return Ok(customer);
                    }
                }
            }

            return NotFound();
        }

        [HttpPatch("Customers(SSN={ssnKey})")]
        public IActionResult PatchCustomerBySSN(string ssnKey, [FromBody]EdmEntityObject delta)
        {
            Assert.Equal("SSN-6-T-006", ssnKey);

            IList<string> changedPropertyNames = delta.GetChangedPropertyNames().ToList();
            Assert.Single(changedPropertyNames);
            Assert.Equal("Name", String.Join(",", changedPropertyNames));

            IEdmEntityObject originalCustomer = null;
            foreach (var customer in AlternateKeysDataSource.Customers)
            {
                object value;
                if (customer.TryGetPropertyValue("SSN", out value))
                {
                    string stringKey = (string)value;
                    if (ssnKey == stringKey)
                    {
                        originalCustomer = customer;
                    }
                }
            }

            if (originalCustomer == null)
            {
                return NotFound();
            }

            object nameValue;
            delta.TryGetPropertyValue("Name", out nameValue);
            Assert.NotNull(nameValue);
            string strName = Assert.IsType<string>(nameValue);
            dynamic original = originalCustomer;
            original.Name = strName;

            return Ok(originalCustomer);
        }
    }

    public class OrdersController : ODataController
    {
        [HttpGet("odata/Orders({orderKey})")]
        public IActionResult GetOrderByPrimitiveKey(int orderKey)
        {
            foreach (var order in AlternateKeysDataSource.Orders)
            {
                object value;
                if (order.TryGetPropertyValue("OrderId", out value))
                {
                    int intKey = (int)value;
                    if (orderKey == intKey)
                    {
                        return Ok(order);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("odata/Orders(Name={orderName})")]
        public IActionResult GetOrderByName([FromODataUri]string orderName)
        {
            foreach (var order in AlternateKeysDataSource.Orders)
            {
                object value;
                if (order.TryGetPropertyValue("Name", out value))
                {
                    string stringKey = (string)value;
                    if (orderName == stringKey)
                    {
                        return Ok(order);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("odata/Orders(Token={token})")]
        public IActionResult GetOrderByToken([FromODataUri]Guid token)
        {
            foreach (var order in AlternateKeysDataSource.Orders)
            {
                object value;
                if (order.TryGetPropertyValue("Token", out value))
                {
                    Guid guidKey = (Guid)value;
                    if (token == guidKey)
                    {
                        return Ok(order);
                    }
                }
            }

            return NotFound();
        }
    }

    public class PeopleController : ODataController
    {
        public IActionResult Get(int key)
        {
            foreach (var person in AlternateKeysDataSource.People)
            {
                object value;
                if (person.TryGetPropertyValue("ID", out value))
                {
                    int intKey = (int)value;
                    if (key == intKey)
                    {
                        return Ok(person);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("odata/People(Country_Region={countryOrRegion},Passport={passport})")]
        public IActionResult FindPeopleByCountryAndPassport([FromODataUri]string countryOrRegion, [FromODataUri]string passport)
        {
            foreach (var person in AlternateKeysDataSource.People)
            {
                object value;
                if (person.TryGetPropertyValue("Country_Region", out value))
                {
                    string countryValue = (string)value;
                    if (person.TryGetPropertyValue("Passport", out value))
                    {
                        string passportValue = (string)value;
                        if (countryValue == countryOrRegion && passportValue == passport)
                        {
                            return Ok(person);
                        }
                    }
                }
            }

            return NotFound();
        }
    }

    public class CompaniesController : ODataController
    {
        public IActionResult Get(int key)
        {
            foreach (var company in AlternateKeysDataSource.Companies)
            {
                object value;
                if (company.TryGetPropertyValue("ID", out value))
                {
                    int intKey = (int)value;
                    if (key == intKey)
                    {
                        return Ok(company);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("odata/Companies(Code={code})")]
        public IActionResult GetCompaniesByCode(int code)
        {
            foreach (var company in AlternateKeysDataSource.Companies)
            {
                object value;
                if (company.TryGetPropertyValue("Code", out value))
                {
                    int intCode = (int)value;
                    if (code == intCode)
                    {
                        return Ok(company);
                    }
                }
            }

            return NotFound();
        }

        [HttpGet("odata/Companies(City={city},Street={street})")]
        public IActionResult GetCompanyByLocation([FromODataUri]string city, [FromODataUri]string street)
        {
            foreach (var company in AlternateKeysDataSource.Companies)
            {
                object value;
                if (company.TryGetPropertyValue("Location", out value))
                {
                    IEdmComplexObject location = value as IEdmComplexObject;
                    if (location == null)
                    {
                        return NotFound();
                    }

                    if (location.TryGetPropertyValue("City", out value))
                    {
                        string locCity = (string) value;

                        if (location.TryGetPropertyValue("Street", out value))
                        {
                            string locStreet = (string) value;
                            if (locCity == city && locStreet == street)
                            {
                                return Ok(company);
                            }
                        }
                    }
                }
            }

            return NotFound();
        }
    }
}
