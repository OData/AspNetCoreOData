//-----------------------------------------------------------------------------
// <copyright file="AutoExpandController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.AutoExpand
{
    public class CustomersController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(AutoExpandDataSource.Customers);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            Customer c = AutoExpandDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find customer with key = {key}");
            }

            return Ok(c);
        }

        [EnableQuery]
        public IActionResult GetHomeAddress(int key)
        {
            Customer c = AutoExpandDataSource.Customers.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find customer with key = {key}");
            }

            return Ok(c.HomeAddress);
        }
    }

    public class PeopleController : ODataController
    {
        [EnableQuery(MaxExpansionDepth = 4)]
        public IQueryable<People> Get()
        {
            return AutoExpandDataSource.People.AsQueryable();
        }
    }

    public class NormalOrdersController : ODataController
    {
        [EnableQuery]
        public IQueryable<NormalOrder> Get()
        {
            return AutoExpandDataSource.NormalOrders.AsQueryable();
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            NormalOrder n = AutoExpandDataSource.NormalOrders.FirstOrDefault(c => c.Id == key);
            if (n == null)
            {
                return NotFound($"Cannot find NormalOrder with key = {key}");
            }

            return Ok(n);
        }
    }
}
