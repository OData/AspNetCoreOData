// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataRoutingSample.Models;

namespace ODataRoutingSample.Controllers
{
    public class ProductsController : ODataController/*ControllerBase*/
    {
        private MyDataContext _context;

        public ProductsController(MyDataContext context)
        {
            _context = context;

            if (_context.Products.Count() == 0)
            {
                IList<Product> products = new List<Product>
                {
                    new Product
                    {
                        Category = "Goods",
                        Color = Color.Red,
                        Detail = new ProductDetail { Id = "3", Info = "Zhang" },
                    },
                    new Product
                    {
                        Category = "Magazine",
                        Color = Color.Blue,
                        Detail = new ProductDetail { Id = "4", Info = "Jinchan" },
                    },
                    new Product
                    {
                        Category = "Fiction",
                        Color = Color.Green,
                        Detail = new ProductDetail { Id = "5", Info = "Hollewye" },
                    },
                };

                foreach (var product in products)
                {
                    _context.Products.Add(product);
                }

                _context.SaveChanges();
            }
        }

        [HttpGet]
       // [EnableQuery]
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

        public IActionResult Get(int key)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == key);
            if (product == null)
            {
                return NotFound($"Not found product with id = {key}");
            }

            return Ok(product);
        }

        [HttpPost]
       // [EnableQuery]
        public IActionResult Post([FromBody]Product product, CancellationToken token)
        {
            _context.Products.Add(product);
            return Created(product);
        }

        public IActionResult Put(int key, [FromBody]Product product)
        {
            return Ok();
        }

        public IActionResult Patch(int key, Delta<Product> product)
        {
            return Ok();
        }

        public IActionResult Delete(int key)
        {
            return Ok();
        }

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
