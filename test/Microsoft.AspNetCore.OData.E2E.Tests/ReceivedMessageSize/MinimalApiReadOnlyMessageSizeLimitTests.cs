//-----------------------------------------------------------------------------
// <copyright file="MinimalApiReadOnlyMessageSizeLimitTests.cs" company=".NET Foundation">
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ReceivedMessageSize;

public class MinimalApiReadOnlyMessageSizeLimitTests : IClassFixture<MinimalTestFixture<MinimalApiReadOnlyMessageSizeLimitTests>>
{
    private const int Limit = 1024;
    private readonly HttpClient _client;

    public MinimalApiReadOnlyMessageSizeLimitTests(MinimalTestFixture<MinimalApiReadOnlyMessageSizeLimitTests> factory)
    {
        _client = factory.CreateClient();
    }

    protected static void ConfigureServices(IServiceCollection services)
    {
        services.AddOData(options => options.SetMaxReceivedMessageSize(Limit));
    }

    protected static void ConfigureAPIs(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Query["buffer"] != "false")
            {
                context.Request.EnableBuffering();
                await context.Request.Body.ReadAsync(new byte[1]);
                context.Request.Body.Position = 0;
            }

            context.Features.Set<IHttpMaxRequestBodySizeFeature>(context.Request.Query["feature"] == "missing" ? null : new ReadOnlyBodySizeFeature());
            await next(context);
        });

        app.MapMethods("odata/read", ["GET", "POST", "PUT", "PATCH"], async (HttpContext context) =>
        {
            using var body = new MemoryStream();
            await context.Request.Body.CopyToAsync(body);
            return Http.Results.Ok(body.Length);
        }).WithODataResult();

        app.MapPost("odata/typed", ([FromBody] MessageSizeItem item) => Http.Results.Ok(item.Payload.Length)).WithODataResult();
        app.MapPost("odata/stream", async (Stream body) =>
        {
            using var destination = new MemoryStream();
            await body.CopyToAsync(destination);
            return Http.Results.Ok(destination.Length);
        }).WithODataResult();
        app.MapPost("odata/pipe", async (PipeReader body) =>
        {
            using var destination = new MemoryStream();
            await body.CopyToAsync(destination);
            return Http.Results.Ok(destination.Length);
        }).WithODataResult();
        app.MapPost("odata/bodyreader", async (HttpContext context) =>
        {
            await context.Request.BodyReader.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataResult();
        app.MapPost("odata/replay", async (HttpContext context) =>
        {
            await context.Request.Body.CopyToAsync(Stream.Null);
            context.Request.Body.Position = 0;
            await context.Request.Body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataResult();
        app.MapPost("odata/options", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataOptions(options => options.SetMaxReceivedMessageSize(Limit / 2));
        app.MapGroup("odata/group").WithODataResult().MapPost("higher", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithODataResult().WithODataOptions(options => options.SetMaxReceivedMessageSize(Limit * 2));
        app.MapPost("plain", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        });
        app.MapPost("odata/failure", (Func<HttpContext, IResult>)(_ => throw new InvalidOperationException("Handler failed."))).WithODataResult();
        app.MapPost("odata/metadata", async (Stream body) =>
        {
            await body.CopyToAsync(Stream.Null);
            return Http.Results.Ok();
        }).WithMetadata(new AlwaysEqualMetadata()).WithODataResult();
    }

    [Theory]
    [InlineData("read", "readonly", "true")]
    [InlineData("read", "missing", "true")]
    [InlineData("typed", "readonly", "true")]
    [InlineData("typed", "readonly", "false")]
    [InlineData("typed", "missing", "false")]
    [InlineData("stream", "readonly", "true")]
    [InlineData("stream", "missing", "false")]
    [InlineData("pipe", "readonly", "true")]
    [InlineData("pipe", "missing", "false")]
    [InlineData("bodyreader", "readonly", "true")]
    public async Task Post_UnknownLengthOversizedBody_IsRejectedBeforeBinding(string endpoint, string feature, string buffer)
    {
        byte[] bytes = endpoint == "typed" ? Encoding.UTF8.GetBytes($"{{\"Id\":1,\"Payload\":\"{new string('x', Limit * 4)}\"}}") : new byte[Limit * 4];
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/{endpoint}?feature={feature}&buffer={buffer}") { Content = new UnknownLengthContent(bytes) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Theory]
    [InlineData("POST", 0)]
    [InlineData("POST", Limit - 1)]
    [InlineData("POST", Limit)]
    [InlineData("PUT", Limit)]
    [InlineData("PATCH", Limit)]
    public async Task BodyMethod_UnknownLengthBodyAtOrUnderLimit_Succeeds(string method, int length)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), "odata/read") { Content = new UnknownLengthContent(new byte[length]) };

        using HttpResponseMessage response = await _client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(length.ToString(), body);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task BodyMethod_UnknownLengthBodyOneByteOverLimit_Returns413(string method)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), "odata/read") { Content = new UnknownLengthContent(new byte[Limit + 1]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Theory]
    [InlineData(Limit * 3 / 4)]
    [InlineData(Limit)]
    public async Task Post_BufferedBodyCanBeReadTwice_WithoutCountingBytesTwice(int length)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "odata/replay") { Content = new UnknownLengthContent(new byte[length]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("typed")]
    [InlineData("stream")]
    [InlineData("pipe")]
    public async Task Post_BoundBodyExactlyAtLimit_Succeeds(string endpoint)
    {
        byte[] bytes = endpoint == "typed" ? Encoding.UTF8.GetBytes($"{{\"Id\":1,\"Payload\":\"{new string('x', Limit - 21)}\"}}") : new byte[Limit];
        Assert.Equal(Limit, bytes.Length);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"odata/{endpoint}") { Content = new UnknownLengthContent(bytes) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_KnownLengthOversizedBody_Returns413()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "odata/stream") { Content = new ByteArrayContent(new byte[Limit + 1]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Post_WithODataOptionsOnly_EnforcesLowerEndpointLimit()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "odata/options") { Content = new UnknownLengthContent(new byte[Limit / 2 + 1]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Post_GroupAndEndpointOptions_AllowHigherEndpointLimit()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "odata/group/higher") { Content = new UnknownLengthContent(new byte[Limit + 1]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "odata/read")]
    [InlineData("POST", "plain")]
    public async Task Request_OutsideODataBodyLimitScope_IsUnchanged(string method, string endpoint)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), endpoint) { Content = new UnknownLengthContent(new byte[Limit + 1]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_UnrelatedHandlerException_IsNotConvertedTo413()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "odata/failure") { Content = new UnknownLengthContent(new byte[1]) };

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _client.SendAsync(request));

        Assert.Equal("Handler failed.", exception.Message);
    }

    [Fact]
    public async Task Post_UnrelatedMetadataEqualityCannotDisableLimit()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "odata/metadata") { Content = new UnknownLengthContent(new byte[Limit + 1]) };

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    private sealed class AlwaysEqualMetadata
    {
        public override bool Equals(object obj) => true;
        public override int GetHashCode() => 0;
    }

    private sealed class ReadOnlyBodySizeFeature : IHttpMaxRequestBodySizeFeature
    {
        public bool IsReadOnly => true;
        public long? MaxRequestBodySize { get => null; set => throw new InvalidOperationException(); }
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
