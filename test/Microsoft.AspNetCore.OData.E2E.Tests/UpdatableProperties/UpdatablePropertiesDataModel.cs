//-----------------------------------------------------------------------------
// <copyright file="UpdatablePropertiesDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.UpdatableProperties;

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Address Address { get; set; }

    [AutoExpand]
    [Contained]
    public Order Order { get; set; }
}

public class Address
{
    public string City { get; set; }

    public string Street { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public string Description { get; set; }

    public decimal Amount { get; set; }
}
