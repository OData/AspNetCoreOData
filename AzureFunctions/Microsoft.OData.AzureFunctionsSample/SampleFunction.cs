using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.OData.AzureFunctions;
using Microsoft.AspNetCore.OData.Query;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OData.AzureFunctionsSample
{
    public static class SampleFunction
    {
        [FunctionName("Customers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [OData(typeof(EdmModelProvider))] ODataQueryOptions<Customer> options)
        {
            var result = options.ApplyTo(customers.AsQueryable());
            return new OkObjectResult(result);
        }

        private static IList<Customer> customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "Ruto",
                Orders = new List<Order>()
                {
                    new Order { Id = 1, OrderName = "Order1" },
                    new Order { Id = 2, OrderName = "Order2" }
                }
            },
            new Customer
            {
                Id = 2,
                Name = "Ken",
                Orders = new List<Order>()
                {
                    new Order { Id =3, OrderName = "Order3" },
                    new Order { Id = 4, OrderName = "Order4" }
                }
            },
            new Customer 
            {
                Id = 3,
                Name = "Koko",
                Orders = new List<Order>()
                {
                    new Order { Id =4, OrderName = "Order4" },
                    new Order { Id = 5, OrderName = "Order5" }
                }
            },
            new Customer 
            {
                Id = 4,
                Name = "Smith",
                Orders = new List<Order>()
                {
                    new Order { Id = 6, OrderName = "Order6" },
                    new Order { Id = 7, OrderName = "Order7" }
                }
            }
        };
    }
}
