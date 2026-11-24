//-----------------------------------------------------------------------------
// <copyright file="ControllerBatchOptionalTemplatePrefixValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
    public class ControllerBatchOptionalTemplatePrefixValidationTests : WebApiTestBase<ControllerBatchOptionalTemplatePrefixValidationTests>
    {
        private static IEdmModel edmModel;

        public ControllerBatchOptionalTemplatePrefixValidationTests(WebApiTestFixture<ControllerBatchOptionalTemplatePrefixValidationTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(ControllerBatchTemplatePrefixItemsController));

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ControllerBatchTemplatePrefixItem>("ControllerBatchTemplatePrefixItems");
            edmModel = builder.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("{tenant?}/v2", edmModel, new DefaultODataBatchHandler()));
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            app.UseODataBatching();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        [Theory]
        [InlineData("/contoso/v2/ControllerBatchTemplatePrefixItems", "/contoso/v2/$batch")]
        [InlineData("http://localhost/contoso/v2/ControllerBatchTemplatePrefixItems", "/contoso/v2/$batch")]
        [InlineData("/fabrikam/v2/ControllerBatchTemplatePrefixItems", "/fabrikam/v2/$batch")]
        [InlineData("http://localhost/fabrikam/v2/ControllerBatchTemplatePrefixItems", "/fabrikam/v2/$batch")]
        [InlineData("/some-tenant/v2/ControllerBatchTemplatePrefixItems", "/some-tenant/v2/$batch")]
        [InlineData("http://localhost/some-tenant/v2/ControllerBatchTemplatePrefixItems", "/some-tenant/v2/$batch")]
        public async Task BatchAcceptsRequestUnderResolvedOptionalTemplatePrefix(string requestUrl, string batchUrl)
        {
            HttpClient client = CreateClient();
            string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": """ + requestUrl + @"""
                }
            ]
        }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, batchUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var statuses = new List<int>();
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            IODataResponseMessage responseMessage = new ODataMessageWrapper(responseStream, response.Content.Headers);
            using ODataMessageReader messageReader = new ODataMessageReader(responseMessage, new ODataMessageReaderSettings(), edmModel);
            ODataBatchReader batchReader = messageReader.CreateODataBatchReader();
            while (batchReader.Read())
            {
                if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    statuses.Add(batchReader.CreateOperationResponseMessage().StatusCode);
                }
            }

            Assert.Equal(new[] { StatusCodes.Status200OK }, statuses);
        }

        [Theory]
        [InlineData("/contoso/v2/ControllerBatchTemplatePrefixItems", "/fabrikam/v2/$batch")]
        [InlineData("/fabrikam/v2/ControllerBatchTemplatePrefixItems", "/contoso/v2/$batch")]
        [InlineData("/some-tenant/v2/ControllerBatchTemplatePrefixItems", "/oya/v2/$batch")]
        public async Task BatchRejectsRequestUnderDifferentOptionalTemplatePrefix(string requestUrl, string batchUrl)
        {
            HttpClient client = CreateClient();
            string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": """ + requestUrl + @"""
                }
            ]
        }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, batchUrl);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            Exception exception = await Record.ExceptionAsync(async () => await client.SendAsync(request));

            Assert.NotNull(exception);
            Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task BatchRouteDoesNotMatchWhenOptionalLeadingSegmentIsOmitted()
        {
            HttpClient client = CreateClient();
            const string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": ""/v2/ControllerBatchTemplatePrefixItems""
                }
            ]
        }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/v2/$batch");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
