//-----------------------------------------------------------------------------
// <copyright file="MinimalApiPutPatchMessageSizeLimitTests.cs" company=".NET Foundation">
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
/// Verifies that <c>SetMaxReceivedMessageSize(...)</c> enforces the message size limit
/// on PUT, PATCH requests and does not block GET requests in a minimal API host.
/// </summary>
public class MinimalApiPutPatchMessageSizeLimitTests : IClassFixture<MinimalTestFixture<MinimalApiPutPatchMessageSizeLimitTests>>
{
    private const int ConfiguredMaxMessageSize = 1 * 1024 * 1024; // 1 MB

    private HttpClient _client;

    public MinimalApiPutPatchMessageSizeLimitTests(MinimalTestFixture<MinimalApiPutPatchMessageSizeLimitTests> factory)
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

        app.MapPut("odata/MessageSizeItems/{id}", ([FromBody] MessageSizeItem item, int id) =>
        {
            item.Id = id;
            return Http.Results.Ok(item);
        })
        .WithODataResult()
        .WithODataModel(model)
        .WithODataOptions(opt => opt.SetMaxReceivedMessageSize(ConfiguredMaxMessageSize));

        app.MapPatch("odata/MessageSizeItems/{id}", ([FromBody] MessageSizeItem item, int id) =>
        {
            item.Id = id;
            return Http.Results.Ok(item);
        })
        .WithODataResult()
        .WithODataModel(model)
        .WithODataOptions(opt => opt.SetMaxReceivedMessageSize(ConfiguredMaxMessageSize));

        app.MapGet("odata/MessageSizeItems", () => Http.Results.Ok(new[] { new MessageSizeItem { Id = 1, Payload = "test" } }))
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
    public async Task Put_SmallPayload_Succeeds()
    {
        // Arrange — a tiny payload well under 1 KB.
        string json = "{\"Id\":1,\"Payload\":\"A\"}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, "odata/MessageSizeItems/1")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Put_PayloadExceedingConfiguredLimit_ThrowsException()
    {
        // Arrange — 5 MB exceeds the 1 MB configured limit.
        var payload = new string('X', 5 * 1024 * 1024);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Put, "odata/MessageSizeItems/1")
        {
            Content = content
        };

        // Act & Assert — middleware throws ODataException with the same message as OData.NET's stream-level enforcement.
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }

    [Fact]
    public async Task Patch_SmallPayload_Succeeds()
    {
        // Arrange — a tiny payload well under 1 KB.
        string json = "{\"Id\":1,\"Payload\":\"A\"}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/MessageSizeItems/1")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Patch_PayloadExceedingConfiguredLimit_ThrowsException()
    {
        // Arrange — 5 MB exceeds the 1 MB configured limit.
        var payload = new string('X', 5 * 1024 * 1024);
        var json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Patch, "odata/MessageSizeItems/1")
        {
            Content = content
        };

        // Act & Assert — middleware throws ODataException with the same message as OData.NET's stream-level enforcement.
        var exception = await Record.ExceptionAsync(async () => await _client.SendAsync(request));
        Assert.NotNull(exception);
        Assert.Contains("maximum number of bytes allowed to be read from the stream has been exceeded", exception.ToString());
    }

    [Fact]
    public async Task Get_Request_IsNotBlockedByMessageSizeMiddleware()
    {
        // Arrange — GET requests are exempt from message size enforcement.
        var request = new HttpRequestMessage(HttpMethod.Get, "odata/MessageSizeItems");

        // Act
        HttpResponseMessage response = await _client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }
}
