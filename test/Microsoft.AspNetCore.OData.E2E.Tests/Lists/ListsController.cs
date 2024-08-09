//-----------------------------------------------------------------------------
// <copyright file="ListsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Lists
{
    [Route("convention")]
    public class ProductsController : ODataController
    {
        private readonly ListsContext _dbContext;
        public ProductsController(ListsContext context)
        {
            _dbContext = context;
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IActionResult Get()
        {
            return Ok(_dbContext.Set<Product>());
        }

        public IActionResult Get(string key)
        {
            return Ok(_dbContext.Set<Product>().Find(key));
        }

        public IActionResult Post([FromBody] Product Product)
        {
            Product.ProductId = _dbContext.Set<Product>().Count() + 1+"";
            _dbContext.Set<Product>().Add(Product);
            _dbContext.SaveChanges();

            return Created(Product);
        }

        public IActionResult Put(int key, [FromBody] Product Product)
        {
            Product.ProductId = key+"";
            Product originalProduct = _dbContext.Set<Product>().Find(key);

            if (originalProduct == null)
            {
                _dbContext.Set<Product>().Add(Product);

                return Created(Product);
            }

            _dbContext.Set<Product>().Remove(originalProduct);
            _dbContext.Set<Product>().Add(Product);
            return Ok(Product);
        }

        public IActionResult Patch(int key, [FromBody] Delta<Product> delta)
        {
            Product originalProduct = _dbContext.Set<Product>().Find(key);

            if (originalProduct == null)
            {
                Product temp = new Product();
                delta.Patch(temp);
                _dbContext.Set<Product>().Add(temp);
                return Created(temp);
            }

            delta.Patch(originalProduct);
            return Ok(delta);
        }

        public IActionResult Delete(int key)
        {
            Product Product = _dbContext.Set<Product>().Find(key);

            _dbContext.Set<Product>().Remove(Product);
            return this.StatusCode(StatusCodes.Status204NoContent);
        }

        [HttpPost("ResetDataSource")]
        public IActionResult ResetDataSource()
        {
            _dbContext.Set<Product>().RemoveRange(_dbContext.Set<Product>());
            _dbContext.SaveChanges();

            // Add new seed data
            _dbContext.Set<Product>().AddRange(
               new Product
               {
                   ProductId = "1",
                   Name = "Product1",
                   Category = "Category1",
                   ListTestString = new List<string> { "Test1", "Test2", "Test99" },
                   ListTestBool = new List<bool> { true, false },
                   ListTestInt = new List<int> { 1, 2, 3 },
                   ListTestDouble = new List<double> { 1.1, 2.2 },
                   ListTestFloat = new List<float> { 1.1f, 2.2f },
                   ListTestDateTime = new List<DateTimeOffset> { DateTime.Now, DateTime.UtcNow },
                   ListTestUri = new List<Uri> { new Uri("https://example.com") },
                   ListTestUint = new uint[] { 1, 2, 3 },
                   ListTestOrders = new List<Order>
                    {
                        new Order { OrderId = "Order1" },
                        new Order { OrderId = "Order2" }
                    }
               },
                new Product
                {
                    ProductId = "2",
                    Name = "Product2",
                    Category = "Category2",
                    ListTestString = new List<string> { "Test3", "Test4", "Test99" },
                    ListTestBool = new List<bool> { false, true },
                    ListTestInt = new List<int> { 4, 5, 6 },
                    ListTestDouble = new List<double> { 3.3, 4.4 },
                    ListTestFloat = new List<float> { 3.3f, 4.4f },
                    ListTestDateTime = new List<DateTimeOffset> { DateTime.Now.AddDays(1), DateTime.UtcNow.AddDays(1) },
                    ListTestUri = new List<Uri> { new Uri("https://example.org") },
                    ListTestUint = new uint[] { 4, 5, 6 }
                },
                new Product
                {
                    ProductId = "3",
                    Name = "Product3",
                    Category = "Category3",
                    ListTestString = new List<string> { "Test5", "Test6" },
                    ListTestBool = new List<bool> { true, true },
                    ListTestInt = new List<int> { 7, 8, 9 },
                    ListTestDouble = new List<double> { 5.5, 6.6 },
                    ListTestFloat = new List<float> { 5.5f, 6.6f },
                    ListTestDateTime = new List<DateTimeOffset> { DateTime.Now.AddDays(2), DateTime.UtcNow.AddDays(2) },
                    ListTestUri = new List<Uri> { new Uri("https://example.net") },
                    ListTestUint = new uint[] { 7, 8, 9 }
                },
                new Product
                {
                    ProductId = "4",
                    Name = "Product4",
                    Category = "Category4",
                    ListTestString = new List<string> { "Test98", "Test98" },
                    ListTestBool = new List<bool> { false, false },
                    ListTestInt = new List<int> { 10, 11, 12 },
                    ListTestDouble = new List<double> { 7.7, 8.8 },
                    ListTestFloat = new List<float> { 7.7f, 8.8f },
                    ListTestDateTime = new List<DateTimeOffset> { DateTime.Now.AddDays(3), DateTime.UtcNow.AddDays(3) },
                    ListTestUri = new List<Uri> { new Uri("https://example.edu") },
                    ListTestUint = new uint[] { 10, 11, 12 }
                },
                new Product
                {
                    ProductId = "5",
                    Name = "Product5",
                    Category = "Category5",
                    ListTestString = new List<string> { "Test98", "Test98" },
                    ListTestBool = new List<bool> { true, false },
                    ListTestInt = new List<int> { 13, 14, 15 },
                    ListTestDouble = new List<double> { 9.9, 10.10 },
                    ListTestFloat = new List<float> { 9.9f, 10.10f },
                    ListTestDateTime = new List<DateTimeOffset> { DateTime.Now.AddDays(4), DateTime.UtcNow.AddDays(4) },
                    ListTestUri = new List<Uri> { new Uri("https://example.gov") },
                    ListTestUint = new uint[] { 13, 14, 15 }
                }
            );
            _dbContext.SaveChanges();
            return this.StatusCode(StatusCodes.Status204NoContent);
        }


    }

}
