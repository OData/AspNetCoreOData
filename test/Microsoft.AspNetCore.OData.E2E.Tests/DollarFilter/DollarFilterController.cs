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

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarFilter
{
    public class PeopleController : ODataController
    {
        [EnableQuery]
        public ActionResult<IEnumerable<Person>> Get()
        {
            return Ok(DollarFilterDataSource.People);
        }
    }
}
