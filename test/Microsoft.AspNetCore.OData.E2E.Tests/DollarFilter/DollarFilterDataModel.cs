//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class Person
{
    public int Id { get; set; }
    public string SSN { get; set; }
}

public class Vendor
{
    public int Id { get; set; }
    public VendorAddress DeclaredSingleValuedProperty { get; set; }
    public int DeclaredPrimitiveProperty { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class VendorAddress
{
    public string Street { get; set; }
    public VendorCity City { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class VendorCity
{
    public string Name { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class NonOpenVendorAddress
{
    public string Street { get; set; }
}

public class NotInModelVendorAddress
{
    public string Street { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public ContactInfo DeclaredContactInfo { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Address
{
    public string DeclaredStreet { get; set; }
    public List<Floor> DeclaredFloors { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class ContactInfo
{
    public List<string> DeclaredEmails { get; set; }
    public List<Address> DeclaredAddresses { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}
public class NonOpenAddress
{
    public string DeclaredStreet { get; set; }
}

public class NotInModelAddress
{
    public string DeclaredStreet { get; set; }
}

public class PropertyIsNotCollectionContactInfo
{
    public Address DeclaredAddress { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Floor
{
    public int DeclaredNumber { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Product
{
    public string Id { get; set; }
    public string DeclaredSingleValuedProperty { get; set; }
    public List<int> DeclaredCollectionValuedProperty { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Basket
{
    public int Id { get; set; }
    public List<Fruit> DeclaredCollectionValuedProperty { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class Fruit
{
    public string Name { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class BasicType
{
    public int Id { get; set; }
    public LiteralInfo DeclaredLiteralInfo { get; set; }
    public List<LiteralInfo> DeclaredLiteralInfos { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class LiteralInfo
{
    public bool DeclaredBooleanProperty { get; set; }
    public byte DeclaredByteProperty { get; set; }
    public sbyte DeclaredSignedByteProperty { get; set; }
    public short DeclaredInt16Property { get; set; }
    public int DeclaredInt32Property { get; set; }
    public long DeclaredInt64Property { get; set; }
    public float DeclaredSingleProperty { get; set; }
    public double DeclaredDoubleProperty { get; set; }
    public decimal DeclaredDecimalProperty { get; set; }
    public Guid DeclaredGuidProperty { get; set; }
    public string DeclaredStringProperty { get; set; }
    public TimeSpan DeclaredTimeSpanProperty { get; set; }
    public Microsoft.OData.Edm.TimeOfDay DeclaredTimeOfDayProperty { get; set; }
    public Microsoft.OData.Edm.Date DeclaredDateProperty { get; set; }
    public DateTimeOffset DeclaredDateTimeOffsetProperty { get; set; }
    public Color DeclaredEnumProperty { get; set; }
    public byte[] DeclaredByteArrayProperty { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public enum Color
{
    Black,
    White
}

public class Catalog
{
    public int Id { get; set; }
    public string DeclaredName { get; set; }
    public string IgnoredName { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class CatalogInfo
{
    public string DeclaredCode { get; set; }
    public string RenamedCode { get; set; }
    public string IgnoredCode { get; set; }
    public List<CatalogTag> DeclaredTags { get; set; }
    public List<CatalogTag> IgnoredTags { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

public class CatalogTag
{
    public string DeclaredLabel { get; set; }
    public Dictionary<string, object> DynamicProperties { get; set; }
}

// A non-open complex type (no dynamic-property container) whose SecretSummaryCode CLR member is
// excluded from the EDM model via [NotMapped]. Used to prove that a property segment addressing an
// excluded CLR member on a non-open modeled type is rejected rather than binding (and exposing) it.
public class CatalogSummary
{
    public string DeclaredSummaryCode { get; set; }

    [NotMapped]
    public string SecretSummaryCode { get; set; }
}
