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
            EntitySetAggregationContext.EnsureDatabaseCreated(context);
            _context = context;
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
            EntitySetAggregationContext.EnsureDatabaseCreated(context);
            _context = context;
        }

        [EnableQuery]
        public IQueryable<Order> Get()
        {
            return _context.Orders;
        }
    }
}
