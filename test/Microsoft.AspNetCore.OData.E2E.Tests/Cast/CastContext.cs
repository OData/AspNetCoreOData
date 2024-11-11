//-----------------------------------------------------------------------------
// <copyright file="CastContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Cast;

public class DataSource
{
    private static IQueryable<Product> _products = null;

    public static IQueryable<Product> InMemoryProducts
    {
        get
        {
            if (_products == null)
            {
                var addresses = new List<MyAddress>
                {
                    new MyAddress { ID = 100, City = "City1" },
                    new MyOtherAddress { ID = 200, City = "City2", Street = "Street2" },
                    new MyAddress { ID = 300, City = "City3" },
                    new MyOtherAddress { ID = 400, City = "City4", Street = "Street4" },
                };

                _products = new List<Product>()
                {
                    new Product()
                    {
                        ID=1,
                        Name="Name1",
                        Domain=Domain.Military,
                        Weight=1.1,
                        DimensionInCentimeter=new List<int>{1,2,3},
                        ManufacturingDate=new DateTimeOffset(2011,1,1,0,0,0,TimeSpan.FromHours(8)),
                        Location = addresses.First(a => a.ID == 100)
                    },
                    new Product()
                    {
                        ID=2,
                        Name="Name2",
                        Domain=Domain.Civil,
                        Weight=2.2,
                        DimensionInCentimeter=new List<int>{2,3,4},
                        ManufacturingDate=new DateTimeOffset(2012,1,1,0,0,0,TimeSpan.FromHours(8)),
                        Location = addresses.First(a => a.ID == 200)
                    },
                    new Product()
                    {
                        ID=3,
                        Name="Name3",
                        Domain=Domain.Both,
                        Weight=3.3,
                        DimensionInCentimeter=new List<int>{3,4,5},
                        ManufacturingDate=new DateTimeOffset(2013,1,1,0,0,0,TimeSpan.FromHours(8)),
                        Location = addresses.First(a => a.ID == 300)
                    },
                    new AirPlane()
                    {
                        ID=4,
                        Name="Name4",
                        Domain=Domain.Both,
                        Weight=4.4,
                        DimensionInCentimeter=new List<int>{4,5,6},
                        ManufacturingDate=new DateTimeOffset(2013,1,1,0,0,0,TimeSpan.FromHours(8)),
                        Speed=100,
                        Location =  addresses.First(a => a.ID == 200)
                    },
                    new JetPlane()
                    {
                        ID=5,
                        Name="Name5",
                        Domain=Domain.Military,
                        Weight=5.5,
                        DimensionInCentimeter=new List<int>{6,7,8},
                        ManufacturingDate=new DateTimeOffset(2013,1,1,0,0,0,TimeSpan.FromHours(8)),
                        Speed=100,
                        Company="Boeing",
                        Location = addresses.First(a => a.ID == 400)
                    },
                    new JetPlane()
                    {
                        ID=6,
                        Name="Name6",
                        Domain=Domain.Civil,
                        Weight=6.6,
                        DimensionInCentimeter=new List<int>{7,8,9},
                        ManufacturingDate=new DateTimeOffset(2013,1,1,0,0,0,TimeSpan.FromHours(8)),
                        Speed=500,
                        Company="AirBus",
                        Location =  addresses.First(a => a.ID == 400)
                    },

               }.AsQueryable<Product>();
            }
            return _products;
        }
    }

    //public static IQueryable<Product> EfProducts
    //{
    //    get
    //    {
    //        if (_context == null)
    //        {
    //            Database.SetInitializer(new DropCreateDatabaseAlways<ProductsContext>());
    //            string databaseName = "CastTest_" + DateTime.Now.Ticks.ToString();
    //            string connectionString = string.Format(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog={0}", databaseName);

    //            _context = new ProductsContext();
    //            foreach (Product product in DataSource.InMemoryProducts)
    //            {
    //                _context.Products.Add(product);
    //            }

    //            _context.SaveChanges();
    //        }

    //        return _context.Products;
    //    }
    //}
}

public class ProductsContext : DbContext
{
    public ProductsContext(DbContextOptions<ProductsContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var splitStringConverter1 = new ValueConverter<IEnumerable<string>, string>(v => string.Join(";", v), v => v.Split(new[] { ';' }));

        Func<string, IList<int>> convertFrom = a =>
        {
            var items = a.Split(new[] { ';' });
            IList<int> values = new List<int>();
            foreach (var item in items)
            {
                values.Add(Int32.Parse(item));
            }

            return values;
        };

        var splitStringConverter = new ValueConverter<IList<int>, string>(v => string.Join(";", v), FuncToExpression(convertFrom));
        modelBuilder.Entity<Product>().Property(nameof(Product.DimensionInCentimeter)).HasConversion(splitStringConverter);
        modelBuilder.Entity<MyAddress>()
            .HasDiscriminator<string>("address_type")
            .HasValue<MyAddress>("MyAddress")
            .HasValue<MyOtherAddress>("MyOtherAddress");

        modelBuilder.Entity<AirPlane>().HasBaseType<Product>();
        modelBuilder.Entity<JetPlane>().HasBaseType<AirPlane>();
    }

    private static Expression<Func<T, IList<int>>> FuncToExpression<T>(Func<T, IList<int>> f)
    {
        return x => f(x);
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    string databaseName = "CastTest_" + DateTime.Now.Ticks.ToString();
    //    string connectionString = string.Format(@"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog={0}", databaseName);

    //    optionsBuilder.UseInMemoryDatabase("Data Source=blogging.db");
    //}
}
