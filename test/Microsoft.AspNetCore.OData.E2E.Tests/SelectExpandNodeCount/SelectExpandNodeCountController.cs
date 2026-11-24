//-----------------------------------------------------------------------------
// <copyright file="SelectExpandNodeCountController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SelectExpandNodeCount;

public class LimitedProjectionItemsController : ODataController
{
    [EnableQuery(MaxSelectExpandNodeCount = 5)]
    public IQueryable<SelectExpandNodeCountItem> Get()
    {
        return SelectExpandNodeCountDataSource.Items.AsQueryable();
    }
}

public class DefaultProjectionItemsController : ODataController
{
    [EnableQuery]
    public IQueryable<SelectExpandNodeCountItem> Get()
    {
        return SelectExpandNodeCountDataSource.Items.AsQueryable();
    }
}

public class LevelsProjectionItemsController : ODataController
{
    // MaxExpansionDepth is raised above the $levels values used in the tests so $levels can scale the projection
    // without tripping the depth check first, while staying small enough that an accepted request materializes only a
    // shallow chain. MaxSelectExpandNodeCount stays low so the level-weighted count is what bounds the request. This
    // mirrors a server that has increased the expansion depth but still relies on the node count to keep the
    // projection tree from growing without limit.
    [EnableQuery(MaxSelectExpandNodeCount = 12, MaxExpansionDepth = 4)]
    public IQueryable<SelectExpandNodeCountItem> Get()
    {
        return SelectExpandNodeCountDataSource.Items.AsQueryable();
    }
}
