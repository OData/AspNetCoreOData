//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchConstrainedTemplatePrefixValidationTests.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.OData.E2E.Tests.Commons;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class MinimalApiBatchConstrainedTemplatePrefixValidationTests : IClassFixture<MinimalTestFixture<MinimalApiBatchConstrainedTemplatePrefixValidationTests>>
{
    private readonly HttpClient _client;

    public MinimalApiBatchConstrainedTemplatePrefixValidationTests(MinimalTestFixture<MinimalApiBatchConstrainedTemplatePrefixValidationTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<MinimalApiBatchPathItem>("Items");
        IEdmModel model = builder.GetEdmModel();
        app.UseODataMiniBatching("v{version:int}/$batch", model);
        app.MapPost("v{version:int}/write", () => Http.Results.NoContent()).WithODataModel(model);
    }

    [Theory]
    [InlineData("/v2/write", "/v2/$batch")]
    [InlineData("/v3/write", "/v3/$batch")]
    [InlineData("/v2001/write", "/v2001/$batch")]
    public async Task BatchAcceptsRequestUnderResolvedConstrainedTemplatePrefix(string requestUrl, string batchUrl)
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": """ + requestUrl + @"""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, batchUrl);
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
    }

    [Theory]
    [InlineData("/v3/write", "/v2/$batch")]
    [InlineData("/v2001/write", "/v2/$batch")]
    public async Task BatchRejectsRequestUnderDifferentConstrainedTemplatePrefix(string requestUrl, string batchUrl)
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": """ + requestUrl + @"""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, batchUrl);
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
    }
}
