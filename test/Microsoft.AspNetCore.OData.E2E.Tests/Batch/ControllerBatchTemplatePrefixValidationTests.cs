//-----------------------------------------------------------------------------
// <copyright file="ControllerBatchTemplatePrefixValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch
{
    public class ControllerBatchTemplatePrefixValidationTests : WebApiTestBase<ControllerBatchTemplatePrefixValidationTests>
    {
        private static IEdmModel edmModel;

        public ControllerBatchTemplatePrefixValidationTests(WebApiTestFixture<ControllerBatchTemplatePrefixValidationTests> fixture)
            : base(fixture)
        {
        }

        protected static void UpdateConfigureServices(IServiceCollection services)
        {
            services.ConfigureControllers(typeof(ControllerBatchTemplatePrefixItemsController));

            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<ControllerBatchTemplatePrefixItem>("ControllerBatchTemplatePrefixItems");
            edmModel = builder.GetEdmModel();
            services.AddControllers().AddOData(options => options.AddRouteComponents("v2{data}", edmModel, new DefaultODataBatchHandler()));
        }

        protected static void UpdateConfigure(IApplicationBuilder app)
        {
            app.UseODataBatching();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        [Theory]
        [InlineData("/v2Customers/$batch", "/v2Customers/ControllerBatchTemplatePrefixItems")]
        [InlineData("/v2Customers/$batch", "http://localhost/v2Customers/ControllerBatchTemplatePrefixItems")]
        [InlineData("/v2Customers/$batch", "ControllerBatchTemplatePrefixItems")]
        [InlineData("/v2Customers/$batch/", "/v2Customers/ControllerBatchTemplatePrefixItems")]
        public async Task BatchAcceptsRequestUnderResolvedTemplatePrefix(string batchPath, string requestPath)
        {
            HttpClient client = CreateClient();
            string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": """ + requestPath + @"""
                }
            ]
        }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, batchPath);
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
        [InlineData("/v2Orders/ControllerBatchTemplatePrefixItems")]
        [InlineData("http://localhost/v2Orders/ControllerBatchTemplatePrefixItems")]
        [InlineData("/v2CustomersAdmin/ControllerBatchTemplatePrefixItems")]
        [InlineData("../v2Orders/ControllerBatchTemplatePrefixItems")]
        public async Task BatchRejectsRequestOutsideResolvedTemplatePrefix(string requestPath)
        {
            HttpClient client = CreateClient();
            string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": """ + requestPath + @"""
                }
            ]
        }";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/v2Customers/$batch");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            Exception exception = await Record.ExceptionAsync(async () => await client.SendAsync(request));

            Assert.NotNull(exception);
            Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public async Task BatchResolvesTemplatePrefixIndependentlyForEachRequest()
        {
            HttpClient client = CreateClient();
            const string customerPayload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": ""/v2Customers/ControllerBatchTemplatePrefixItems""
                }
            ]
        }";
            HttpRequestMessage customerRequest = new HttpRequestMessage(HttpMethod.Post, "/v2Customers/$batch");
            customerRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            customerRequest.Content = new StringContent(customerPayload, Encoding.UTF8, "application/json");

            using HttpResponseMessage customerResponse = await client.SendAsync(customerRequest);

            Assert.Equal(HttpStatusCode.OK, customerResponse.StatusCode);
            var customerStatuses = new List<int>();
            Stream customerResponseStream = await customerResponse.Content.ReadAsStreamAsync();
            IODataResponseMessage customerResponseMessage = new ODataMessageWrapper(customerResponseStream, customerResponse.Content.Headers);
            using ODataMessageReader customerMessageReader = new ODataMessageReader(customerResponseMessage, new ODataMessageReaderSettings(), edmModel);
            ODataBatchReader customerBatchReader = customerMessageReader.CreateODataBatchReader();
            while (customerBatchReader.Read())
            {
                if (customerBatchReader.State == ODataBatchReaderState.Operation)
                {
                    customerStatuses.Add(customerBatchReader.CreateOperationResponseMessage().StatusCode);
                }
            }

            Assert.Equal(new[] { StatusCodes.Status200OK }, customerStatuses);

            const string orderPayload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""GET"",
                    ""url"": ""/v2Orders/ControllerBatchTemplatePrefixItems""
                }
            ]
        }";
            HttpRequestMessage orderRequest = new HttpRequestMessage(HttpMethod.Post, "/v2Orders/$batch");
            orderRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            orderRequest.Content = new StringContent(orderPayload, Encoding.UTF8, "application/json");

            using HttpResponseMessage orderResponse = await client.SendAsync(orderRequest);

            Assert.Equal(HttpStatusCode.OK, orderResponse.StatusCode);
            var orderStatuses = new List<int>();
            Stream orderResponseStream = await orderResponse.Content.ReadAsStreamAsync();
            IODataResponseMessage orderResponseMessage = new ODataMessageWrapper(orderResponseStream, orderResponse.Content.Headers);
            using ODataMessageReader orderMessageReader = new ODataMessageReader(orderResponseMessage, new ODataMessageReaderSettings(), edmModel);
            ODataBatchReader orderBatchReader = orderMessageReader.CreateODataBatchReader();
            while (orderBatchReader.Read())
            {
                if (orderBatchReader.State == ODataBatchReaderState.Operation)
                {
                    orderStatuses.Add(orderBatchReader.CreateOperationResponseMessage().StatusCode);
                }
            }

            Assert.Equal(new[] { StatusCodes.Status200OK }, orderStatuses);
        }
    }

    public class ControllerBatchTemplatePrefixItemsController : ODataController
    {
        public ActionResult<IEnumerable<ControllerBatchTemplatePrefixItem>> Get()
        {
            return Ok(new[] { new ControllerBatchTemplatePrefixItem { Id = 1, Name = "Customer" } });
        }
    }

    public class ControllerBatchTemplatePrefixItem
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
