// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
            _context = context;
            _context.Database.EnsureCreated();

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
                    Id = i,
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
}
