//-----------------------------------------------------------------------------
// <copyright file="ConcurrentQueryController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ConcurrentQuery
{
    public class CustomersController : Controller
    {
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Count | AllowedQueryOptions.Filter | AllowedQueryOptions.Expand)]
        public IQueryable<Customer> GetCustomers()
        {
            return Enumerable.Range(1, 100)
                .Select(i => new Customer
                {
                    Id = i,
                    Orders = Enumerable.Range(1, 5)
                    .Select(x => new Order
                    {
                        Id = x
                    })
                }).AsQueryable();
        }
    }
}
