//-----------------------------------------------------------------------------
// <copyright file="DollarFilterController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter;

public class PeopleController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Person>> Get()
    {
        return Ok(DollarFilterDataSource.People);
    }
}

public class ProductsController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Product>> Get()
    {
        return DollarFilterDataSource.Products;
    }
}

public class CustomersController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Customer>> Get()
    {
        return DollarFilterDataSource.Customers;
    }

    [EnableQuery]
    public ActionResult<IEnumerable<Address>> GetAddresses(int key)
    {
        var customer = DollarFilterDataSource.Customers.FirstOrDefault(d => d.Id == key);

        if (customer == null)
        {
            return NotFound();
        }

        return customer.Addresses;
    }
}
