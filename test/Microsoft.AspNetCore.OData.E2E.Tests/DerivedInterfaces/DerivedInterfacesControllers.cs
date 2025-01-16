//-----------------------------------------------------------------------------
// <copyright file="DerivedInterfacesControllers.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.E2E.Tests.DerivedInterfaces
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
                    Order = new VipOrder()
                    {
                        Id = 10,
                        Product = "Test Product 1",
                        LoyaltyCardNo = "42"
                    }
                },
                new Customer {
                    Id = 2,
                    Name = "Customer 1",
                    Order = new VipOrder()
                    {
                        Id = 11,
                        Product = "Test Product 2",
                        LoyaltyCardNo = "43"
                    }
                },
            };
        }

        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.All)]
        [HttpGet("odata/Customers/Microsoft.AspNetCore.OData.E2E.Tests.DerivedInterfaces.Customers")]
        public IActionResult Get()
        {
            return Ok(Customers);
        }
    }
}
