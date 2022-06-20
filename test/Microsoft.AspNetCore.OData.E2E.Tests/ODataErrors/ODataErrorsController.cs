//-----------------------------------------------------------------------------
// <copyright file="ODataErrorsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors
{
    public class CustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            return Unauthorized("Not authorized to access this resource.");
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return NotFound($"Customer with key: {key} not found.");
        }

        public IActionResult Post([FromBody] Customer customer)
        {
            return UnprocessableEntity("Unprocessable customer object.");
        }

        public IActionResult Patch([FromODataUri] int key,[FromBody] Customer customer)
        {
            return Conflict("Conflict during update.");
        }

        public IActionResult Put([FromODataUri] int key, [FromBody] Customer customer)
        {
            return ODataErrorResult("400", "Bad request during PUT.");
        }

        public IActionResult Delete(int key)
        {
            return BadRequest("Bad request on delete.");
        }
    }

    public class OrdersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            return NotFound();
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            return ODataErrorResult("404", $"Order with key: {key} not found.");
        }

        [EnableQuery]
        public IActionResult Post()
        {
            return UnprocessableEntity("Unprocessable order object.");
        }

        [EnableQuery]
        public IActionResult Patch()
        {
            return Conflict("Conflict on patch.");
        }

        [EnableQuery]
        public IActionResult Delete()
        {
            return BadRequest("Bad request on delete.");
        }
    }
}
