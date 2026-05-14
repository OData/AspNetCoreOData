//-----------------------------------------------------------------------------
// <copyright file="MessageSizeController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

public class MessageSizeItemsController : ODataController
{
    [HttpPost]
    public IActionResult Post([FromBody] MessageSizeItem item)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Created(item);
    }

    [HttpPut]
    public IActionResult Put(int key, [FromBody] MessageSizeItem item)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        item.Id = key;
        return Ok(item);
    }
}
