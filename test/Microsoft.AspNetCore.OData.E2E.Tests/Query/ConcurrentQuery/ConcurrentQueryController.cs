//-----------------------------------------------------------------------------
// <copyright file="ConcurrentQueryController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.ConcurrentQuery
{
    public class CustomersController : ControllerBase
    {
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Count | AllowedQueryOptions.Filter)]
        public IQueryable<Customer> GetCustomers()
        {
            return Enumerable.Range(1, 100)
                .Select(i => new Customer
                {
                    Id = i,
                }).AsQueryable();
        }
    }
}
