// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataOrderByTest
{
    public class OrderByContext : DbContext
    {
        public OrderByContext(DbContextOptions<OrderByContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }

        public DbSet<Item2> Items2 { get; set; }

        public DbSet<ItemWithEnum> ItemsWithEnum { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>()
                .HasKey(c => new { c.A, c.B, c.C });

            modelBuilder.Entity<Item2>()
                .HasKey(c => new { c.A, c.B, c.C });

            modelBuilder.Entity<ItemWithEnum>()
                .HasKey(c => new { c.A, c.B, c.C });
        }
    }
}
