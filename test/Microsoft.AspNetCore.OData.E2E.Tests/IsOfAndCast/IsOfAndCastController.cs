//-----------------------------------------------------------------------------
// <copyright file="IsOfAndCastController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast;

public class IsOfAndCastController : ODataController
{
    private static IsOfAndCastDataSource _dataSource = new IsOfAndCastDataSource();

    [EnableQuery]
    [HttpGet("odata/products")]
    public IActionResult GetProducts()
    {
        return Ok(_dataSource.Products);
    }

    [EnableQuery]
    [HttpGet("odata/orders")]
    public IActionResult GetOrders()
    {
        return Ok(_dataSource.Orders);
    }
}
