//-----------------------------------------------------------------------------
// <copyright file="MediaTypesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MediaTypes;

public class OrdersController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Order>> Get()
    {
        return MediaTypesDataSource.Orders;
    }

    [EnableQuery]
    public ActionResult<Order> Get(int key)
    {
        var order = MediaTypesDataSource.Orders.SingleOrDefault(d => d.Id == key);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }
}
