//-----------------------------------------------------------------------------
// <copyright file="TypelessDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Typeless;

// Types included for comparison with typeless scenario
public class ChangeSet
{
    public int Id { get; set; }
    public object Changed { get; set; }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal CreditLimit { get; set; }
    public List<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset OrderDate { get; set; }
}
