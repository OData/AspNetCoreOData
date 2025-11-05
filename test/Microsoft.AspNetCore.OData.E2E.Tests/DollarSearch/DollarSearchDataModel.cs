//-----------------------------------------------------------------------------
// <copyright file="DollarSearchDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarSearch;

public class SearchProduct
{
    public int Id { get; set; }

    public string Name { get; set; }

    public SearchColor Color { get; set; }

    public double Price { get; set; }

    public int Qty { get; set; }

    public SearchCategory Category { get; set; }

    public List<SearchTag> Tags { get; set; } // List of tags
}

public class SearchCategory
{
    public int Id { get; set; }

    public string Name { get; set; }
}

public class SearchTag
{
    public int Id { get; set; }

    public string Name { get; set; } // Tag name (e.g., "Telemetry", "Privacy", "Security")

    public string Description { get; set; }
}

public enum SearchColor
{
    White,

    Red,

    Green,

    Blue,

    Brown
}
