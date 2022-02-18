//-----------------------------------------------------------------------------
// <copyright file="QueryValidationBeforeActionController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.QueryValidationBeforeAction
{
    public class CustomersController : ControllerBase
    {
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Top, MaxTop = 10)]
        public IEnumerable<Customer> GetCustomers()
        {
            throw new Exception("Controller should never be invoked as query validation should fail");
        }
    }
}
