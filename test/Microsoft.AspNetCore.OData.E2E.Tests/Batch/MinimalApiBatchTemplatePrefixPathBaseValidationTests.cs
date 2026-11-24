//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchTemplatePrefixPathBaseValidationTests.cs" company=".NET Foundation">
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

public class MinimalApiBatchTemplatePrefixPathBaseValidationTests : IClassFixture<MinimalTestFixture<MinimalApiBatchTemplatePrefixPathBaseValidationTests>>
{
    private readonly HttpClient _client;

    public MinimalApiBatchTemplatePrefixPathBaseValidationTests(MinimalTestFixture<MinimalApiBatchTemplatePrefixPathBaseValidationTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<MinimalApiBatchPathItem>("Items");
        IEdmModel model = builder.GetEdmModel();
        app.UsePathBase("/base");
        app.UseODataMiniBatching("v2{data}/$batch", model);
        app.MapPost("v2{data}/write", () => Http.Results.NoContent()).WithODataModel(model);
    }

    [Fact]
    public async Task BatchAcceptsRequestUnderPathBaseAndResolvedTemplatePrefix()
    {
        const string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/base/v2Customers/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/base/v2Customers/$batch");
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

    [Fact]
    public async Task BatchRejectsRequestOutsidePathBaseAndResolvedTemplatePrefix()
    {
        const string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/base/v2Orders/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/base/v2Customers/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
    }
}
