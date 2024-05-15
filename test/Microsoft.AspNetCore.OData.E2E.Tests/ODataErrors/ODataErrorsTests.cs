//-----------------------------------------------------------------------------
// <copyright file="ODataErrorsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System.IO;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors
{
    public class ODataErrorsTests : WebApiTestBase<ODataErrorsTests>
    {
        private readonly ITestOutputHelper output;

        public ODataErrorsTests(WebApiTestFixture<ODataErrorsTests> fixture, ITestOutputHelper output)
            : base(fixture)
        {
            this.output = output;
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            IEdmModel edmModel = ODataErrorsEdmModel.GetEdmModel();

            services.ConfigureControllers(typeof(CustomersController),
                typeof(OrdersController));

            services.AddControllers().AddOData(opt =>
                opt.Count().Filter().OrderBy().Expand().SetMaxTop(null).Select().AddRouteComponents("odataerrors", edmModel));
        }

        [Theory]
        [InlineData("odataerrors/Customers(1)", "{\"error\":{\"code\":\"404\",\"message\":\"Customer with key: 1 not found.\"}}")]
        [InlineData("odataerrors/Orders(1)", "{\"error\":{\"code\":\"404\",\"message\":\"Order with key: 1 not found.\"}}")]
        public async Task NotFoundResponseFromODataControllerIsSerializedAsODataError(string queryUrl, string expectedResponse)
        {
            // Arrange
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            using HttpClient client = CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Theory]
        [InlineData("odataerrors/Customers")]
        [InlineData("odataerrors/Orders")]
        public async Task UnauthorizedResponseFromODataControllerIsSerializedAsODataError(string queryUrl)
        {
            // Arrange
            const string expectedResponse = "{\"error\":{\"code\":\"401\",\"message\":\"Not authorized to access this resource.\"}}";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);           
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            using HttpClient client = CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Theory]
        [InlineData("odataerrors/Customers(1000)", @"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Customer',
                            'ID':1000,'Name':'Customer 1000,}}")]
        [InlineData("odataerrors/Orders(1000)", @"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Order',
                            'ID':1000,'Name':'Order 1000,}}")]
        public async Task ConflictResponseFromODataControllerIsSerializedAsODataError(string queryUrl, string requestContent)
        {
            // Arrange
            const string expectedResponse = "{\"error\":{\"code\":\"409\",\"message\":\"Conflict during update.\"}}";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, queryUrl);
            using var content = new StringContent(requestContent);
            request.Content = content;
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            using HttpClient client = CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Theory]
        [InlineData("odataerrors/Customers(1)")]
        [InlineData("odataerrors/Orders(1)")]
        public async Task BadRequestResponseFromODataControllerIsSerializedAsODataError(string queryUrl)
        {
            // Arrange
            const string expectedResponse = "{\"error\":{\"code\":\"400\",\"message\":\"Bad request on delete.\"}}";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            using HttpClient client = CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Theory]
        [InlineData(
            "odataerrors/Customers",
            @"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Customer', 'ID':1000,'Name':'Customer 1000}}",
            "{\"error\":{\"code\":\"422\",\"message\":\"Unprocessable customer object.\"}}")]
        [InlineData(
            "odataerrors/Orders",
            @"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Order', 'ID':1000,'Name':'Order 1000}}",
            "{\"error\":{\"code\":\"422\",\"message\":\"Unprocessable order object.\"}}")]
        public async Task UnprocessableEntityResponseFromODataControllerIsSerializedAsODataError(string queryUrl, string requestContent, string expectedResponse)
        {
            // Arrange

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            using var content = new StringContent(requestContent);
            request.Content = content;
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            using HttpClient client = CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Theory]
        [InlineData("odataerrors/Customers(1000)", @"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Customer',
                            'ID':1000,'Name':'Customer 1000,}}")]
        [InlineData("odataerrors/Orders(1000)", @"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Order',
                            'ID':1000,'Name':'Order 1000,}}")]
        public async Task ODataErrorResultResponseFromODataControllerIsSerializedAsODataError(string queryUrl, string requestContent)
        {
            // Arrange
            const string expectedResponse = "{\"error\":{\"code\":\"400\",\"message\":\"Bad request during PUT.\"}}";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, queryUrl);
            using var content = new StringContent(requestContent);
            request.Content = content;
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            using HttpClient client = CreateClient();

            // Act
            using HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Fact]
        public void TestDirectoryAccess()
        {
            var odataErrorJsonFile = Path.Combine(Environment.CurrentDirectory, "ODataErrors\\ODataError.json");

            try
            {

                if (File.Exists(odataErrorJsonFile))
                {
                    Assert.True(true);
                }
                else
                {
                    Assert.True(false, "ODataError.json file not found");
                }
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
        }
    }
}
