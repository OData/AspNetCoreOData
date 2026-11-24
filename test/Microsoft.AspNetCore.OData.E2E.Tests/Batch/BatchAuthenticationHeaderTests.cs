//-----------------------------------------------------------------------------
// <copyright file="BatchAuthenticationHeaderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch
{
    public class BatchAuthenticationHeaderTests : WebApiTestBase<BatchAuthenticationHeaderTests>
    {
        private static readonly string[] requestContextHeaders =
        {
        "Authorization",
        "Cookie",
        "Proxy-Authorization",
        "X-MS-Client-Principal",
        "X-MS-API-ROLE",
        "X-Forwarded-For",
        "X-MS-Client-Principal-Id",
    };

        private static IEdmModel edmModel;

        public BatchAuthenticationHeaderTests(WebApiTestFixture<BatchAuthenticationHeaderTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(DefaultBatchCustomersController));

            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DefaultBatchCustomer>("DefaultBatchCustomers");
            edmModel = builder.GetEdmModel();

            services.AddControllers().AddOData(options =>
                options.AddRouteComponents("BatchAuthentication", edmModel, new DefaultODataBatchHandler()));
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            app.UseODataBatching();
            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (context.Request.Headers.ContainsKey("X-Test-Capture-Authentication-Headers"))
                {
                    foreach (string headerName in requestContextHeaders)
                    {
                        string value = context.Request.Headers[headerName].ToString();
                        context.Response.Headers["X-Observed-" + headerName] =
                            string.IsNullOrEmpty(value) ? "<absent>" : value;
                    }

                    context.Response.Headers["X-Observed-Accept"] = context.Request.Headers["Accept"].ToString();
                }

                await next();
            });
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BatchSubRequestAuthenticationHeaders_DoNotOverrideOuterRequest(bool useJsonBatch)
        {
            // Arrange & Act
            IDictionary<string, string> observedHeaders = await SendBatchRequestAsync(useJsonBatch, includeOuterHeaders: true);

            // Assert
            Assert.Equal("Bearer outer-user", observedHeaders["Authorization"]);
            Assert.Equal("session=outer", observedHeaders["Cookie"]);
            Assert.Equal("Basic outer-proxy", observedHeaders["Proxy-Authorization"]);
            Assert.Equal("outer-principal", observedHeaders["X-MS-Client-Principal"]);
            Assert.Equal("outer-role", observedHeaders["X-MS-API-ROLE"]);
            Assert.Equal("203.0.113.10", observedHeaders["X-Forwarded-For"]);
            Assert.Equal("outer-principal-id", observedHeaders["X-MS-Client-Principal-Id"]);
            Assert.Equal("application/json", observedHeaders["Accept"]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task BatchSubRequestAuthenticationHeaders_AreRemovedWithoutOuterValues(bool useJsonBatch)
        {
            // Arrange & Act
            IDictionary<string, string> observedHeaders = await SendBatchRequestAsync(
                useJsonBatch,
                includeOuterHeaders: false);

            // Assert
            foreach (string headerName in requestContextHeaders)
            {
                Assert.Equal("<absent>", observedHeaders[headerName]);
            }
        }

        private async Task<IDictionary<string, string>> SendBatchRequestAsync(bool useJsonBatch, bool includeOuterHeaders)
        {
            HttpClient client = CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "BatchAuthentication/$batch");
            request.Headers.TryAddWithoutValidation("X-Test-Capture-Authentication-Headers", "true");

            if (includeOuterHeaders)
            {
                request.Headers.TryAddWithoutValidation("Authorization", "Bearer outer-user");
                request.Headers.TryAddWithoutValidation("Cookie", "session=outer");
                request.Headers.TryAddWithoutValidation("Proxy-Authorization", "Basic outer-proxy");
                request.Headers.TryAddWithoutValidation("X-MS-Client-Principal", "outer-principal");
                request.Headers.TryAddWithoutValidation("X-MS-API-ROLE", "outer-role");
                request.Headers.TryAddWithoutValidation("X-Forwarded-For", "203.0.113.10");
                request.Headers.TryAddWithoutValidation("X-MS-Client-Principal-Id", "outer-principal-id");
            }

            request.Content = useJsonBatch ? CreateJsonBatchContent() : CreateMultipartBatchContent();
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(useJsonBatch ? "application/json" : "multipart/mixed"));

            HttpResponseMessage response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var observedHeaders = new Dictionary<string, string>();
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(responseStream, response.Content.Headers);
            using ODataMessageReader messageReader = new ODataMessageReader(
                responseMessage,
                new ODataMessageReaderSettings(),
                edmModel);
            ODataBatchReader batchReader = messageReader.CreateODataBatchReader();

            while (batchReader.Read())
            {
                if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    ODataBatchOperationResponseMessage operationMessage = batchReader.CreateOperationResponseMessage();
                    Assert.Equal(StatusCodes.Status200OK, operationMessage.StatusCode);
                    foreach (string headerName in requestContextHeaders)
                    {
                        observedHeaders[headerName] = operationMessage.GetHeader("X-Observed-" + headerName);
                    }

                    observedHeaders["Accept"] = operationMessage.GetHeader("X-Observed-Accept");
                }
            }

            Assert.Equal(requestContextHeaders.Length + 1, observedHeaders.Count);
            return observedHeaders;
        }

        private static HttpContent CreateMultipartBatchContent()
        {
            const string boundary = "batch_authentication_headers";
            var content = new StringContent(
                $@"--{boundary}
Content-Type: application/http
Content-Transfer-Encoding: binary

GET http://localhost/BatchAuthentication/DefaultBatchCustomers(1) HTTP/1.1
Host: localhost
Authorization: Bearer inner-admin
Cookie: session=inner
Proxy-Authorization: Basic inner-proxy
X-MS-Client-Principal: inner-principal
X-MS-API-ROLE: inner-role
X-Forwarded-For: 198.51.100.99
X-MS-Client-Principal-Id: inner-principal-id
Accept: application/json


--{boundary}--");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed;boundary={boundary}");
            return content;
        }

        private static HttpContent CreateJsonBatchContent()
        {
            var content = new StringContent(
                @"{
  ""requests"": [
    {
      ""id"": ""1"",
      ""method"": ""GET"",
      ""url"": ""http://localhost/BatchAuthentication/DefaultBatchCustomers(1)"",
      ""headers"": {
        ""Authorization"": ""Bearer inner-admin"",
        ""Cookie"": ""session=inner"",
        ""Proxy-Authorization"": ""Basic inner-proxy"",
        ""X-MS-Client-Principal"": ""inner-principal"",
        ""X-MS-API-ROLE"": ""inner-role"",
        ""X-Forwarded-For"": ""198.51.100.99"",
        ""X-MS-Client-Principal-Id"": ""inner-principal-id"",
        ""Accept"": ""application/json""
      }
    }
  ]
}");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }
    }
}
