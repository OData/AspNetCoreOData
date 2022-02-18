//-----------------------------------------------------------------------------
// <copyright file="SingleResultController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.SingleResultTest
{
    public class CustomersController : ODataController
    {
        private readonly SingleResultContext _db = new SingleResultContext();

        [EnableQuery]
        public SingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new SingleResultContext();
            return SingleResult.Create<Customer>(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Name = $"name_{i}",
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Title = $"title_{i}",
                        }
                    }
                };

                _db.Customers.Add(customer);
            }

            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            _db.Database.EnsureCreated();
            if (!_db.Customers.Any())
            {
                Generate();
            }
        }
    }
}
