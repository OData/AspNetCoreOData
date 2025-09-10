//-----------------------------------------------------------------------------
// <copyright file="IsOfAndCastDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IsOfAndCast;

public class Product
{
    [Key]
    public int ID { get; set; }
    public string Name { get; set; }
    public Domain Domain { get; set; }
    public Double Weight { get; set; }
}

[Flags]
public enum Domain
{
    Military = 1,
    Civil = 2,
    Both = 3,
}

public class AirPlane : Product
{
    public int Speed { get; set; }
    public string Model { get; set; }
}

public class JetPlane : AirPlane
{
    public string JetType { get; set; }
}

public class Order
{
    [Key]
    public int OrderID { get; set; }
    public Address Location { get; set; }
    public IList<Product> Products { get; set; }
}

public class Address
{
    public string City { get; set; }
}

public class HomeAddress : Address
{
    public string HomeNo { get; set; }
}

