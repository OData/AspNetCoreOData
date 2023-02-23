//-----------------------------------------------------------------------------
// <copyright file="DerivedTypesControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes
{
    public class CustomersController : ODataController
    {
        static List<Customer> Customers { get; set; }

        static CustomersController()
        {
            Customers = new List<Customer>
            {
                new Customer { 
                    Id = 1,
                    Name = "Customer 1",
                    Orders = new List<Order>
                    {
                        new Order { Id = 1, Amount = 100M }
                    }
                },
                new VipCustomer {
                    Id = 2,
                    Name = "Customer 2",
                    LoyaltyCardNo = "9876543210",
                    Orders = new List<Order>
                    {
                        new Order { Id = 2, Amount = 230M },
                        new Order { Id = 3, Amount = 150M }
                    }
                },
                new Customer {
                    Id = 3,
                    Name = "Customer 3",
                    Orders = new List<Order>
                    {
                        new Order { Id = 4, Amount = 170M }
                    }
                },
                new EnterpriseCustomer
                {
                    Id = 4,
                    Name = "Customer 4",
                    Orders = new List<Order>
                    {
                        new Order { Id = 5, Amount = 190M}
                    },
                    RelationshipManager = new Employee { Id = 1, Name = "Employee 1" }
                }
            };
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(Customers);
        }

        [EnableQuery]
        [HttpGet("odata/Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedTypes.VipCustomer({key})")] // convention routing doesn't create this template.
        public IActionResult Get(int key)
        {
            var customer = Customers.FirstOrDefault(c => c.Id == key);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        // Handles /entityset/cast path template
        [EnableQuery]
        public IActionResult GetFromVipCustomer()
        {
            return Ok(Customers.OfType<VipCustomer>());
        }

        // Handles /entityset/key/cast and /entityset/cast/key path templates
        [EnableQuery]
        public IActionResult GetVipCustomer([FromODataUri] int key)
        {
            var vipCustomer = Customers.OfType<VipCustomer>().SingleOrDefault(d => d.Id.Equals(key));

            if (vipCustomer == null)
            {
                return NotFound();
            }

            return Ok(vipCustomer);
        }

        [EnableQuery]
        public IActionResult GetEnterpriseCustomer([FromRoute] int key)
        {
            var enterpriseCustomer = Customers.OfType<EnterpriseCustomer>().SingleOrDefault(d => d.Id.Equals(key));

            if (enterpriseCustomer == null)
            {
                return NotFound();
            }

            return Ok(enterpriseCustomer);
        }
    }
}
