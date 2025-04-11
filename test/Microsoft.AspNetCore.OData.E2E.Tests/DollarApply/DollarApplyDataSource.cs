//-----------------------------------------------------------------------------
// <copyright file="DollarApplyDataSource.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    public class DollarApplyDbContext : DbContext
    {
        public DollarApplyDbContext(DbContextOptions<DollarApplyDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Sale> Sales { get; set; }
    }

    public class DollarApplySqlDbContext : DbContext
    {
        public DollarApplySqlDbContext(DbContextOptions<DollarApplySqlDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Sale>(d => d.Property(p => p.Id).ValueGeneratedNever());
        }

        public virtual DbSet<Category> Categories { get; set; }

        public virtual DbSet<Product> Products { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<Sale> Sales { get; set; }
    }

    public class DataSource
    {
        private static readonly Company company;
        private static readonly List<Employee> employees;

        static DataSource()
        {
            company = new Company
            {
                Name = "Northwind Traders"
            };

            employees = new List<Employee>
            {
                new Employee
                {
                    Id = 1,
                    Name = "Nancy Davolio",
                    BaseSalary = 1300,
                    Address = new Address
                    {
                        City = "Seattle",
                        State = "WA"
                    },
                    Company = company,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Commission", 250 },
                        { "Gender", "Female" }
                    }
                },
                new Employee
                {
                    Id = 2,
                    Name = "Andrew Fuller",
                    BaseSalary = 1500,
                    Address = new Address
                    {
                        City = "Tacoma",
                        State = "WA"
                    },
                    Company = company,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Commission", 190 },
                        { "Gender", "Male" }
                    }
                },
                new Employee
                {
                    Id = 3,
                    Name = "Janet Leverling",
                    BaseSalary = 1100,
                    Address = new Address
                    {
                        City = "Kirkland",
                        State = "WA"
                    },
                    Company = company,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Commission", 370 },
                        { "Gender", "Female" }
                    }
                },
                new Employee
                {
                    Id = 9,
                    Name = "Anne Dodsworth",
                    BaseSalary = 1000,
                    Address = new Address
                    {
                        City = "London",
                        State = "UK"
                    },
                    Company = company,
                    DynamicProperties = new Dictionary<string, object>
                    {
                        { "Commission", 310 },
                        { "Gender", "Female" }
                    }
                },
            };

            company.VP = employees.First(e => e.Id == 2);
        }

        public static List<Employee> Employees => employees;
    }

    internal static class DollarApplyDbContextInitializer
    {
        public static void SeedDatabase(DollarApplyDbContext db)
        {
            db.Database.EnsureCreated();

            if (!db.Sales.Any())
            {
                var (categories, products, customers, sales) = Generate();
                
                db.Categories.AddRange(categories);
                db.Products.AddRange(products);
                db.Customers.AddRange(customers);
                db.Sales.AddRange(sales);

                db.SaveChanges();
            }
        }

        public static void SeedDatabase(DollarApplySqlDbContext db)
        {
            db.Database.EnsureCreated();

            if (!db.Sales.Any())
            {
                var (categories, products, customers, sales) = Generate();

                db.Categories.AddRange(categories);
                db.Products.AddRange(products);
                db.Customers.AddRange(customers);
                db.Sales.AddRange(sales);

                db.SaveChanges();
            }
        }

        private static (Category[] Categories, Product[] Products, Customer[] Customers, Sale[] Sales) Generate()
        {
            var pg1Category = new Category { Id = "PG1", Name = "Food" };
            var pg2Category = new Category { Id = "PG2", Name = "Non-Food" };

            Category[] categories = new []
            {
                pg1Category,
                pg2Category
            };

            var p1Product = new Product { Id = "P1", Name = "Sugar", Category = pg1Category, TaxRate = 0.06m };
            var p2Product = new Product { Id = "P2", Name = "Coffee", Category = pg1Category, TaxRate = 0.06m };
            var p3Product = new Product { Id = "P3", Name = "Paper", Category = pg2Category, TaxRate = 0.14m };
            var p4Product = new Product { Id = "P4", Name = "Pencil", Category = pg2Category, TaxRate = 0.14m };

            Product[] products = new []
            {
                p1Product,
                p2Product,
                p3Product,
                p4Product
            };

            var c1Customer = new Customer { Id = "C1", Name = "Joe" };
            var c2Customer = new Customer { Id = "C2", Name = "Sue" };
            var c3Customer = new Customer { Id = "C3", Name = "Sue" };
            var c4Customer = new Customer { Id = "C4", Name = "Luc" };

            Customer[] customers = new []
            {
                c1Customer,
                c2Customer,
                c3Customer,
                c4Customer
            };

            Sale[] sales = new [] 
            {
                new Sale { Id = 1, Year = 2022, Quarter = "2022-1", Customer = c1Customer, Product = p3Product, Amount = 1 },
                new Sale { Id = 2, Year = 2022, Quarter = "2022-2", Customer = c1Customer, Product = p1Product, Amount = 2 },
                new Sale { Id = 3, Year = 2022, Quarter = "2022-3", Customer = c1Customer, Product = p2Product, Amount = 4 },
                new Sale { Id = 4, Year = 2022, Quarter = "2022-1", Customer = c2Customer, Product = p2Product, Amount = 8 },
                new Sale { Id = 5, Year = 2022, Quarter = "2022-4", Customer = c2Customer, Product = p3Product, Amount = 4 },
                new Sale { Id = 6, Year = 2022, Quarter = "2022-2", Customer = c3Customer, Product = p1Product, Amount = 2 },
                new Sale { Id = 7, Year = 2022, Quarter = "2022-3", Customer = c3Customer, Product = p3Product, Amount = 1 },
                new Sale { Id = 8, Year = 2022, Quarter = "2022-4", Customer = c3Customer, Product = p3Product, Amount = 2 },
            };

            return (categories, products, customers, sales);
        }
    }
}
