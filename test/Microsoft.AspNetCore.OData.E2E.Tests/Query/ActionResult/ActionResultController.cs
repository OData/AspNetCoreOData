//-----------------------------------------------------------------------------
// <copyright file="ActionResultController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.ActionResult
{
    public class CustomersController : ControllerBase
    {
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Expand | AllowedQueryOptions.Filter)]
        public async Task<ActionResult<IEnumerable<Customer>>> Get()
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
