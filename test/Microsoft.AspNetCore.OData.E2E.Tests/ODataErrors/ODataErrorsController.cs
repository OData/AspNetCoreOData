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
using Microsoft.OData;

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
            ODataError odataError = new ODataError()
            {
                Code = "401",
                Message = "Not authorized to access this resource."
            };
            return Unauthorized(odataError);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            ODataError odataError = new ODataError()
            {
                Code = "404",
                Message = $"Order with key: {key} not found."
            };
            return NotFound(odataError);
        }

        public IActionResult Post([FromBody] Order order)
        {
            ODataError odataError = new ODataError()
            {
                Code = "422",
                Message = "Unprocessable order object."
            };
            return UnprocessableEntity(odataError);
        }

        public IActionResult Patch([FromODataUri] int key, [FromBody] Order order)
        {
            ODataError odataError = new ODataError()
            {
                Code = "409",
                Message = "Conflict during update."
            };
            return Conflict(odataError);
        }

        public IActionResult Put([FromODataUri] int key, [FromBody] Order order)
        {
            ODataError odataError = new ODataError()
            {
                Code = "400",
                Message = "Bad request during PUT."
            };
            return ODataErrorResult(odataError);
        }

        public IActionResult Delete(int key)
        {
            ODataError odataError = new ODataError()
            {
                Code = "400",
                Message = "Bad request on delete."
            };
            return BadRequest(odataError);
        }
    }
}
