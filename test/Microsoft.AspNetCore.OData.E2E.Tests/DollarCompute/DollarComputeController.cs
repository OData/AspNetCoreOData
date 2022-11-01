//-----------------------------------------------------------------------------
// <copyright file="DollarComputeController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute
{
    public class CustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(DollarComputeDataSource.Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            ComputeCustomer c = DollarComputeDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find customer with key = {key}");
            }

            return Ok(c);
        }

        [EnableQuery]
        public IActionResult GetLocation(int key)
        {
            ComputeCustomer c = DollarComputeDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find customer with key = {key}");
            }

            return Ok(c.Location);
        }

        [HttpGet("odata/sales")]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.None)]
        public IActionResult GetSales()
        {
            return Ok();
        }
    }
}
