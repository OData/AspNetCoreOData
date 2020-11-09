// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
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
        [EnableQuery]
        public IActionResult Get(CancellationToken token)
        {
            return Ok(_context.Products);
        }

        [EnableQuery]
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

            _context.SaveChanges();

            return Created(product);
        }

        public IActionResult Put(int key, [FromBody]Delta<Product> product)
        {
            var original = _context.Products.FirstOrDefault(p => p.Id == key);
            if (original == null)
            {
                return NotFound($"Not found product with id = {key}");
            }

            product.Put(original);
            _context.SaveChanges();
            return Updated(original);
        }

        public IActionResult Patch(int key, Delta<Product> product)
        {
            var original = _context.Products.FirstOrDefault(p => p.Id == key);
            if (original == null)
            {
                return NotFound($"Not found product with id = {key}");
            }

            product.Patch(original);

            _context.SaveChanges();

            return Updated(original);
        }

        public IActionResult Delete(int key)
        {
            var original = _context.Products.FirstOrDefault(p => p.Id == key);
            if (original == null)
            {
                return NotFound($"Not found product with id = {key}");
            }

            _context.Products.Remove(original);
            _context.SaveChanges();
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

        [HttpGet]
        public string GetOptional()
        {
            return "GetOptional without parameter";
        }

        [HttpGet]
        public string GetOptional(string param)
        {
            return $"GetOptional without parameter value: param = {param}";
        }

        [HttpGet]
        [ODataRoute("CalculateSalary(minSalary={min},maxSalary={max})", prefix: "")]
        public string CalculateSalary(int min, int max)
        {
            return $"Unbound function call on CalculateSalary: min={min}, max={max}";
        }

        [HttpGet]
        [ODataRoute("CalculateSalary(minSalary={min},maxSalary={max},wholeName={name})", prefix: "")]
        public string CalculateSalary(int min, int max, string name)
        {
            return $"Unbound function call on CalculateSalary: min={min}, max={max}, name={name}";
        }
    }
}
