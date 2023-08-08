//-----------------------------------------------------------------------------
// <copyright file="IAsyncEnumerableDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IAsyncEnumerableTests
{
    public class IAsyncEnumerableContext : DbContext
    {
        public IAsyncEnumerableContext(DbContextOptions<IAsyncEnumerableContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Address).WithOwner();
        }
    }

    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public IList<Order> Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }
    }

    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }
}
