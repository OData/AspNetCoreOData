//-----------------------------------------------------------------------------
// <copyright file="EntitySetAggregationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.EntitySetAggregation
{
    public class CustomersController : ODataController
    {
        private readonly EntitySetAggregationContext _context;

        public CustomersController(EntitySetAggregationContext context)
        {
            context.Database.EnsureCreated();
            _context = context;

            if (!_context.Customers.Any())
            {
                Generate();
            }
        }

        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            return _context.Customers;
        }

        [EnableQuery]
        public SingleResult<Customer> Get(int key)
        {
            return SingleResult.Create(_context.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i <= 3; i++)
            {
                var customer = new Customer
                {
                   // Id = i,
                    Name = "Customer" + (i+1) % 2,
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

                _context.Customers.Add(customer);
            }

            _context.SaveChanges();
        }
    }

    public class EmployeesController : ODataController
    {
        private static readonly List<Employee> employees = new List<Employee>
        {
            new Employee
            {
                Id = 1,
                NextOfKin = new NextOfKin { Name = "NoK 1", PhysicalAddress = new Location { City = "Redmond" } }
            },
            new Employee
            {
                Id = 2,
                NextOfKin = new NextOfKin { Name = "NoK 2", PhysicalAddress = new Location { City = "Nairobi" } }
            },
            new Employee
            {
                Id = 3,
                NextOfKin = new NextOfKin { Name = "NoK 3", PhysicalAddress = new Location { City = "Redmond" } }
            }
        };

        [EnableQuery]
        public IQueryable<Employee> Get()
        {
            return employees.AsQueryable();
        }
    }

    public class OrdersController : ODataController
    {
        private readonly EntitySetAggregationContext _context;

        public OrdersController(EntitySetAggregationContext context)
        {
            context.Database.EnsureCreated();
            _context = context;

            if (!_context.Orders.Any())
            {
                Generate();
            }
        }

        [EnableQuery]
        public IQueryable<Order> Get()
        {
            return _context.Orders;
        }

        [EnableQuery]
        public SingleResult<Order> Get(int key)
        {
            return SingleResult.Create(_context.Orders.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i <= 3; i++)
            {
                var order = new Order
                {
                    Name = "Order" + 2 * i,
                    Price = i * 25,
                    SaleInfo = new SaleInfo { Quantity = i, UnitPrice = 25 }
                };

                _context.Orders.Add(order);
            }

            _context.SaveChanges();
        }
    }
}
