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
