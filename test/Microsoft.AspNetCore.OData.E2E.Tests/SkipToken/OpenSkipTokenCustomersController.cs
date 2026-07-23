//-----------------------------------------------------------------------------
// <copyright file="OpenSkipTokenCustomersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SkipToken;

public class OpenSkipTokenCustomersController : ODataController
{
    // 8 customers with dynamic properties: Score (int), Tag (string), IsActive (bool), Status (string).
    // PageSize = 2, giving 4 pages per ordering.
    private static readonly IList<OpenSkipTokenCustomer> _customers = new List<OpenSkipTokenCustomer>
    {
        new OpenSkipTokenCustomer { Id = 1, Name = "Alice",  DynamicProperties = new Dictionary<string, object> { { "Score", 80 }, { "Tag", "Gold"   }, { "IsActive", true  }, { "Status", "Premium"  } } },
        new OpenSkipTokenCustomer { Id = 2, Name = "Bob",    DynamicProperties = new Dictionary<string, object> { { "Score", 30 }, { "Tag", "Bronze" }, { "IsActive", false }, { "Status", "Standard" } } },
        new OpenSkipTokenCustomer { Id = 3, Name = "Carol",  DynamicProperties = new Dictionary<string, object> { { "Score", 60 }, { "Tag", "Silver" }, { "IsActive", true  }, { "Status", "Premium"  } } },
        new OpenSkipTokenCustomer { Id = 4, Name = "Dave",   DynamicProperties = new Dictionary<string, object> { { "Score", 90 }, { "Tag", "Gold"   }, { "IsActive", false }, { "Status", "Standard" } } },
        new OpenSkipTokenCustomer { Id = 5, Name = "Eve",    DynamicProperties = new Dictionary<string, object> { { "Score", 50 }, { "Tag", "Bronze" }, { "IsActive", true  }, { "Status", "Gold"     } } },
        new OpenSkipTokenCustomer { Id = 6, Name = "Frank",  DynamicProperties = new Dictionary<string, object> { { "Score", 70 }, { "Tag", "Silver" }, { "IsActive", true  }, { "Status", "Gold"     } } },
        new OpenSkipTokenCustomer { Id = 7, Name = "Grace",  DynamicProperties = new Dictionary<string, object> { { "Score", 40 }, { "Tag", "Gold"   }, { "IsActive", false }, { "Status", "Standard" } } },
        new OpenSkipTokenCustomer { Id = 8, Name = "Henry",  DynamicProperties = new Dictionary<string, object> { { "Score", 20 }, { "Tag", "Bronze" }, { "IsActive", true  }, { "Status", "Premium"  } } },
    };

    [EnableQuery(PageSize = 2)]
    [HttpGet("/open/customers")]
    public IActionResult Get()
    {
        return Ok(_customers);
    }
}
