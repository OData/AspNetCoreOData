//-----------------------------------------------------------------------------
// <copyright file="DetachedQueryOptionsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.E2E.Tests.QueryOptionsFromDictionary;

[ApiController]
[Route("api/detachedcustomers")]
public class DetachedQueryOptionsController : ControllerBase
{
    private static readonly IEdmModel Model = DetachedQueryOptionsEdmModel.GetEdmModel();

    // Applies query options supplied as a JSON dictionary body, constructing the
    // ODataQueryOptions<T> WITHOUT any HttpRequest (the stand-alone/detached scenario
    // enabled by the new dictionary constructor).
    [HttpPost("apply")]
    public IActionResult Apply([FromBody] Dictionary<string, string> queryOptions)
    {
        var options = new ODataQueryOptions<DetachedCustomer>(
            queryOptions ?? new Dictionary<string, string>(),
            Model);

        IQueryable<DetachedCustomer> data = DetachedQueryOptionsDataSource.Customers.AsQueryable();

        var result = options.ApplyTo(data).Cast<DetachedCustomer>().ToList();

        return Ok(result);
    }
}
