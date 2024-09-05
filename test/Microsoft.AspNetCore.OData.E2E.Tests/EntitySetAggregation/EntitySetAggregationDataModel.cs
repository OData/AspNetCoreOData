//-----------------------------------------------------------------------------
// <copyright file="EntitySetAggregationDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EntitySetAggregation;

public class EntitySetAggregationContext : DbContext
{
    //public static string ConnectionString =
    //    @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=EntitySetAggregationTest1";

    public EntitySetAggregationContext(DbContextOptions<EntitySetAggregationContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().OwnsOne(c => c.Address).WithOwner();
        modelBuilder.Entity<Order>().OwnsOne(c => c.SaleInfo).WithOwner();
    }

    public static void EnsureDatabaseCreated(EntitySetAggregationContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Customers.Any())
        {
            for (int i = 1; i <= 3; i++)
            {
                var customer = new Customer
                {
                    // Id = i,
                    Name = "Customer" + (i + 1) % 2,
                    Orders =
                        new List<Order> {
                        new Order {
                            Name = "Order" + 2*i,
                            Price = i * 25,
                            SaleInfo = new SaleInfo { Quantity = i, UnitPrice = 25 }
                        },
                        new Order {
                            Name = "Order" + 2*i+1,
                            Price = i * 75,
                            SaleInfo = new SaleInfo { Quantity = i, UnitPrice = 75 }
                        }
                        },
                    Address = new Address
                    {
                        Name = "City" + i % 2,
                        Street = "Street" + i % 2,
                    }
                };

                context.Customers.Add(customer);
            }

            context.SaveChanges();
        }
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

public class Employee
{
    public int Id { get; set; }
    public NextOfKin NextOfKin { get; set; }
}

public class NextOfKin
{
    public string Name { get; set; }
    public Location PhysicalAddress { get; set; }
}

public class Location
{
    public string City { get; set; }
}
