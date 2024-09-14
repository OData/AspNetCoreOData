//-----------------------------------------------------------------------------
// <copyright file="ConcurrentQueryODataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ConcurrentQuery;

public class Customer
{
    public int Id { get; set; }
    [Contained]
    public IEnumerable<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }
}
