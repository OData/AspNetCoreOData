//-----------------------------------------------------------------------------
// <copyright file="SingleResultDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SingleResultTest;

public class SingleResultContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=SingleResultTest2");
        base.OnConfiguring(optionsBuilder);
    }
}

public class Customer
{
    public int Id { get; set; }

    public string Name { get; set; }

    public List<Order> Orders { get; set; }
}

public class Order
{
    public int Id { get; set; }

    public string Title { get; set; }
}
