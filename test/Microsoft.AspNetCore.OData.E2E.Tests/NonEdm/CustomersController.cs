//-----------------------------------------------------------------------------
// <copyright file="CustomersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.NonEdm
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(ODataQueryOptions<Customer> options)
        {
            return Ok(options.ApplyTo(NonEdmDbContext.GetCustomers().AsQueryable()));
        }

        [HttpGet("WithEnableQueryAttribute")]
        [EnableQuery]
        public IActionResult GetWithEnableQueryAttribute()
        {
            return Ok(NonEdmDbContext.GetCustomers());
        }
    }
}
