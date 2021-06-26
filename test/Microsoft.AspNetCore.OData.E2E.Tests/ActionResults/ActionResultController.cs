// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ActionResults
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : Controller
    {
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Expand)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await Task.FromResult(new List<Customer>
            { 
                new Customer
                {
                    Id = "CustId",
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Id = "BookId",
                        },
                    },
                },
            });
        }
    }
}
