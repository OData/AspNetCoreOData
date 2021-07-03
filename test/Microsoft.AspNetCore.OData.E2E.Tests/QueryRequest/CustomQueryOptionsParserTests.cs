// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Routing.QueryRequest
{
    public class CustomQueryOptionsParserTests : WebApiTestBase<CustomQueryOptionsParserTests>
    {
        public CustomQueryOptionsParserTests(WebApiTestFixture<CustomQueryOptionsParserTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(DollarQueryCustomersController));
            services.AddControllers().AddOData(opt => opt.AddRouteComponents("odata", GetEdmModel()).Count().Filter().OrderBy().Expand().SetMaxTop(null).Select());
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IODataQueryRequestParser, CustomODataQueryOptionsParser>());
            // NOTE: The following statement also does what is expected
            // services.AddSingleton<IODataQueryRequestParser, CustomODataQueryOptionsParser>();
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            // Add OData /$query middleware
            app.UseODataQueryRequest();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();

            builder.EntitySet<DollarQueryCustomer>("DollarQueryCustomers");
            builder.EntitySet<DollarQueryOrder>("DollarQueryOrders");

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ODataQueryOptionsInRequestBody_ForSupportedMediaType()
        {
            // Arrange
            string requestUri = "odata/DollarQueryCustomers/$query";
            var contentType = "text/xml";
            var queryOptionsPayload = "<QueryOptions><QueryOption Option=\"$filter\" Value=\"Id eq 1\"/></QueryOptions>";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(queryOptionsPayload);
            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(contentType);
            request.Content.Headers.ContentLength = queryOptionsPayload.Length;

            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=minimal"));
            HttpClient client = CreateClient();

            // Act
            var response = await client.SendAsync(request);

            // Arrange
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"value\":[{\"Id\":1,\"Name\":\"Customer Name 1\"}]",
                await response.Content.ReadAsStringAsync());
        }
    }
}
