//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidationDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ApplyQueryValidation;

/// <summary>
/// The entity used by the apply/compute query validation tests. It carries allowed properties
/// (<see cref="Name"/>, <see cref="Amount"/>), a not-filterable string and numeric property
/// (<see cref="RestrictedName"/>, <see cref="RestrictedAmount"/>) and a property that is configured
/// as not selectable through model-bound query settings (<see cref="NotSelectableName"/>).
/// </summary>
public class ApplyValidationItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Amount { get; set; }

    // Configured as not selectable via model-bound query settings in the EDM model.
    public string NotSelectableName { get; set; }

    [NotFilterable]
    public string RestrictedName { get; set; }

    [NotFilterable]
    public int RestrictedAmount { get; set; }

    // A self-referencing collection so entity-set aggregates (aggregate(Related(... with ...)))
    // can be exercised end-to-end.
    public IList<ApplyValidationItem> Related { get; set; }
}

/// <summary>
/// A deterministic in-memory data source for <see cref="ApplyValidationItem"/>.
/// </summary>
public static class ApplyValidationDataSource
{
    private static readonly List<ApplyValidationItem> Data = BuildData();

    private static List<ApplyValidationItem> BuildData()
    {
        var item1 = new ApplyValidationItem { Id = 1, Name = "Alpha", Amount = 10, NotSelectableName = "S1", RestrictedName = "R1", RestrictedAmount = 100 };
        var item2 = new ApplyValidationItem { Id = 2, Name = "Beta", Amount = 20, NotSelectableName = "S2", RestrictedName = "R2", RestrictedAmount = 200 };
        var item3 = new ApplyValidationItem { Id = 3, Name = "Alpha", Amount = 30, NotSelectableName = "S3", RestrictedName = "R3", RestrictedAmount = 300 };

        item1.Related = new List<ApplyValidationItem> { item2, item3 };
        item2.Related = new List<ApplyValidationItem> { item3 };
        item3.Related = new List<ApplyValidationItem> { item1 };

        return new List<ApplyValidationItem> { item1, item2, item3 };
    }

    public static IQueryable<ApplyValidationItem> Items => Data.AsQueryable();
}
