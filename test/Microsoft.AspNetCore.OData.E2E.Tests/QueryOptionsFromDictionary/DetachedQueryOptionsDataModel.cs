//-----------------------------------------------------------------------------
// <copyright file="DetachedQueryOptionsDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.E2E.Tests.QueryOptionsFromDictionary;

public class DetachedCustomer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int Age { get; set; }
}

public static class DetachedQueryOptionsDataSource
{
    public static IList<DetachedCustomer> Customers { get; } = new List<DetachedCustomer>
    {
        new DetachedCustomer { Id = 1, Name = "Alice", Age = 30 },
        new DetachedCustomer { Id = 2, Name = "Bob", Age = 25 },
        new DetachedCustomer { Id = 3, Name = "Charlie", Age = 40 },
        new DetachedCustomer { Id = 4, Name = "Dave", Age = 35 },
        new DetachedCustomer { Id = 5, Name = "Eve", Age = 28 }
    };
}
