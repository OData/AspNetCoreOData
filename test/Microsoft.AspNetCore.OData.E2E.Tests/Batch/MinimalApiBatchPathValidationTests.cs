//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchPathValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
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

public class MinimalApiBatchPathValidationTests : IClassFixture<MinimalTestFixture<MinimalApiBatchPathValidationTests>>
{
    private static int inRootWriteCount;
    private static int outsideWriteCount;
    private readonly HttpClient _client;

    public MinimalApiBatchPathValidationTests(MinimalTestFixture<MinimalApiBatchPathValidationTests> factory)
    {
        _client = factory.CreateClient();
        Interlocked.Exchange(ref inRootWriteCount, 0);
        Interlocked.Exchange(ref outsideWriteCount, 0);
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<MinimalApiBatchPathItem>("Items");
        IEdmModel model = builder.GetEdmModel();
        app.UseODataMiniBatching("mini/$batch", model);
        app.Use(async (context, next) =>
        {
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path == "/outside/write")
            {
                Interlocked.Increment(ref outsideWriteCount);
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next();
        });
        app.MapPost("mini/write", () =>
        {
            Interlocked.Increment(ref inRootWriteCount);
            return Http.Results.NoContent();
        }).WithODataModel(model);
    }

    [Fact]
    public async Task BatchExecutesSingleRequestWithinRoutePrefix()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/mini/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
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
        Assert.Equal(1, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }

    [Fact]
    public async Task BatchExecutesMultipleRequestsWithinRoutePrefix()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/mini/write""
                },
                {
                    ""id"": ""2"",
                    ""method"": ""POST"",
                    ""url"": ""/mini/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
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

        Assert.Equal(new[] { StatusCodes.Status204NoContent, StatusCodes.Status204NoContent }, statuses);
        Assert.Equal(2, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }

    [Fact]
    public async Task BatchRejectsAbsoluteRequestOutsideRoutePrefix()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""http://localhost/outside/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }

    [Fact]
    public async Task BatchRejectsRelativeParentRequestOutsideRoutePrefix()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""../../outside/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }

    [Fact]
    public async Task BatchRejectsSiblingPrefix()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/mini-admin/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }

    [Fact]
    public async Task BatchRejectsMixedPathsBeforeExecutingAnyRequest()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/mini/write""
                },
                {
                    ""id"": ""2"",
                    ""method"": ""POST"",
                    ""url"": ""/outside/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }
}

public class MinimalApiBatchPathBaseValidationTests : IClassFixture<MinimalTestFixture<MinimalApiBatchPathBaseValidationTests>>
{
    private static int inRootWriteCount;
    private static int outsideWriteCount;
    private readonly HttpClient _client;

    public MinimalApiBatchPathBaseValidationTests(MinimalTestFixture<MinimalApiBatchPathBaseValidationTests> factory)
    {
        _client = factory.CreateClient();
        Interlocked.Exchange(ref inRootWriteCount, 0);
        Interlocked.Exchange(ref outsideWriteCount, 0);
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<MinimalApiBatchPathItem>("Items");
        IEdmModel model = builder.GetEdmModel();
        app.UsePathBase("/base");
        app.UseODataMiniBatching("mini/$batch", model);
        app.Use(async (context, next) =>
        {
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path == "/outside/write")
            {
                Interlocked.Increment(ref outsideWriteCount);
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next();
        });
        app.MapPost("mini/write", () =>
        {
            Interlocked.Increment(ref inRootWriteCount);
            return Http.Results.NoContent();
        }).WithODataModel(model);
    }

    [Fact]
    public async Task BatchCombinesPathBaseAndRoutePrefix()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/base/mini/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/base/mini/$batch");
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
        Assert.Equal(1, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }

    [Fact]
    public async Task BatchRejectsRequestOutsideRoutePrefixUnderPathBase()
    {
        string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""/base/outside/write""
                }
            ]
        }";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/base/mini/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

        Exception exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));

        Assert.NotNull(exception);
        Assert.Contains("is not within the OData service root", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, inRootWriteCount);
        Assert.Equal(0, outsideWriteCount);
    }
}

public class MinimalApiBatchPathItem
{
    public int Id { get; set; }
}
