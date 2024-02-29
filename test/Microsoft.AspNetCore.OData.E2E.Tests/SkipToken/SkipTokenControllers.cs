//-----------------------------------------------------------------------------
// <copyright file="SkipTokenControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SkipToken
{
    public class SkipTokenControllers : ODataController
    {
        private static IList<StCustomer> _stCustomers;
        private static IList<StOrder> _stOrders;

        #region Data
        static SkipTokenControllers()
        {
            IList<StAddress> stAddresses = new List<StAddress>()
            {
                new StAddress
                {
                    State = "WA",
                    City = "Settle",
                    ZipCode = 9811,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Region", "Bb" }
                    }
                },

                new StAddress
                {
                    State = "OT",
                    City = "Perry",
                    ZipCode = 1831,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Region", "Aa" }
                    }
                },

                new StAddress
                {
                    State = "AJ",
                    City = "Reedy",
                    ZipCode = 7817,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Region", "Cc" }
                    }
                }
            };

            _stCustomers = new List<StCustomer>
            {
                new StCustomer
                {
                    Id = 1,
                    Name = "Mars",
                    Age = 871,
                    Birthday = new DateTimeOffset(1999, 1, 2, 1, 1, 2, 3, TimeSpan.Zero),
                    MagicNumber = -88,
                    PhoneNumbers = new [] { "M-110", "M-019", "M-712"},
                    FavoritePlace = stAddresses[1],
                    Orders = new List<StOrder>
                    {
                        new StOrder{ RegId = "A9", Id = 11, Amount = 66, Location = stAddresses[0] },
                        new StOrder{ RegId = "A9", Id = 12, Amount = 42, Location = stAddresses[2] },
                        new StOrder{ RegId = "A9", Id = 13, Amount = 17, Location = stAddresses[1] }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Detail", "Vip" }
                    }
                },
                new StCustomer
                {
                    Id = 2,
                    Name = "Earth",
                    Age = 79,
                    Birthday = new DateTimeOffset(2001, 4, 6, 6, 1, 4, 3, TimeSpan.Zero),
                    MagicNumber = -86,
                    PhoneNumbers = new [] { "E-819" },
                    FavoritePlace = stAddresses[2],
                    Orders = new List<StOrder>
                    {
                        new StOrder{ RegId = "A8", Id = 2, Amount = 18, Location = stAddresses[1] },
                        new StOrder{ RegId = "A9", Id = 5, Amount = 87, Location = stAddresses[0] }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Detail", "Regular" }
                    }
                },
                new StCustomer
                {
                    Id = 3,
                    Name = "Apply",
                    Age = 1001,
                    Birthday = new DateTimeOffset(1948, 12, 6, 9, 11, 14, 5, TimeSpan.Zero),
                    MagicNumber = -90,
                    PhoneNumbers = new [] { "A-123", "A-819" },
                    FavoritePlace = stAddresses[0],
                    Orders = new List<StOrder>
                    {
                        new StOrder{ RegId = "A8", Id = 3, Amount = 78, Location = stAddresses[2] }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Detail", "Regular" }
                    }
                },
                new StCustomer
                {
                    Id = 4,
                    Name = "Apple",
                    Age = 11,
                    Birthday = new DateTimeOffset(2024, 2, 12, 3, 5, 8, 13, TimeSpan.Zero),
                    MagicNumber = -84,
                    PhoneNumbers = new [] { "A-444", "A-888", "A-111", "A-446" },
                    FavoritePlace = stAddresses[2],
                    Orders = new List<StOrder>
                    {
                        new StOrder{ RegId = "A8", Id = 31, Amount = 78, Location = stAddresses[2] }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Detail", "Vip" }
                    }
                },
                new StCustomer
                {
                    Id = 5,
                    Name = "Kare",
                    Age = 101,
                    Birthday = new DateTimeOffset(1978, 11, 15, 13, 15, 18, 23, TimeSpan.Zero),
                    MagicNumber = -85,
                    PhoneNumbers = new [] { "K-023", "K-919", "K-119", "K-745" },
                    FavoritePlace = stAddresses[1],
                    Orders = new List<StOrder>
                    {
                        new StOrder{ RegId = "A9", Id = 83, Amount = 14, Location = stAddresses[1] },
                        new StOrder{ RegId = "A9", Id = 65, Amount = 42, Location = stAddresses[0] }
                    },
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Detail", "Vip" }
                    }
                }
            };

            List<StOrder> orders = new List<StOrder>();
            foreach (var c in _stCustomers)
            {
                orders.AddRange(c.Orders);
            }

            _stOrders = orders;
        }
        #endregion

        [EnableQuery(PageSize = 2)]
        [HttpGet("/odata/customers")]
        public IActionResult Get()
        {
            return Ok(_stCustomers);
        }

        [EnableQuery(PageSize = 2)]
        [HttpGet("/odata/customers/{id}")]
        public IActionResult Get(int id)
        {
            StCustomer customer = _stCustomers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [EnableQuery(PageSize = 2)]
        [HttpGet("/odata/orders")]
        public IActionResult GetOrders()
        {
            return Ok(_stOrders);
        }

        [EnableQuery]
        [HttpGet("all/customers")] // no page size, used to get all customers for reference
        public IActionResult GetAllCustomers()
        {
            return Ok(_stCustomers);
        }

        [EnableQuery]
        [HttpGet("all/orders")] // no page size, used to get all orders for reference
        public IActionResult GetAllOrders()
        {
            return Ok(_stOrders);
        }
    }
}
