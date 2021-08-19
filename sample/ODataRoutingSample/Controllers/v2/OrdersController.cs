//-----------------------------------------------------------------------------
// <copyright file="OrdersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers.v2
{
    [ODataRouteComponent("v2{data}")]
    public class OrdersController : ODataController
    {
        private MyDataContext _context;

        public OrdersController(MyDataContext context)
        {
            _context = context;

            if (_context.Orders.Count() == 0)
            {
                IList<Order> orders = new List<Order>
                {
                    new Order
                    {
                        Title = "Goods",
                        Category = new Category { },
                    },
                    new Order
                    {
                        Title = "Magazine",
                        Category = new Category { },
                    },
                    new Order
                    {
                        Title = "Fiction",
                        Category = new Category { },
                    },
                };

                foreach (var order in orders)
                {
                    _context.Orders.Add(order);
                }

                _context.SaveChanges();
            }
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Order> Get()
        {
            return _context.Orders;
        }

        [HttpGet] // ~/Orders({key})
        [EnableQuery]
        public Order Get(int key)
        {
            return new Order
            {
                Id = key,
                Title = "Title + " + key
            };
        }

        [HttpPost]
        [EnableQuery]
        public IActionResult Post([FromBody] Order order, CancellationToken token)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
            return Created(order);
        }

        [HttpDelete] // ~/Orders({key})
        public string Delete(int key)
        {
            return $"Delete Order at {key}";
        }

       // [Http] // ~/Orders({key})
        [HttpPatch]
        public string Patch(int key)
        {
            return $"Patch Order at {key}";
        }

        // http://localhost:5000/v21/Orders(2)/CanMoveToAddress(address={"City":"abc","Street":"fdsfg"})
        // http://localhost:5000/v21/Orders(2)/CanMoveToAddress(address=@p)?@p={"City":"abc","Street":"fdsfg"}
        [HttpGet]
        public IActionResult CanMoveToAddress(int key, [FromODataUri] Address address)
        {
            if (address == null)
            {
                return NotFound("address is null");
            }

            return Ok(System.Text.Json.JsonSerializer.Serialize(address));
        }

        // http://localhost:5000/v21/Orders/CanMoveToManyAddress(addresses=[{"City":"abc","Street":"sfg"},{"City":"ab2c","Street":"fdsfg"}])
        // http://localhost:5000/v21/Orders/CanMoveToManyAddress(addresses=@p)?@p=[{"City":"abc","Street":"sfg"},{"City":"ab2c","Street":"fdsfg"}]
        [HttpGet]
        public IActionResult CanMoveToManyAddress([FromODataUri] IEnumerable<Address> addresses)
        {
            if (addresses == null)
            {
                return NotFound("addresses is null");
            }

            return Ok(System.Text.Json.JsonSerializer.Serialize(addresses));
        }

        [HttpGet]
        public string GetWholeSalary(int key, int minSalary, int maxSalary, int aveSalary)
        {
            return $"Orders/{key}/GetWholeSalary: {minSalary}, {maxSalary}, {aveSalary}" ;
        }


        [HttpGet]
        public string GetProperty(int key, string property)
        {
            return $"{property} in order";
        }

        [HttpGet]
        public string GetTitle(int key)
        {
            return "Orders Title";
        }

        [HttpPost]
        public string PostToCategory(int key)
        {
            return "PostToCategory + " + key;
        }

        [HttpPost]
        public string PostToCategoryFromVipOrder(int key)
        {
            return "PostToCategoryFromVipOrder + " + key;
        }

        [HttpPost]
        public string PostToCategoryFromUnknowOrder(int key)
        {
            return "PostToCategoryFromUnknowOrder + " + key;
        }

        [HttpPost]
        public string CreateRefToCategory(int key)
        {
            return "CreateRefToCategory";
        }
    }
}

// Request using the $batch
/*
{
  "requests":[
      {
      "id": "2",
      "atomicityGroup": "transaction",
      "method": "post",
      "url": "/v2bla/Orders",
      "headers": { "content-type": "application/json", "Accept": "application/json", "odata-version": "4.0" },
      "body": {"Title":"MyName11"}
      },
      {
      "id": "3",
      "atomicityGroup": "transaction",
      "method": "post",
      "url": "/v2bla/Orders",
      "headers": { "content-type": "application/json", "Accept": "application/json", "odata-version": "4.0" },
      "body": {"Title":"MyName12"}
      },
      {
      "id": "4",
      "atomicityGroup": "transaction",
      "method": "post",
      "url": "/v2bla/Orders",
      "headers": { "content-type": "application/json", "Accept": "application/json", "odata-version": "4.0" },
      "body": {"Title":"MyName13"}
      }
  ]
}
*/

// Response
/*
--batchresponse_abbbc5f4-f310-4f28-ae2f-ebd787f78a16
Content-Type: multipart/mixed; boundary=changesetresponse_ca691c38-cf02-45d9-b789-a909cdf1c72b

--changesetresponse_ca691c38-cf02-45d9-b789-a909cdf1c72b
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 2

HTTP/1.1 201 Created
Location: http://localhost:5000/v2bla/Orders/Orders(4)
Content-Type: application/json; odata.metadata=minimal; odata.streaming=true
OData-Version: 4.0

{"@odata.context":"http://localhost:5000/v2bla/Orders/$metadata#Orders/$entity","Id":4,"Title":"MyName11"}
--changesetresponse_ca691c38-cf02-45d9-b789-a909cdf1c72b
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 3

HTTP/1.1 201 Created
Location: http://localhost:5000/v2bla/Orders/Orders(5)
Content-Type: application/json; odata.metadata=minimal; odata.streaming=true
OData-Version: 4.0

{"@odata.context":"http://localhost:5000/v2bla/Orders/$metadata#Orders/$entity","Id":5,"Title":"MyName12"}
--changesetresponse_ca691c38-cf02-45d9-b789-a909cdf1c72b
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 4

HTTP/1.1 201 Created
Location: http://localhost:5000/v2bla/Orders/Orders(6)
Content-Type: application/json; odata.metadata=minimal; odata.streaming=true
OData-Version: 4.0

{"@odata.context":"http://localhost:5000/v2bla/Orders/$metadata#Orders/$entity","Id":6,"Title":"MyName13"}
--changesetresponse_ca691c38-cf02-45d9-b789-a909cdf1c72b--
--batchresponse_abbbc5f4-f310-4f28-ae2f-ebd787f78a16--
*/
