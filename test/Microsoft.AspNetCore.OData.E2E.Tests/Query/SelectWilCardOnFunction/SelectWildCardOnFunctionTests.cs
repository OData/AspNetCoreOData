//-----------------------------------------------------------------------------
// <copyright file="SelectWildCardOnFunctionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

﻿﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Query.SelectWilCardOnFunction
{
    public sealed class SelectWildCardOnFunctionTests : WebODataTestBase<SelectWildCardOnFunctionTests.Startup>
    {
        public class Startup : TestStartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.ConfigureControllers(typeof(CustomersController));

                IEdmModel model = GetEdmModel();
                services.AddControllers().AddOData(opt =>
                {
                    opt.Select();
                    opt.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                    opt.AddRouteComponents("odata", model);
                });
            }

            public static IEdmModel GetEdmModel()
            {
                ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
                builder.EntitySet<Customer>("Customers").EntityType
                    .Collection.Function("GetAllCustomers")
                    .ReturnsCollectionFromEntitySet<Customer>("Customers");

                return builder.GetEdmModel();
            }
        }

        public SelectWildCardOnFunctionTests(WebODataTestFixture<Startup> fixture)
            : base(fixture)
        {
        }

        /// <summary>
        /// For Select query with wildcard on Function
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SelectWildCardOnFunction_Success()
        {
            //Arrange
            string queryUrl = "odata/Customers/GetAllCustomers?$select=*";
            
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl))
            {
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

                //Act
                using (HttpResponseMessage response = await this.Client.SendAsync(request))
                {
                    // Assert
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();
                    foreach(Customer c in customers)
                    {
                        Assert.Equal("custId1", c.Id);
                        Assert.Equal("John", c.Name);
                        Assert.Equal("Active", c.Status);
                    }
                }
            }
        }
    }

    public class Customer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class CustomersController : ODataController
    {
        [HttpGet]
        public IEnumerable<Customer> GetAllCustomers()
        {
            return new List<Customer>()
            {
                new Customer
                {
                    Id = "custId1",
                    Name = "John",
                    Status = "Active"
                }
             };
        }
    }
}
