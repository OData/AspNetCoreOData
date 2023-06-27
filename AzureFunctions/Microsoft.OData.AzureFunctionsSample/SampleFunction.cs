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
using ODataQueryBuilder.Query;
using System.Collections.Generic;
using System.Linq;
using ODataQueryBuilder.Abstracts;
using Microsoft.OData.ModelBuilder;
using System.Net.Http;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;
using ODataQueryBuilder.Query.Expressions;
using ODataQueryBuilder.Query.Validator;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.OData.AzureFunctionsSample
{
    public static class SampleFunction
    {
        [FunctionName("Customers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            [OData(typeof(EdmModelProvider))] ODataQueryOptionsFundamentals<Customer> options)
        {
            (IQueryable result, bool pageSizeLimited) = options.ApplyTo(customers.AsQueryable(), new ODataQuerySettings());

            return new OkObjectResult(result);
        }

        #region Testing
        private static IODataFeature GetODataFeature(HttpRequest req)
        {
            IODataFeature odataFeature = req.HttpContext.Features.Get<IODataFeature>();
            if (odataFeature == null)
            {
                odataFeature = new ODataFeature();
                req.HttpContext.Features.Set(odataFeature);
            }

            return odataFeature;
        }
        #endregion

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
