//-----------------------------------------------------------------------------
// <copyright file="ODataErrorsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
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

        [Fact]
        public async Task NotFoundResponseFromODataControllerIsSerializedAsODataError()
        {
            // Arrange
            string queryUrl = "odataerrors/Customers(1)";
            const string expectedResponse = "{\"error\":{\"code\":\"404\",\"message\":\"Customer with key: 1 not found.\"}}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Fact]
        public async Task UnauthorizedResponseFromODataControllerIsSerializedAsODataError()
        {
            // Arrange
            string queryUrl = "odataerrors/Customers";
            const string expectedResponse = "{\"error\":{\"code\":\"401\",\"message\":\"Not authorized to access this resource.\"}}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, queryUrl);           
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Fact]
        public async Task ConflictResponseFromODataControllerIsSerializedAsODataError()
        {
            // Arrange
            string queryUrl = "odataerrors/Customers(1000)";
            const string expectedResponse = "{\"error\":{\"code\":\"409\",\"message\":\"Conflict during update.\"}}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, queryUrl);
            request.Content = new StringContent(
                    string.Format(@"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Customer',
                            'ID':1000,'Name':'Customer 1000,}}"));
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Fact]
        public async Task BadRequestResponseFromODataControllerIsSerializedAsODataError()
        {
            // Arrange
            string queryUrl = "odataerrors/Customers(1)";
            const string expectedResponse = "{\"error\":{\"code\":\"400\",\"message\":\"Bad request on delete.\"}}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, queryUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Fact]
        public async Task UnprocessableEntityResponseFromODataControllerIsSerializedAsODataError()
        {
            // Arrange
            string queryUrl = "odataerrors/Customers";
            const string expectedResponse = "{\"error\":{\"code\":\"422\",\"message\":\"Unprocessable customer object.\"}}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, queryUrl);
            request.Content = new StringContent(
                    string.Format(@"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Customer',
                            'ID':1000,'Name':'Customer 1000,}}"));
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }

        [Fact]
        public async Task ODataErrorResultResponseFromODataControllerIsSerializedAsODataError()
        {
            // Arrange
            string queryUrl = "odataerrors/Customers(1000)";
            const string expectedResponse = "{\"error\":{\"code\":\"400\",\"message\":\"Bad request during PUT.\"}}";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, queryUrl);
            request.Content = new StringContent(
                    string.Format(@"{{'@odata.type':'#Microsoft.AspNetCore.OData.E2E.Tests.ODataErrors.Customer',
                            'ID':1000,'Name':'Customer 1000,}}"));
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpClient client = CreateClient();
            HttpResponseMessage response;

            // Act
            response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            string payload = await response.Content.ReadAsStringAsync();
            Assert.Equal(expectedResponse, payload);
        }
    }
}
