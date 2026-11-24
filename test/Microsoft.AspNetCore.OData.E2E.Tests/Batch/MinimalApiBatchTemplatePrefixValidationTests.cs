//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchTemplatePrefixValidationTests.cs" company=".NET Foundation">
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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class MinimalApiBatchTemplatePrefixValidationTests : IClassFixture<MinimalTestFixture<MinimalApiBatchTemplatePrefixValidationTests>>
{
    private static int writeCount;
    private readonly HttpClient _client;

    public MinimalApiBatchTemplatePrefixValidationTests(MinimalTestFixture<MinimalApiBatchTemplatePrefixValidationTests> factory)
    {
        _client = factory.CreateClient();
        Interlocked.Exchange(ref writeCount, 0);
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<MinimalApiBatchPathItem>("Items");
        IEdmModel model = builder.GetEdmModel();
        app.UseODataMiniBatching("v2{data}/$batch", model);
        app.MapPost("v2{data}/write", () =>
        {
            Interlocked.Increment(ref writeCount);
            return Http.Results.NoContent();
        }).WithODataModel(model);
    }

    [Theory]
    [InlineData("/v2Customers/$batch", "/v2Customers/write")]
    [InlineData("/v2Customers/$batch", "http://localhost/v2Customers/write")]
    [InlineData("/v2Customers/$batch", "write")]
    [InlineData("/v2Customers/$batch/", "/v2Customers/write")]
    public async Task BatchAcceptsRequestUnderResolvedTemplatePrefix(string batchPath, string requestPath)
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": """ + requestPath + @"""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, batchPath);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var statuses = new List<int>();
        Stream responseStream = await response.Content.ReadAsStreamAsync();
        IODataResponseMessage responseMessage = new ODataMessageWrapper(responseStream, response.Content.Headers);
        using ODataMessageReader messageReader = new ODataMessageReader(responseMessage, new ODataMessageReaderSettings(), new ODataConventionModelBuilder().GetEdmModel());
        ODataBatchReader batchReader = messageReader.CreateODataBatchReader();
        while (batchReader.Read())
        {
            if (batchReader.State == ODataBatchReaderState.Operation)
            {
                statuses.Add(batchReader.CreateOperationResponseMessage().StatusCode);
            }
        }

        Assert.Equal(new[] { StatusCodes.Status204NoContent }, statuses);
        Assert.Equal(1, writeCount);
    }

    [Theory]
    [InlineData("/v2Orders/write")]
    [InlineData("http://localhost/v2Orders/write")]
    [InlineData("/v2CustomersAdmin/write")]
    [InlineData("../v2Orders/write")]
    public async Task BatchRejectsRequestOutsideResolvedTemplatePrefix(string requestPath)
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": """ + requestPath + @"""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/v2Customers/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, writeCount);
    }

    [Fact]
    public async Task BatchResolvesTemplatePrefixIndependentlyForEachRequest()
    {
        const string customerPayload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/v2Customers/write""
                }
            ]
        }";
        HttpRequestMessage customerRequest = new HttpRequestMessage(HttpMethod.Post, "/v2Customers/$batch");
        customerRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        customerRequest.Content = new StringContent(customerPayload, Encoding.UTF8, "application/json");

        using HttpResponseMessage customerResponse = await _client.SendAsync(customerRequest);

        Assert.Equal(HttpStatusCode.OK, customerResponse.StatusCode);

        const string orderPayload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/v2Orders/write""
                }
            ]
        }";
        HttpRequestMessage orderRequest = new HttpRequestMessage(HttpMethod.Post, "/v2Orders/$batch");
        orderRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        orderRequest.Content = new StringContent(orderPayload, Encoding.UTF8, "application/json");

        using HttpResponseMessage orderResponse = await _client.SendAsync(orderRequest);

        Assert.Equal(HttpStatusCode.OK, orderResponse.StatusCode);
        Assert.Equal(2, writeCount);
    }
}
