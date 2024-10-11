//-----------------------------------------------------------------------------
// <copyright file="DollarFilterDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class Person
{
    public int Id { get; set; }
    public string SSN { get; set; }
}

public class Product
{
    public string Id { get; set; }
    public string DeclaredSingleValuedProperty { get; set; }
    public List<int> DeclaredCollectionValuedProperty { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public List<Address> Addresses { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}

public enum Color
{
    Black,
    White
}
