//-----------------------------------------------------------------------------
// <copyright file="MatchesPatternTimeoutController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.MatchesPatternTimeout;

internal static class MatchesPatternTimeoutDataSource
{
    private static readonly MatchesPatternProduct[] products = new[]
    {
        new MatchesPatternProduct { Id = 1, Name = "Alpha" },
        new MatchesPatternProduct { Id = 2, Name = "Beta" },
        new MatchesPatternProduct { Id = 3, Name = "Gamma" },
        new MatchesPatternProduct { Id = 4, Name = "Alabama" },
        new MatchesPatternProduct { Id = 5, Name = new string('a', 40) + "!" }, // requires extensive backtracking for the '(a+)+$' pattern
    };

    public static IEnumerable<MatchesPatternProduct> Products => products;
}

public class ProductsController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<MatchesPatternProduct>> Get()
    {
        return Ok(MatchesPatternTimeoutDataSource.Products);
    }
}

public class BoundedProductsController : ODataController
{
    [BoundedEnableQuery]
    public ActionResult<IEnumerable<MatchesPatternProduct>> Get()
    {
        return Ok(MatchesPatternTimeoutDataSource.Products);
    }
}

public class DefaultBoundedProductsController : ODataController
{
    [PagedEnableQuery]
    public ActionResult<IEnumerable<MatchesPatternProduct>> Get()
    {
        return Ok(MatchesPatternTimeoutDataSource.Products);
    }
}

/// <summary>
/// Serves the collection with a configured matchesPattern time span. A page size is also set so the
/// collection is truncated (and therefore materialized) while the bounded evaluation runs; the limit is
/// larger than the data set, so benign queries are returned in full.
/// </summary>
internal sealed class BoundedEnableQueryAttribute : EnableQueryAttribute
{
    public BoundedEnableQueryAttribute()
    {
        MatchesPatternTimeout = TimeSpan.FromMilliseconds(100);
        PageSize = 100;
    }
}

/// <summary>
/// Serves the collection with only a page size configured, so the default matchesPattern time span
/// applies while the collection is materialized during query execution. The limit is larger than the
/// data set, so benign queries are returned in full.
/// </summary>
internal sealed class PagedEnableQueryAttribute : EnableQueryAttribute
{
    public PagedEnableQueryAttribute()
    {
        PageSize = 100;
    }
}
