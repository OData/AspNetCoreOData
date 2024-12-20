//-----------------------------------------------------------------------------
// <copyright file="DollarApplyController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply
{
    public class SalesController : ODataController
    {
        private readonly DollarApplyDbContext db;

        public SalesController(DollarApplyDbContext db)
        {
            this.db = db;
            SeedDatabase(this.db);
        }

        [EnableQuery]
        public ActionResult<IQueryable<Sale>> Get()
        {
            return db.Sales;
        }

        private void SeedDatabase(DollarApplyDbContext db)
        {
            db.Database.EnsureCreated();

            if (!db.Sales.Any())
            {
                var pg1Category = new Category { Id = "PG1", Name = "Food" };
                var pg2Category = new Category { Id = "PG2", Name = "Non-Food" };

                db.Categories.AddRange(new[]
                {
                    pg1Category,
                    pg2Category
                });

                var p1Product = new Product { Id = "P1", Name = "Sugar", Category = pg1Category, TaxRate = 0.06m };
                var p2Product = new Product { Id = "P2", Name = "Coffee", Category = pg1Category, TaxRate = 0.06m };
                var p3Product = new Product { Id = "P3", Name = "Paper", Category = pg2Category, TaxRate = 0.14m };
                var p4Product = new Product { Id = "P4", Name = "Pencil", Category = pg2Category, TaxRate = 0.14m };

                db.Products.AddRange(new[]
                {
                    p1Product,
                    p2Product,
                    p3Product,
                    p4Product
                });

                var c1Customer = new Customer { Id = "C1", Name = "Joe" };
                var c2Customer = new Customer { Id = "C2", Name = "Sue" };
                var c3Customer = new Customer { Id = "C3", Name = "Sue" };
                var c4Customer = new Customer { Id = "C4", Name = "Luc" };

                db.Customers.AddRange(new[]
                {
                    c1Customer,
                    c2Customer,
                    c3Customer,
                    c4Customer
                });

                db.Sales.AddRange(new[]
                {
                    new Sale { Id = 1, Year = 2022, Quarter = "2022-1", Customer = c1Customer, Product = p3Product, Amount = 1 },
                    new Sale { Id = 2, Year = 2022, Quarter = "2022-2", Customer = c1Customer, Product = p1Product, Amount = 2 },
                    new Sale { Id = 3, Year = 2022, Quarter = "2022-3", Customer = c1Customer, Product = p2Product, Amount = 4 },
                    new Sale { Id = 4, Year = 2022, Quarter = "2022-1", Customer = c2Customer, Product = p2Product, Amount = 8 },
                    new Sale { Id = 5, Year = 2022, Quarter = "2022-4", Customer = c2Customer, Product = p3Product, Amount = 4 },
                    new Sale { Id = 6, Year = 2022, Quarter = "2022-2", Customer = c3Customer, Product = p1Product, Amount = 2 },
                    new Sale { Id = 7, Year = 2022, Quarter = "2022-3", Customer = c3Customer, Product = p3Product, Amount = 1 },
                    new Sale { Id = 8, Year = 2022, Quarter = "2022-4", Customer = c3Customer, Product = p3Product, Amount = 2 },
                });

                db.SaveChanges();
            }
        }
    }

    public class  EmployeesController : ODataController
    {
        private static readonly List<Employee> employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                Name = "Nancy Davolio",
                Salary = 1300,
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
                Salary = 1500,
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
                Salary = 1100,
                DynamicProperties = new Dictionary<string, object>
                {
                    { "Commission", 370 },
                    { "Gender", "Female" }
                }
            },
        };

        [EnableQuery]
        public ActionResult<IEnumerable<Employee>> Get()
        {
            return employees;
        }
    }
}
