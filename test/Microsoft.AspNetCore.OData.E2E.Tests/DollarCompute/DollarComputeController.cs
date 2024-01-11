//-----------------------------------------------------------------------------
// <copyright file="DollarComputeController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarCompute
{
    public class CustomersController : ODataController
    {
        [EnableQuery(PageSize = 2)]
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

    public class StudentsController : ODataController
    {
        private static IList<ComputeStudent> _students = new List<ComputeStudent>
        {
            new ComputeStudent { Id = 1, Name = "cc" },
            new ComputeStudent { Id = 2, Name = "dd" },
            new ComputeStudent { Id = 3, Name = "AA"},
            new ComputeStudent { Id = 4, Name = "DD" },
            new ComputeStudent { Id = 5, Name = "BB" },
            new ComputeStudent { Id = 6, Name = "CC" },
            new ComputeStudent { Id = 7, Name = "aa" },
            new ComputeStudent { Id = 8, Name = "bb" },
        };

        [EnableQuery(PageSize = 2)]
        public IActionResult Get()
        {
            return Ok(_students);
        }
    }
}
