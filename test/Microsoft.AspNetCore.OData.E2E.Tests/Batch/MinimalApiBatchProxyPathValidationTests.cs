//-----------------------------------------------------------------------------
// <copyright file="MinimalApiBatchProxyPathValidationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Batch;

public class MinimalApiBatchProxyPathValidationTests
{
    [Fact]
    public async Task MinimalBatchRejectsOutsidePrefixAfterProxyForwardsBatch()
    {
        bool targetReached = false;
        string blockedPath = null;
        string forwardedPath = null;
        string forwardedHost = null;
        CapturingDefaultODataBatchHandler batchHandler = new CapturingDefaultODataBatchHandler();
        IEdmModel model = GetEdmModel();
        WebApplicationBuilder backendBuilder = WebApplication.CreateBuilder();
        backendBuilder.WebHost.UseTestServer();
        backendBuilder.Services.AddOData(options => options.EnableAll());
        await using WebApplication backend = backendBuilder.Build();
        backend.UseODataMiniBatching("mini/$batch", model, batchHandler);
        backend.Use(async (context, next) =>
        {
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path == "/admin/write")
            {
                targetReached = true;
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next();
        });
        await backend.StartAsync();
        using HttpClient backendClient = backend.GetTestClient();

        WebApplicationBuilder proxyBuilder = WebApplication.CreateBuilder();
        proxyBuilder.WebHost.UseTestServer();
        await using WebApplication proxy = proxyBuilder.Build();
        proxy.Run(async context =>
        {
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                blockedPath = context.Request.Path;
                context.Response.Headers["X-Proxy-Decision"] = "blocked-admin-path";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (context.Request.Path != "/mini/$batch" && context.Request.Path != "/odata/$batch")
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            forwardedPath = context.Request.Path;
            forwardedHost = context.Request.Host.Value;
            using HttpRequestMessage backendRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), new Uri($"http://localhost{context.Request.Path}{context.Request.QueryString}"))
            {
                Content = new StreamContent(context.Request.Body)
            };
            backendRequest.Headers.Host = context.Request.Host.Value;
            foreach ((string name, Microsoft.Extensions.Primitives.StringValues values) in context.Request.Headers)
            {
                if (name.Equals("Host", StringComparison.OrdinalIgnoreCase) || name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!backendRequest.Headers.TryAddWithoutValidation(name, values.ToArray()))
                {
                    backendRequest.Content.Headers.TryAddWithoutValidation(name, values.ToArray());
                }
            }

            try
            {
                using HttpResponseMessage backendResponse = await backendClient.SendAsync(backendRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                context.Response.StatusCode = (int)backendResponse.StatusCode;
                await backendResponse.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        });
        await proxy.StartAsync();
        using HttpClient proxyClient = proxy.GetTestClient();

        using HttpResponseMessage directResponse = await proxyClient.PostAsync("/admin/write", new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Forbidden, directResponse.StatusCode);
        Assert.Equal("blocked-admin-path", Assert.Single(directResponse.Headers.GetValues("X-Proxy-Decision")));
        Assert.False(targetReached);
        Assert.Equal("/admin/write", blockedPath);

        const string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""http://localhost/admin/write"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {}
                }
            ]
        }";
        HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "/mini/$batch");
        batchRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        batchRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        using HttpResponseMessage batchResponse = await proxyClient.SendAsync(batchRequest);
        string batchBody = await batchResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, batchResponse.StatusCode);
        Assert.Empty(batchBody);
        Assert.False(targetReached);
        Assert.Equal("/mini/$batch", forwardedPath);
        Assert.Equal("localhost", forwardedHost);
        Assert.Null(batchHandler.PrefixName);
        Assert.Equal("mini", batchHandler.CapturedRoutePrefix);
    }

    [Fact]
    public async Task ControllerBatchRejectsOutsidePrefixAfterProxyForwardsBatch()
    {
        bool targetReached = false;
        string blockedPath = null;
        string forwardedPath = null;
        string forwardedHost = null;
        CapturingDefaultODataBatchHandler batchHandler = new CapturingDefaultODataBatchHandler();
        IEdmModel model = GetEdmModel();
        WebApplicationBuilder backendBuilder = WebApplication.CreateBuilder();
        backendBuilder.WebHost.UseTestServer();
        backendBuilder.Services.AddControllers().AddOData(options => options.AddRouteComponents("odata", model, batchHandler));
        await using WebApplication backend = backendBuilder.Build();
        backend.UseODataBatching();
        backend.Use(async (context, next) =>
        {
            if (HttpMethods.IsPost(context.Request.Method) && context.Request.Path == "/admin/write")
            {
                targetReached = true;
                context.Response.StatusCode = StatusCodes.Status204NoContent;
                return;
            }

            await next();
        });
        backend.UseRouting();
        backend.MapControllers();
        await backend.StartAsync();
        using HttpClient backendClient = backend.GetTestClient();

        WebApplicationBuilder proxyBuilder = WebApplication.CreateBuilder();
        proxyBuilder.WebHost.UseTestServer();
        await using WebApplication proxy = proxyBuilder.Build();
        proxy.Run(async context =>
        {
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                blockedPath = context.Request.Path;
                context.Response.Headers["X-Proxy-Decision"] = "blocked-admin-path";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (context.Request.Path != "/mini/$batch" && context.Request.Path != "/odata/$batch")
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            forwardedPath = context.Request.Path;
            forwardedHost = context.Request.Host.Value;
            using HttpRequestMessage backendRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), new Uri($"http://localhost{context.Request.Path}{context.Request.QueryString}"))
            {
                Content = new StreamContent(context.Request.Body)
            };
            backendRequest.Headers.Host = context.Request.Host.Value;
            foreach ((string name, Microsoft.Extensions.Primitives.StringValues values) in context.Request.Headers)
            {
                if (name.Equals("Host", StringComparison.OrdinalIgnoreCase) || name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!backendRequest.Headers.TryAddWithoutValidation(name, values.ToArray()))
                {
                    backendRequest.Content.Headers.TryAddWithoutValidation(name, values.ToArray());
                }
            }

            try
            {
                using HttpResponseMessage backendResponse = await backendClient.SendAsync(backendRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
                context.Response.StatusCode = (int)backendResponse.StatusCode;
                await backendResponse.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        });
        await proxy.StartAsync();
        using HttpClient proxyClient = proxy.GetTestClient();

        using HttpResponseMessage directResponse = await proxyClient.PostAsync("/admin/write", new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Forbidden, directResponse.StatusCode);
        Assert.Equal("blocked-admin-path", Assert.Single(directResponse.Headers.GetValues("X-Proxy-Decision")));
        Assert.False(targetReached);
        Assert.Equal("/admin/write", blockedPath);

        const string payload = @"{
            ""requests"": [
                {
                    ""id"": ""1"",
                    ""method"": ""POST"",
                    ""url"": ""http://localhost/admin/write"",
                    ""headers"": {
                        ""Content-Type"": ""application/json""
                    },
                    ""body"": {}
                }
            ]
        }";
        HttpRequestMessage batchRequest = new HttpRequestMessage(HttpMethod.Post, "/odata/$batch");
        batchRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
        batchRequest.Content = new StringContent(payload, Encoding.UTF8, "application/json");
        using HttpResponseMessage batchResponse = await proxyClient.SendAsync(batchRequest);
        string batchBody = await batchResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, batchResponse.StatusCode);
        Assert.Empty(batchBody);
        Assert.False(targetReached);
        Assert.Equal("/odata/$batch", forwardedPath);
        Assert.Equal("localhost", forwardedHost);
        Assert.Equal("odata", batchHandler.PrefixName);
        Assert.Equal("odata", batchHandler.CapturedRoutePrefix);
    }

    private static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<ProxyBatchItem>("Entities");
        return modelBuilder.GetEdmModel();
    }

    private sealed class CapturingDefaultODataBatchHandler : DefaultODataBatchHandler
    {
        public string CapturedRoutePrefix { get; private set; }

        public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
        {
            try
            {
                await base.ProcessBatchAsync(context, nextHandler);
            }
            finally
            {
                CapturedRoutePrefix = context.ODataFeature().RoutePrefix;
            }
        }
    }

    private sealed class ProxyBatchItem
    {
        public int Id { get; set; }
    }
}
