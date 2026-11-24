//-----------------------------------------------------------------------------
// <copyright file="ControllerBatchConstrainedTemplatePrefixValidationTests.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class ControllerBatchConstrainedTemplatePrefixValidationTests : WebApiTestBase<ControllerBatchConstrainedTemplatePrefixValidationTests>
{
    private static IEdmModel edmModel;

    public ControllerBatchConstrainedTemplatePrefixValidationTests(WebApiTestFixture<ControllerBatchConstrainedTemplatePrefixValidationTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(ControllerBatchTemplatePrefixItemsController));

        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<ControllerBatchTemplatePrefixItem>("ControllerBatchTemplatePrefixItems");
        edmModel = builder.GetEdmModel();
        services.AddControllers().AddOData(options => options.AddRouteComponents("v{version:int}", edmModel, new DefaultODataBatchHandler()));
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseODataBatching();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    [Theory]
    [InlineData("/v2/ControllerBatchTemplatePrefixItems", "/v2/$batch")]
    [InlineData("/v3/ControllerBatchTemplatePrefixItems", "/v3/$batch")]
    [InlineData("/v4/ControllerBatchTemplatePrefixItems", "/v4/$batch")]
    [InlineData("/v10/ControllerBatchTemplatePrefixItems", "/v10/$batch")]
    [InlineData("/v999/ControllerBatchTemplatePrefixItems", "/v999/$batch")]
    public async Task BatchAcceptsRequestUnderResolvedConstrainedTemplatePrefix(string requestUrl, string batchUrl)
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
    [InlineData("/v3/ControllerBatchTemplatePrefixItems")]
    [InlineData("/v4/ControllerBatchTemplatePrefixItems")]
    [InlineData("/v5/ControllerBatchTemplatePrefixItems")]
    public async Task BatchRejectsRequestUnderDifferentConstrainedTemplatePrefix(string requestUrl)
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
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/v2/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
    }
}
