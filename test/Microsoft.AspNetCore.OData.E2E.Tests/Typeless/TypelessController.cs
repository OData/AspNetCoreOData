//-----------------------------------------------------------------------------
// <copyright file="TypelessController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

public class TypelessDeltaController : ODataController
{
    [HttpGet]
    public EdmChangedObjectCollection GetChanges()
    {
        return TypelessDataSource.TypelessChangeSets;
    }
}

public class TypedDeltaController : ODataController
{
    [HttpGet]
    public DeltaSet<ChangeSet> GetChanges()
    {
        return TypelessDataSource.TypedChangeSets;
    }
}

public class TypelessOrdersController : ODataController
{
    [HttpGet("Orders")]
    public EdmEntityObjectCollection Get()
    {
        Request.Process();

        return TypelessDataSource.TypelessOrders;
    }

    [HttpGet("Orders({key})")]
    public IEdmEntityObject Get(int key)
    {
        Request.Process();

        var orderEntityObject = TypelessDataSource.TypelessOrders.FirstOrDefault(d =>
        {
            if (d.TryGetPropertyValue("Id", out object value) && value.Equals(key))
            {
                return true;
            }

            return false;
        });

        return orderEntityObject;
    }
}
