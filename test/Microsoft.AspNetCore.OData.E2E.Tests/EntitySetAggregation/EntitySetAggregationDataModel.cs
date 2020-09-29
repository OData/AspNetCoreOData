// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EntitySetAggregation
{
    public class EntitySetAggregationContext : DbContext
    {
        //public static string ConnectionString =
        //    @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EntitySetAggregationTest1";

        public EntitySetAggregationContext(DbContextOptions<EntitySetAggregationContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Address).WithOwner();
            modelBuilder.Entity<Order>().OwnsOne(c => c.SaleInfo).WithOwner();
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

        public SaleInfo SaleInfo { get; set; }
    }

    public class SaleInfo
    {
        public int Quantity { get; set; }

        public int UnitPrice { get; set; }
    }

    //[Owned, ComplexType]
    public class Address
    {
        public string Name { get; set; }

        public string Street { get; set; }
    }
}
