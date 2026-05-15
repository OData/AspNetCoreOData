//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchMessageSizeLimitTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

/// <summary>
/// Verifies that DefaultMaxReceivedMessageSize of 100 MB is enforced by default
/// on batch requests in a minimal API host.
/// </summary>
public class MinimalApiBatchDefaultMessageSizeLimitTests : IClassFixture<MinimalTestFixture<MinimalApiBatchDefaultMessageSizeLimitTests>>
{
    private HttpClient _client;

    public MinimalApiBatchDefaultMessageSizeLimitTests(MinimalTestFixture<MinimalApiBatchDefaultMessageSizeLimitTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData();
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
    public async Task Batch_PayloadUnderDefaultLimit_Succeeds()
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
    public async Task Batch_PayloadExceedingDefaultLimit_IsRejected()
    {
        // Arrange — 105 MB payload exceeds the 100MB batch handler default limit.
        var id = 2;
        var largePayload = new string('X', 105 * 1024 * 1024);

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
