//-----------------------------------------------------------------------------
// <copyright file="DollarSearchController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarSearch
{
    public class ProductsController : ODataController
    {
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(DollarSearchDataSource.Products);
        }

        [EnableQuery]
        public IActionResult Get(int key)
        {
            SearchProduct c = DollarSearchDataSource.Products.FirstOrDefault(c => c.Id == key);
            if (c == null)
            {
                return NotFound($"Cannot find product with key = {key}");
            }

            return Ok(c);
        }
    }
}
