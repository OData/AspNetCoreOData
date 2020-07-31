// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new Product
            {
                Id = index,
                Category = "Category + " + index
            })
            .ToArray();
        }


        [HttpGet]
        public string GetWholeSalary(int minSalary, int maxSalary, int aveSalary)
        {
            return $"Products/GetWholeSalary: {minSalary}, {maxSalary}, {aveSalary}";
        }

        [HttpGet]
        public string GetWholeSalary(int minSalary, int maxSalary)
        {
            return $"Products/GetWholeSalary: {minSalary}, {maxSalary}";
        }

        [HttpGet]
        public string GetWholeSalary(int minSalary, string name)
        {
            return $"Products/GetWholeSalary: {minSalary}, {name}";
        }

        [HttpGet]
        public string GetWholeSalary(string order, string name)
        {
            return $"Products/GetWholeSalary: {order}, {name}";
        }
    }
}
