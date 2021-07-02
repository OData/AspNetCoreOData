// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.E2E.Tests.Query.ActionResult;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests
{
    /// <summary>
    /// EnableQuery attribute works correctly when controller returns ActionResult.
    /// </summary>
    public class ODataActionResultTests : WebApiTestBase<ODataActionResultTests>
    {

        public ODataActionResultTests(WebApiTestFixture<ODataActionResultTests> fixture, ITestOutputHelper output)
            : base(fixture, output)
        {
            ConfigureServicesAction = (IServiceCollection services) =>
            {
                services.ConfigureControllers(typeof(CustomersController));

                IEdmModel model = ActionResultEdmModel.GetEdmModel();
                services.AddControllers().AddOData(options => options.AddModel("odata", model).EnableODataQuery(2));
            };
        }

        /// <summary>
        /// For OData paths enable query should work with expansion.
        /// </summary>
        /// <returns>Task tracking operation.</returns>
        [Fact]
        public async Task ActionResultODataPathReturnsExpansion()
        {
            // Arrange
            string queryUrl = "odata/Customers?$expand=books";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await CreateClient().SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Assert.Single(customers);
            Assert.Equal("CustId", customers.First().Id);
            Assert.Single(customers.First().Books);
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
            string queryUrl = "odata/Customers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await CreateClient().SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Assert.Single(customers);
            Assert.Equal("CustId", customers.First().Id);
            Assert.Null(customers.First().Books);
        }

        /// <summary>
        /// $filter should work with $count.
        /// </summary>
        [Theory]
        [InlineData(1, 1)]
        [InlineData(0, 0)]
        public async Task ActionResultShouldAllowCountInFilter(int filterParam, int expectedItemsCount)
        {
            // Arrange
            string queryUrl = $"odata/Customers?$filter=Books/$count eq {filterParam}";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));

            // Act
            HttpResponseMessage response = await CreateClient().SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            List<Customer> customers = JToken.Parse(await response.Content.ReadAsStringAsync())["value"].ToObject<List<Customer>>();

            Assert.Equal(expectedItemsCount, customers.Count());

        }
    }
}
