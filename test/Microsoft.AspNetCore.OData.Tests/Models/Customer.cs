//-----------------------------------------------------------------------------
// <copyright file="Customer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Tests.Models;

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string City { get; set; }

    public Address Address { get; set; }

    public Address WorkAddress { get; set; }

    public string Website { get; set; }

    public string ShareSymbol { get; set; }

    public decimal? SharePrice { get; set; }

    public Company Company { get; set; }

    public List<Order> Orders { get; set; }

    public List<string> Aliases { get; set; }

    public List<Address> Addresses { get; set; }

    public Dictionary<string, object> DynamicProperties { get; set; }

    public DateTimeOffset? StartDate { get; set; }
}
