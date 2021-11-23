//-----------------------------------------------------------------------------
// <copyright file="DefaultODataBatchHandlerController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch
{
    public class DefaultBatchCustomersController : ODataController
    {
        private static IList<DefaultBatchCustomer> _customers = Enumerable.Range(0, 10).Select(i =>
            new DefaultBatchCustomer
            {
                Id = i,
                Name = string.Format("Name {0}", i)
            }).ToList();

        public IActionResult Get(int key)
        {
            DefaultBatchCustomer customers = _customers.FirstOrDefault(c => c.Id == key);
            if (customers == null)
            {
                return BadRequest();
            }

            return Ok(customers);
        }

        [EnableQuery]
        public IQueryable<DefaultBatchCustomer> OddCustomers()
        {
            return _customers.Where(x => x.Id % 2 == 1).AsQueryable();
        }

        public IActionResult Post([FromBody] DefaultBatchCustomer customer)
        {
            _customers.Add(customer);
            return Created(customer);
        }

        public Task CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            return Task.FromResult(StatusCode(StatusCodes.Status204NoContent));
        }
    }

    public class DefaultBatchOrdersController : ODataController
    {
        public DefaultBatchOrdersController()
        {
        }
    }
}
