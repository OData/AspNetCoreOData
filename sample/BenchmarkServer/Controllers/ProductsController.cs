//-----------------------------------------------------------------------------
// <copyright file="ProductsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataPerformanceProfile.Models;

namespace ODataPerformanceProfile.Controllers;

public class ProductsController : ODataController
{
    private ProductsContext _context;
    private IList<Product> products;

    public ProductsController(ProductsContext context)
    {
         _context = context;
        products = new List<Product>();
            
        for (int i = 1; i < 3000; i++)
        {
            var prod = new Product()
            {
                Id = i,
                Category = "Goods" + i,
                Color = Color.Red,
                Others = new List<string> {"Others1", "Others2", "Others3"},
                CreatedDate = new DateTimeOffset(2001, 4, 15, 16, 24, 8, TimeSpan.FromHours(-8)),
                UpdatedDate = new DateTimeOffset(2011, 2, 15, 16, 24, 8, TimeSpan.FromHours(-8)),
                Detail = new ProductDetail { Id = "Id" + i, Info = "Info" + i },
                ProductOrders = new List<Order> {
                    new Order
                    {
                        Id = i,
                        OrderNo = "Order"+i
                    }
                },
                ProductSuppliers = new List<Supplier>
                {
                    new Supplier
                    {
                        Id = i,
                        Name = "Supplier"+i,
                        Description = "SupplierDesc"+i,
                        SupplierAddress = new Location
                        {
                            City = "SupCity"+i,
                            Address = "SupAddre"+i
                        }
                    }
                },
                Properties = new Dictionary<string, object>
                {
                    { "Prop1", new DateTimeOffset(2014, 7, 3, 0, 0, 0, 0, new TimeSpan(0))},
                    { "Prop2", new [] { "Leonard G. Lobel", "Eric D. Boyd" }},
                    { "Prop3", "Others"}
                }
            };

            products.Add(prod);
        }
    }

    [HttpGet]
    [EnableQuery(PageSize = 3000)]
    public IActionResult Get()
    {
        return Ok(products);
    }

    [HttpGet("odata/Products/mostRecent()")]
    public IActionResult MostRecent()
    {
        var maxProductId = products.Max(x => x.Id);
        return Ok(maxProductId);
    }

    [HttpPost("odata/Products({key})/Rate")]
    public IActionResult Rate([FromODataUri] string key, ODataActionParameters parameters)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        int rating = (int)parameters["rating"];

        if (rating < 0)
        {
            return BadRequest();
        }

        return Ok(new ProductRating() { Id = key, Rating = rating });
    }
}
