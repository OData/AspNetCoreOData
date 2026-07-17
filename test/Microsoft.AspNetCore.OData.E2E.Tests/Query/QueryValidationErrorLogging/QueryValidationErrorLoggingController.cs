//-----------------------------------------------------------------------------
// <copyright file="QueryValidationErrorLoggingController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationErrorLogging;

public class LoggingCustomersController : ControllerBase
{
    [HttpGet]
    [EnableQuery(EnableQueryValidationErrorLogging = true)]
    public ActionResult<IEnumerable<LoggingCustomer>> Get()
    {
        return new List<LoggingCustomer>
        {
            new LoggingCustomer { Id = 1, Name = "Alice" },
        };
    }
}

public class PlainCustomersController : ControllerBase
{
    [HttpGet]
    [EnableQuery]
    public ActionResult<IEnumerable<LoggingCustomer>> Get()
    {
        return new List<LoggingCustomer>
        {
            new LoggingCustomer { Id = 1, Name = "Alice" },
        };
    }
}

public class OptOutCustomersController : ControllerBase
{
    [HttpGet]
    [EnableQuery(EnableQueryValidationErrorLogging = false)]
    public ActionResult<IEnumerable<LoggingCustomer>> Get()
    {
        return new List<LoggingCustomer>
        {
            new LoggingCustomer { Id = 1, Name = "Alice" },
        };
    }
}

[Route("postaction")]
public class PostActionLoggingCustomersController : ControllerBase
{
    // The non-generic IActionResult return type cannot be resolved before the action runs, so the query is
    // validated on the post-action path. This exercises that path for the logging diagnostic.
    [HttpGet("customers")]
    [EnableQuery(EnableQueryValidationErrorLogging = true)]
    public IActionResult Get()
    {
        IQueryable<LoggingCustomer> customers = new List<LoggingCustomer>
        {
            new LoggingCustomer { Id = 1, Name = "Alice" },
        }.AsQueryable();

        return Ok(customers);
    }
}
