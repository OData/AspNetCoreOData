//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchMessageSizeLimitTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

/// <summary>
/// Verifies that a small configured MaxReceivedMessageSize (1 MB) is enforced
/// on batch requests in a minimal API host. Uses a small limit to avoid OOM in CI
/// while still exercising the same code path as the default 100 MB limit.
/// </summary>
public class MinimalApiBatchSmallMessageSizeLimitTests : IClassFixture<MinimalTestFixture<MinimalApiBatchSmallMessageSizeLimitTests>>
{
    private const long SmallMaxReceivedMessageSize = 1 * 1024 * 1024; // 1 MB

    private HttpClient _client;

    public MinimalApiBatchSmallMessageSizeLimitTests(MinimalTestFixture<MinimalApiBatchSmallMessageSizeLimitTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData(opt => opt.SetMaxReceivedMessageSize(SmallMaxReceivedMessageSize));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = GetEdmModel();

        app.UseODataMiniBatching("odata/$batch", model);

        app.MapPost("odata/MessageSizeItems", ([FromBody] MessageSizeItem item) => Http.Results.Created($"http://localhost/odata/MessageSizeItems({item.Id})", item))
           .WithODataResult()
           .WithODataModel(model);
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Batch_PayloadUnderSmallLimit_Succeeds()
    {
        // Arrange — a tiny payload keeps the total batch envelope well under 4 KB.
        var id = 1;
        var payload = "A";

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST /odata/MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{payload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("HTTP/1.1 201 Created", responseBody, StringComparison.Ordinal);
        Assert.Contains($@"""id"":{id}", responseBody, StringComparison.Ordinal);
        Assert.Contains($@"""payload"":""{payload}""", responseBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Batch_PayloadExceedingConfiguredSmallLimit_IsRejected()
    {
        // Arrange — 2 MB payload exceeds the 1 MB configured limit.
        var id = 2;
        var largePayload = new string('X', 2 * 1024 * 1024);

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST /odata/MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{largePayload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        // Act & Assert — when the batch stream exceeds MaxReceivedMessageSize,
        // the OData reader throws ODataException which propagates through the pipeline.
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }
}

/// <summary>
/// Verifies that ConfiguredMaxReceivedMessageSize of 10 MB is enforced
/// on batch requests in a minimal API host.
/// </summary>
public class MinimalApiBatchConfiguredMessageSizeLimitTests : IClassFixture<MinimalTestFixture<MinimalApiBatchConfiguredMessageSizeLimitTests>>
{
    private const long ConfiguredMaxReceivedMessageSize = 10 * 1024 * 1024; // 10 MB
    private HttpClient _client;

    public MinimalApiBatchConfiguredMessageSizeLimitTests(MinimalTestFixture<MinimalApiBatchConfiguredMessageSizeLimitTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData(opt => opt.SetMaxReceivedMessageSize(ConfiguredMaxReceivedMessageSize));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = GetEdmModel();

        app.UseODataMiniBatching("odata/$batch", model);

        app.MapPost("odata/MessageSizeItems", ([FromBody] MessageSizeItem item) => Http.Results.Created($"http://localhost/odata/MessageSizeItems({item.Id})", item))
           .WithODataResult()
           .WithODataModel(model);
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Batch_PayloadUnderConfiguredLimit_Succeeds()
    {
        // Arrange — a tiny payload keeps the total batch envelope well under 4 KB.
        var id = 1;
        var payload = "A";

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST /odata/MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{payload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("HTTP/1.1 201 Created", responseBody, StringComparison.Ordinal);
        Assert.Contains($@"""id"":{id}", responseBody, StringComparison.Ordinal);
        Assert.Contains($@"""payload"":""{payload}""", responseBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Batch_PayloadExceedingConfiguredLimit_IsRejected()
    {
        // Arrange — 20 MB payload exceeds the 10 MB batch handler configured limit.
        var id = 2;
        var largePayload = new string('X', 20 * 1024 * 1024);

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST /odata/MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{largePayload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        // Act & Assert — when the batch stream exceeds MaxReceivedMessageSize,
        // the OData reader throws ODataException which propagates through the pipeline.
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }
}


/// <summary>
/// Verifies that a specific batch endpoint can override the global MaxReceivedMessageSize
/// by using a custom <see cref="DefaultODataBatchHandler"/>.
/// </summary>
public class MinimalApiBatchPerEndpointMessageSizeLimitTests : IClassFixture<MinimalTestFixture<MinimalApiBatchPerEndpointMessageSizeLimitTests>>
{
    private const long GlobalMaxReceivedMessageSize = 10 * 1024 * 1024; // 10 MB
    private const long EndpointMaxReceivedMessageSize = 1 * 1024 * 1024; // 1 MB
    private HttpClient _client;

    public MinimalApiBatchPerEndpointMessageSizeLimitTests(MinimalTestFixture<MinimalApiBatchPerEndpointMessageSizeLimitTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData(opt => opt.SetMaxReceivedMessageSize(GlobalMaxReceivedMessageSize));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = GetEdmModel();

        var batchHandler = new DefaultODataBatchHandler();
        batchHandler.MessageQuotas.MaxReceivedMessageSize = EndpointMaxReceivedMessageSize;
        app.UseODataMiniBatching("odata/$batch", model, batchHandler);

        app.MapPost("odata/MessageSizeItems", ([FromBody] MessageSizeItem item) => Http.Results.Created($"http://localhost/odata/MessageSizeItems({item.Id})", item))
            .WithODataResult()
            .WithODataModel(model);
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Batch_PayloadExceedingEndpointSpecificLimit_IsRejected()
    {
        // Arrange — 2 MB exceeds the endpoint-specific 1 MB limit,
        // and remains under the global 10 MB limit.
        var id = 3;
        var largePayload = new string('X', 2 * 1024 * 1024);

        var batchBoundary = "batch_36522ad7-fc75-4b56-8c71-56071383e77b";
        var changesetBoundary = "changeset_36522ad7-fc75-4b56-8c71-56071383e77b";

        var batchBody =
$@"--{batchBoundary}
Content-Type: multipart/mixed; boundary={changesetBoundary}

--{changesetBoundary}
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: 1

POST /odata/MessageSizeItems HTTP/1.1
Content-Type: application/json;odata.metadata=minimal
Accept: application/json;odata.metadata=minimal
Accept-Charset: UTF-8
OData-Version: 4.0

{{""{nameof(MessageSizeItem.Id)}"":{id},""{nameof(MessageSizeItem.Payload)}"":""{largePayload}""}}
--{changesetBoundary}--
--{batchBoundary}--
";
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/$batch");
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("multipart/mixed"));

        var content = new StringContent(batchBody);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse($"multipart/mixed; boundary={batchBoundary}");
        request.Content = content;

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }
}
