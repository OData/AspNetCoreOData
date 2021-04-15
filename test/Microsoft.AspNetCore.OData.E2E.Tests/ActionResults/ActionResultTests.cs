// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ActionResults
{
    public class ActionResultTests : WebApiTestBase<ActionResultTests>
    {
        public ActionResultTests(WebApiTestFixture<ActionResultTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(CustomersController), typeof(ODataEndpointController));

            services.AddOData(opt => opt.Count().Filter().OrderBy().Expand().SetMaxTop(null)
                .AddModel("actionresult", ActionResultEdmModel.GetEdmModel()));
        }

        [Fact]
        public async Task TestRoutes()
        {
            // Arrange
            string requestUri = "$odata";
            HttpClient client = CreateClient();

            // Act
            var response = await client.GetAsync(requestUri);

            // Assert
            response.EnsureSuccessStatusCode();
            string payload = await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// For Non-OData json based paths. EnableQuery should work.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ActionResultNonODataPathReturnsExpansion()
        {
            // Arrange
            string queryUrl = "api/Customers?$expand=Books";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JsonConvert.DeserializeObject<List<Customer>>(await response.Content.ReadAsStringAsync());

            Customer customer = Assert.Single(customers);
            Assert.Equal("CustId", customer.Id);
            Assert.Single(customer.Books);
            Assert.Equal("BookId", customer.Books.First().Id);
        }

        /// <summary>
        /// For OData paths enable query should work with expansion.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ActionResultODataPathReturnsExpansion()
        {
            // Arrange
            string queryUrl = "actionresult/Customers?$expand=books";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Customer customer = Assert.Single(customers);
            Assert.Equal("CustId", customer.Id);
            Assert.Single(customer.Books);
            Assert.Equal("BookId", customers.First().Books.First().Id);
        }

        /// <summary>
        /// For OData paths enable query should work without expansion.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ActionResultODataPathReturnsBaseWithoutExpansion()
        {
            // Arrange
            string queryUrl = "actionresult/Customers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Customer customer = Assert.Single(customers);
            Assert.Equal("CustId", customer.Id);
            Assert.Null(customer.Books);
        }
    }
}
