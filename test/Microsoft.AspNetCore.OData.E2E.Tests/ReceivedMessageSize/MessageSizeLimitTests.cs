//-----------------------------------------------------------------------------
// <copyright file="MessageSizeLimitTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

/// <summary>
/// Verifies the default MaxReceivedMessageSize behavior (100 MB) when no custom value is configured.
/// </summary>
public class MessageSizeLimitDefaultTests : WebApiTestBase<MessageSizeLimitDefaultTests>
{
    public MessageSizeLimitDefaultTests(WebApiTestFixture<MessageSizeLimitDefaultTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(MessageSizeItemsController));

        IEdmModel edmModel = GetEdmModel();
        services.AddControllers().AddOData(opt =>
        {
            opt.AddRouteComponents("odata", edmModel);
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
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
        string payload = new string('X', 1024);
        string json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode,
            $"Expected success but got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    [Trait("Category", "LargePayload")]
    public async Task Post_PayloadExceedingDefaultLimit_Fails()
    {
        // Arrange — 105 MB exceeds the 100 MB default limit.
        // This allocation is large; if it causes OOM the configured-limit tests
        // cover the same logic with a 1 KB limit.
        var content = new StringContent($"{{\"Id\":1,\"Payload\":\"{new string('X', 105 * 1024 * 1024)}\"}}", Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert — the exact status code depends on how ODL surfaces the quota exception.
        Assert.False(response.IsSuccessStatusCode,
            $"Expected a non-success status code for a payload exceeding the default message size limit. Got {response.StatusCode}.");
    }
}

/// <summary>
/// Verifies that MaxReceivedMessageSize is configurable by setting it to 1 KB
/// and confirming enforcement at that boundary.
/// </summary>
public class MessageSizeLimitConfiguredTests : WebApiTestBase<MessageSizeLimitConfiguredTests>
{
    // A deliberately small limit to make testing fast and deterministic.
    private const int ConfiguredMaxMessageSize = 1024; // 1 KB

    public MessageSizeLimitConfiguredTests(WebApiTestFixture<MessageSizeLimitConfiguredTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(MessageSizeItemsController));

        IEdmModel edmModel = GetEdmModel();
        services.AddControllers().AddOData(opt =>
        {
            opt.MaxReceivedMessageSize = ConfiguredMaxMessageSize;
            opt.AddRouteComponents("odata", edmModel);
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Post_PayloadUnderConfiguredLimit_Succeeds()
    {
        // Arrange — ~500 bytes, well under the 1 KB limit.
        string payload = new string('X', 500);
        string json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode,
            $"Expected success but got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task Post_PayloadExceedingConfiguredLimit_IsRejected()
    {
        // Arrange — ~2 KB exceeds the 1 KB configured limit.
        string payload = new string('X', 2048);
        string json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.False(response.IsSuccessStatusCode,
            $"Expected a non-success status code for a payload exceeding the configured message size limit. Got {response.StatusCode}.");
    }

    [Fact]
    public async Task Put_PayloadExceedingConfiguredLimit_IsRejected()
    {
        // Arrange — ~2 KB exceeds the 1 KB configured limit.
        string payload = new string('X', 2048);
        string json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Put, "odata/MessageSizeItems(1)")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.False(response.IsSuccessStatusCode,
            $"Expected a non-success status code for a PUT payload exceeding the configured message size limit. Got {response.StatusCode}.");
    }
}

/// <summary>
/// Verifies the opt-out path: users can raise MaxReceivedMessageSize above the default
/// to allow larger payloads while still having enforcement at the configured boundary.
/// </summary>
public class MessageSizeLimitOptOutTests : WebApiTestBase<MessageSizeLimitOptOutTests>
{
    // Raised limit to prove opt-out works.
    private const long RaisedMaxMessageSize = 5 * 1024 * 1024; // 5 MB

    public MessageSizeLimitOptOutTests(WebApiTestFixture<MessageSizeLimitOptOutTests> fixture)
        : base(fixture)
    {
    }

    protected static void UpdateConfigureServices(IServiceCollection services)
    {
        services.ConfigureControllers(typeof(MessageSizeItemsController));

        IEdmModel edmModel = GetEdmModel();
        services.AddControllers().AddOData(opt =>
        {
            opt.MaxReceivedMessageSize = RaisedMaxMessageSize;
            opt.AddRouteComponents("odata", edmModel);
        });
    }

    protected static void UpdateConfigure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }

    private static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<MessageSizeItem>("MessageSizeItems");
        return builder.GetEdmModel();
    }

    [Fact]
    public async Task Post_PayloadUnderRaisedLimit_Succeeds()
    {
        // Arrange — 3 MB is under the raised 5 MB limit.
        // This proves users can opt out of the lower default by configuring a higher value.
        string payload = new string('X', 3 * 1024 * 1024);
        string json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode,
            $"Expected success but got {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task Post_PayloadExceedingRaisedLimit_IsRejected()
    {
        // Arrange — 6 MB exceeds the raised 5 MB limit.
        // This proves the raised limit IS still enforced (not bypassed).
        string payload = new string('X', 6 * 1024 * 1024);
        string json = $"{{\"Id\":1,\"Payload\":\"{payload}\"}}";
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        HttpClient client = CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "odata/MessageSizeItems")
        {
            Content = content
        };

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.False(response.IsSuccessStatusCode,
            $"Expected a non-success status code for a payload exceeding the raised message size limit. Got {response.StatusCode}.");
    }
}
