//-----------------------------------------------------------------------------
// <copyright file="Product.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Models;

public class Product
{
    public int ProductID { get; set; }

    public string ProductName { get; set; }
    public int SupplierID { get; set; }
    public int CategoryID { get; set; }
    public string QuantityPerUnit { get; set; }
    public decimal? UnitPrice { get; set; }
    public double? Weight { get; set; }
    public float? Width { get; set; }
    public short? UnitsInStock { get; set; }
    public short? UnitsOnOrder { get; set; }

    public short? ReorderLevel { get; set; }
    public bool? Discontinued { get; set; }
    public DateTimeOffset? DiscontinuedDate { get; set; }
    public DateTime Birthday { get; set; }

    public DateTimeOffset NonNullableDiscontinuedDate { get; set; }
    [NotFilterable]
    public DateTimeOffset NotFilterableDiscontinuedDate { get; set; }

    public DateTimeOffset DiscontinuedOffset { get; set; }
    public TimeSpan DiscontinuedSince { get; set; }

    public Date DateProperty { get; set; }
    public Date? NullableDateProperty { get; set; }

    public Guid GuidProperty { get; set; }
    public Guid? NullableGuidProperty { get; set; }

    public TimeOfDay TimeOfDayProperty { get; set; }
    public TimeOfDay? NullableTimeOfDayProperty { get; set; }

    public ushort? UnsignedReorderLevel { get; set; }

    public SimpleEnum Ranking { get; set; }

    public Category Category { get; set; }

    public Address SupplierAddress { get; set; }

    public int[] AlternateIDs { get; set; }
    public Address[] AlternateAddresses { get; set; }
    [NotFilterable]
    public Address[] NotFilterableAlternateAddresses { get; set; }
}

public class DerivedProduct : Product
{
    public string DerivedProductName { get; set; }
}

public class DynamicProduct : Product
{
    public Dictionary<string, object> ProductProperties { get; set; }
}
