// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        [EnableQuery]
        public IEnumerable<Product> Get(CancellationToken token)
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Product
            {
                Id = index,
                Category = "Category + " + index
            })
            .ToArray();
        }

        //[HttpPost]

        //[EnableQuery]
        //public IEnumerable<Product> Post(CancellationToken token)
        //{
        //    var rng = new Random();
        //    return Enumerable.Range(1, 5).Select(index => new Product
        //    {
        //        Id = index,
        //        Category = "Category + " + index
        //    })
        //    .ToArray();
        //}


        [HttpGet]
        // ~/....(minSalary=4, maxSalary=5, aveSalary=9)
        public string GetWholeSalary(int minSalary, int maxSalary, string aveSalary/*, CancellationToken token, ODataQueryOptions queryOptions, ODataPath path*/)
        {
            return $"Products/GetWholeSalary: {minSalary}, {maxSalary}, {aveSalary}";
        }

        [HttpGet]
        // ~/....(minSalary=4, maxSalary=5)
        public string GetWholeSalary(int minSalary, int maxSalary)
        {
            // return $"Products/GetWholeSalary: {minSalary}, {maxSalary}";
            return GetWholeSalary(minSalary, maxSalary, aveSalary: "9");
        }

        [HttpGet]
        [ActionName("GetWholeSalary")] //
        // ~/....(minSalary=4, maxSalary=5)
        public string GetWholeSalary(int minSalary, string aveSalary)
        {
            int maxSalary = 10;
            // return $"Products/GetWholeSalary: {minSalary}, {maxSalary}";
            return GetWholeSalary(minSalary, maxSalary, aveSalary);
        }

        [HttpGet]
        // ~/....(minSalary=4)
        // ~/....(minSalary=4, name='abc')
        public string GetWholeSalary(int minSalary, /*[FromBody]*/double name)
        {
            return $"Products/GetWholeSalary: {minSalary}, {name}";
        }

        [HttpGet]
        // GetWholeSalary(order={order}, name={name})
        // http://localhost:5000/Products/Default.GetWholeSalary(order='2',name='abc')
        public string GetWholeSalary(string order, string name)
        {
            return $"Products/GetWholeSalary: {order}, {name}";
        }
    }
}
