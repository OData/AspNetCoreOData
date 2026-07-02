//-----------------------------------------------------------------------------
// <copyright file="NotMappedPropertyController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.TypedEdm;

public class NotMappedPropertyController : ODataController
{
    private static readonly IList<UserAccount> _accounts = new List<UserAccount>
    {
        new UserAccount { Id = 1, Name = "Alice", PasswordHash = "hash_for_alice", DynamicProperties = new Dictionary<string, object>() },
        new UserAccount { Id = 2, Name = "Bob",   PasswordHash = "hash_for_bob",   DynamicProperties = new Dictionary<string, object>() },
        new UserAccount { Id = 3, Name = "Carol", PasswordHash = "hash_for_carol", DynamicProperties = new Dictionary<string, object>() },
    };

    [EnableQuery(PageSize = 2)]
    [HttpGet("/auth/accounts")]
    public IActionResult Get() => Ok(_accounts);
}
