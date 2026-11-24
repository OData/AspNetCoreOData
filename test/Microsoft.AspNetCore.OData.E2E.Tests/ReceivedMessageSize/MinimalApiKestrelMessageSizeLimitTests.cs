//-----------------------------------------------------------------------------
// <copyright file="MinimalApiKestrelMessageSizeLimitTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

public class MinimalApiKestrelMessageSizeLimitTests : IAsyncLifetime
{
    private const int Limit = 1024;
    private WebApplication _application;
    private HttpClient _client;
    private long _earlyMiddlewareBytesRead;

    public async Task InitializeAsync()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = null;
            options.Listen(IPAddress.Loopback, 0);
        });
        builder.Services.AddOData(options => options.SetMaxReceivedMessageSize(Limit));
        _application = builder.Build();
        _application.Use(async (context, next) =>
        {
            if (context.Request.Query["stricter"] == "true")
            {
                context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = Limit / 2;
            }

            await next(context);
        });
        _application.UseRouting();
        _application.UseWhen(context => context.Request.Query["limitMiddleware"] == "true", app => app.UseODataMessageSizeLimit());
        _application.Use(async (context, next) =>
        {
            if (long.TryParse(context.Request.Query["responseLength"], out long responseLength))
            {
                context.Response.ContentLength = responseLength;
            }

            if (context.Request.Query["consumeEarly"] == "true")
            {
                context.Request.EnableBuffering();
                using var body = new MemoryStream();
                try
                {
                    await context.Request.Body.CopyToAsync(body);
                    context.Request.Body.Position = 0;
                }
                finally
                {
                    _earlyMiddlewareBytesRead = body.Length;
                }
            }
            else if (context.Request.Query["early"] != "false")
            {
                context.Request.EnableBuffering();
                await context.Request.Body.ReadAsync(new byte[1]);
                context.Request.Body.Position = 0;
            }

            context.Items["originalBody"] = context.Request.Body;
            await next(context);
        });
        _application.MapMethods("odata/stream", ["POST", "PUT", "PATCH"], async (Stream body) =>
        {
            using var destination = new MemoryStream();
            await body.CopyToAsync(destination);
            return Http.Results.Ok(destination.Length);
        }).WithODataResult().WithODataOptions(options => options.SetMaxReceivedMessageSize(Limit));
        _application.MapPost("odata/typed", ([FromBody] MessageSizeItem item) => Http.Results.Ok(item.Payload.Length)).WithODataResult();
        _application.MapPost("odata/reject", async (Stream body) =>
        {
            string message = "application rejection";
            try
            {
                await body.CopyToAsync(Stream.Null);
            }
            catch (BadHttpRequestException exception) when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                message = "handled size rejection";
            }

            return Http.Results.Text(message, statusCode: StatusCodes.Status413PayloadTooLarge);
        }).WithODataResult();
        _application.MapPost("odata/pipe", async (PipeReader body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataResult();
        _application.MapPost("odata/metadata", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithMetadata(new RequestSizeLimitAttribute(Limit * 2)).WithODataResult();
        _application.MapPost("odata/strict-metadata", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataResult().WithMetadata(new RequestSizeLimitAttribute(Limit / 2));
        _application.MapPost("odata/options", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataOptions(options => options.SetMaxReceivedMessageSize(Limit / 2));
        _application.MapGroup("odata/group").WithODataResult().MapPost("higher", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataOptions(options => options.SetMaxReceivedMessageSize(Limit * 2));
        _application.MapPost("odata/state", (HttpContext context) => Http.Results.Ok(new
        {
            limit = context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize,
            wrapped = !ReferenceEquals(context.Items["originalBody"], context.Request.Body)
        })).WithODataResult();
        await _application.StartAsync();
        string address = Assert.Single(_application.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses);
        _client = new HttpClient { BaseAddress = new Uri(address) };
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_application != null)
        {
            await _application.DisposeAsync();
        }
    }

    [Theory]
    [InlineData("stream", false, Limit)]
    [InlineData("stream", true, Limit / 2)]
    [InlineData("metadata", false, Limit)]
    [InlineData("strict-metadata", false, Limit / 2)]
    [InlineData("options", false, Limit / 2)]
    [InlineData("group/higher", false, Limit * 2)]
    public async Task Post_ChunkedOversizedBody_IsRejectedBeforeMiddlewareConsumesPastLimit(string endpoint, bool stricter, int expectedLimit)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/{endpoint}?consumeEarly=true&limitMiddleware=true&stricter={stricter.ToString().ToLowerInvariant()}")
        {
            Content = new UnknownLengthContent(new byte[expectedLimit + 1]),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.True(_earlyMiddlewareBytesRead <= expectedLimit, $"Early middleware consumed {_earlyMiddlewareBytesRead} bytes before the {expectedLimit}-byte limit was enforced.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_ChunkedOversizedBody_WithResponseContentLength_ReturnsComplete413(bool limitMiddleware)
    {
        string middleware = limitMiddleware.ToString().ToLowerInvariant();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/stream?early=false&responseLength=64&consumeEarly={middleware}&limitMiddleware={middleware}")
        {
            Content = new UnknownLengthContent(new byte[Limit + 1]),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_ChunkedOversizedJson_WithResponseContentLength_ReturnsComplete413(bool limitMiddleware)
    {
        string middleware = limitMiddleware.ToString().ToLowerInvariant();
        string json = $"{{\"Id\":1,\"Payload\":\"{new string('x', Limit * 4)}\"}}";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/typed?early=false&responseLength=64&limitMiddleware={middleware}")
        {
            Content = new UnknownLengthContent(Encoding.UTF8.GetBytes(json)),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Post_ApplicationGenerated413_PreservesResponseBody(bool limitMiddleware, bool oversized)
    {
        string middleware = limitMiddleware.ToString().ToLowerInvariant();
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/reject?early=false&responseLength=64&limitMiddleware={middleware}")
        {
            Content = new UnknownLengthContent(new byte[oversized ? Limit * 4 : 16]),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(oversized ? "handled size rejection" : "application rejection", body);
        Assert.Equal(Encoding.UTF8.GetByteCount(body), response.Content.Headers.ContentLength);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Post_ValidChunkedJson_WithOrWithoutResponseContentLength_Succeeds(bool limitMiddleware, bool responseLength)
    {
        string middleware = limitMiddleware.ToString().ToLowerInvariant();
        string json = $"{{\"Id\":1,\"Payload\":\"{new string('x', 16)}\"}}";
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/typed?early=false&responseLength={(responseLength ? "2" : "")}&limitMiddleware={middleware}")
        {
            Content = new UnknownLengthContent(Encoding.UTF8.GetBytes(json)),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("16", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    // Kestrel counts HTTP/1.1 chunk framing toward its quota; chunked positive cases leave room for it.
    [InlineData("stream", Limit - 32, false)]
    [InlineData("metadata", Limit - 32, false)]
    [InlineData("strict-metadata", Limit / 2 - 32, false)]
    [InlineData("options", Limit / 2 - 32, false)]
    [InlineData("group/higher", Limit * 2 - 32, false)]
    [InlineData("stream", Limit, true)]
    [InlineData("metadata", Limit, true)]
    [InlineData("strict-metadata", Limit / 2, true)]
    [InlineData("options", Limit / 2, true)]
    [InlineData("group/higher", Limit * 2, true)]
    public async Task Post_ValidBody_CanBeBufferedByMiddlewareAndReadByEndpoint(string endpoint, int length, bool knownLength)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/{endpoint}?consumeEarly=true&limitMiddleware=true")
        {
            Content = knownLength ? new ByteArrayContent(new byte[length]) : new UnknownLengthContent(new byte[length]),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(length, _earlyMiddlewareBytesRead);
    }

    [Theory]
    [InlineData("stream", "true")]
    [InlineData("pipe", "true")]
    [InlineData("typed", "true")]
    [InlineData("typed", "false")]
    [InlineData("stream", "false")]
    [InlineData("metadata", "true")]
    [InlineData("metadata", "false")]
    public async Task Post_ChunkedOversizedBody_Returns413BeforeBinding(string endpoint, string early)
    {
        byte[] bytes = endpoint == "typed" ? Encoding.UTF8.GetBytes($"{{\"Id\":1,\"Payload\":\"{new string('x', Limit * 4)}\"}}") : new byte[Limit + 1];
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/{endpoint}?early={early}")
        {
            Content = new UnknownLengthContent(bytes),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", 0)]
    [InlineData("POST", Limit - 1)]
    [InlineData("POST", Limit)]
    [InlineData("PUT", Limit)]
    [InlineData("PATCH", Limit)]
    public async Task BodyMethod_ChunkedBodyAtOrUnderLimit_SucceedsAfterEarlierRead(string method, int length)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), "odata/stream")
        {
            Content = new UnknownLengthContent(new byte[length]),
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(length.ToString(), await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(false, Limit)]
    [InlineData(true, Limit / 2)]
    public async Task Post_ReadOnlyServerFeatureWithSufficientLimit_DoesNotAddStreamWrapper(bool stricter, int expectedLimit)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/state?consumeEarly=true&limitMiddleware=true&stricter={stricter.ToString().ToLowerInvariant()}") { Content = new UnknownLengthContent(new byte[expectedLimit - 32]) };

        using HttpResponseMessage response = await _client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"\"limit\":{expectedLimit}", body);
        Assert.Contains("\"wrapped\":false", body);
    }

    [Theory]
    [InlineData(false, Limit)]
    [InlineData(true, Limit / 2)]
    public async Task Post_WritableServerFeature_UsesServerLimitWithoutWrapping(bool stricter, int expectedLimit)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/state?early=false&stricter={stricter.ToString().ToLowerInvariant()}") { Content = new UnknownLengthContent(new byte[0]) };

        using HttpResponseMessage response = await _client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"\"limit\":{expectedLimit}", body);
        Assert.Contains("\"wrapped\":false", body);
    }

    private sealed class UnknownLengthContent : HttpContent
    {
        private readonly byte[] _bytes;

        public UnknownLengthContent(byte[] bytes)
        {
            _bytes = bytes;
            Headers.ContentType = new MediaTypeHeaderValue("application/json");
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) => stream.WriteAsync(_bytes, 0, _bytes.Length);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
