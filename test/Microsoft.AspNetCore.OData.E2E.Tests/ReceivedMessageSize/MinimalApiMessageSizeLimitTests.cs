//-----------------------------------------------------------------------------
// <copyright file="MinimalApiMessageSizeLimitTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
/// Verifies that <c>SetMaxReceivedMessageSize(...)</c> configured enforces the message size limit on requests in a minimal API host.
/// </summary>
public class MinimalApiMessageSizeLimitConfiguredTests : IClassFixture<MinimalTestFixture<MinimalApiMessageSizeLimitConfiguredTests>>
{
    // A deliberately small limit to make testing fast and deterministic.
    private const int ConfiguredMaxMessageSize = 1 * 1024 * 1024; // 1 MB

    private HttpClient _client;

    public MinimalApiMessageSizeLimitConfiguredTests(MinimalTestFixture<MinimalApiMessageSizeLimitConfiguredTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData(opt => opt.SetMaxReceivedMessageSize(ConfiguredMaxMessageSize));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        IEdmModel model = GetEdmModel();

        app.MapPost("odata/MessageSizeItems", ([FromBody] MessageSizeItem item) => Http.Results.Created($"/odata/MessageSizeItems({item.Id})", item))
            .WithODataResult()
            .WithODataModel(model)
            .WithODataOptions(opt => opt.SetMaxReceivedMessageSize(ConfiguredMaxMessageSize));
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Post_SmallPayload_Succeeds()
    {
        // Arrange — a tiny payload well under 1 KB.
        string json = "{\"Id\":1,\"Payload\":\"A\"}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await _client.SendAsync(request);

        // Assert
        string responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Post_PayloadExceedingConfiguredLimit_Fails()
    {
        // Arrange — 5 MB exceeds the 1 MB configured limit.
        var payload = new string('X', 5 * 1024 * 1024);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }

    [Fact]
    public async Task Post_PayloadJustUnderConfiguredLimit_Succeeds()
    {
        // Arrange — payload sized to keep total JSON body just under the 1 MB limit.
        // The JSON envelope {"Id":1,"Payload":"..."} adds ~25 bytes of overhead.
        var payload = new string('X', ConfiguredMaxMessageSize - 100);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Post_PayloadExactlyAtConfiguredLimit_Succeeds()
    {
        // Arrange — construct payload so Content-Length == ConfiguredMaxMessageSize exactly.
        // The middleware uses strict greater-than (>) so exactly at the limit should pass.
        string jsonTemplate = "{\"Id\":1,\"Payload\":\"\"}";
        int overhead = Encoding.UTF8.GetByteCount(jsonTemplate);
        var payload = new string('X', ConfiguredMaxMessageSize - overhead);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Post_PayloadOneByteOverConfiguredLimit_Fails()
    {
        // Arrange — construct payload so Content-Length == ConfiguredMaxMessageSize + 1.
        // The middleware uses strict greater-than (>) so one byte over should fail.
        string jsonTemplate = "{\"Id\":1,\"Payload\":\"\"}";
        int overhead = Encoding.UTF8.GetByteCount(jsonTemplate);
        var payload = new string('X', ConfiguredMaxMessageSize - overhead + 1);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }
}

/// <summary>
/// Verifies that the DefaultMaxReceivedMessageSize of 100 MB is enforced by default
/// on POST requests in a minimal API host.
/// </summary>
public class MinimalApiMessageSizeLimitDefaultTests : IClassFixture<MinimalTestFixture<MinimalApiMessageSizeLimitDefaultTests>>
{
    private HttpClient _client;

    public MinimalApiMessageSizeLimitDefaultTests(MinimalTestFixture<MinimalApiMessageSizeLimitDefaultTests> factory)
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
    public async Task Post_SmallPayload_Succeeds()
    {
        // Arrange — 1 KB payload is well under the 100 MB default limit.
        var payload = new string('X', 1024);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains(payload, responseBody);
    }

    [Fact]
    public async Task Post_PayloadExceedingDefaultLimit_Fails()
    {
        // Arrange — 105 MB exceeds the 100 MB default limit.
        // This allocation is large; if it causes OOM the configured-limit tests
        // cover the same logic with a 1 KB limit.
        var payload = new string('X', 105 * 1024 * 1024);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act & Assert
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }
}