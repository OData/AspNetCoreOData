using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using ODataPerformanceProfile.Models;

namespace ODataPerformanceProfile.Controllers
{
    public class ProductsController : ODataController
    {
        private ProductsContext _context;
        private IList<Product> products;

        public ProductsController(ProductsContext context)
        {
            /* _context = context;

             if (_context.Products.Count() == 0)
             {


                 foreach (var product in products)
                 {
                     _context.Products.Add(product);
                 }

                 _context.SaveChanges();
             }*/
            products = new List<Product>();
            for (int i = 1; i < 3000; i++)
            {
                var prod = new Product()
                {
                    Id = i,
                    Category = "Goods" + i,
                    Color = Color.Red,
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
                    }
                };

                products.Add(prod);
            }
        }

        [HttpGet]
        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(products);
        }
    }
}
