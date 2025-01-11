//-----------------------------------------------------------------------------
// <copyright file="DollarFilterController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
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

public class VendorsController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Vendor>> Get()
    {
        return DollarFilterDataSource.Vendors;
    }
}

public class BadVendorsController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Vendor>> Get()
    {
        return DollarFilterDataSource.BadVendors;
    }
}

public class CustomersController : ODataController
{
    [EnableQuery(MaxAnyAllExpressionDepth = 2)]
    public ActionResult<IEnumerable<Customer>> Get()
    {
        return DollarFilterDataSource.Customers;
    }
}

public class BadCustomersController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Customer>> Get()
    {
        return DollarFilterDataSource.BadCustomers;
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

public class BasketsController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Basket>> Get()
    {
        return DollarFilterDataSource.Baskets;
    }
}

public class BasicTypesController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<BasicType>> Get()
    {
        return DollarFilterDataSource.BasicTypes;
    }
}
