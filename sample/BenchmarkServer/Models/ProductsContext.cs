//-----------------------------------------------------------------------------
// <copyright file="ProductsContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;

namespace ODataPerformanceProfile.Models;

public class ProductsContext : DbContext
{
    public ProductsContext(DbContextOptions<ProductsContext> options)
       : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Order> Orders { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasMany(a => a.ProductSuppliers);
        modelBuilder.Entity<Product>().HasMany(a => a.ProductOrders);
        modelBuilder.Entity<Supplier>().OwnsOne(c => c.SupplierAddress).WithOwner();
    }
}
