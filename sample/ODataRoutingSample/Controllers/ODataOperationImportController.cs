//-----------------------------------------------------------------------------
// <copyright file="ODataOperationImportController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class ODataOperationImportController : ControllerBase
    {
        [HttpPost]
        public IEnumerable<Product> ResetData()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Product
            {
                Id = index,
                Category = "Category + " + index
            })
            .ToArray();
        }
    }
}
