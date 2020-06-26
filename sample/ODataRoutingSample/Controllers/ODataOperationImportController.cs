// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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

