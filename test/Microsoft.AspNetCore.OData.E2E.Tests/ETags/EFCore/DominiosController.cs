//-----------------------------------------------------------------------------
// <copyright file="DominiosController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ETags.EFCore;

public class DominiosController : ODataController
{
    private ETagCurrencyTokenEfContext _db;

    public DominiosController(ETagCurrencyTokenEfContext context)
    {
        _db = context;
        ETagCurrencyTokenEfContextInitializer.Seed(_db);
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_db.Dominios);
    }
}
