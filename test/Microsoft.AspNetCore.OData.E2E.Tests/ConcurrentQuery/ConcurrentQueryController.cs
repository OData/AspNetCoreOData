// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ConcurrentQuery
{
    public class CustomersController : Controller
    {
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
