//-----------------------------------------------------------------------------
// <copyright file="ODataApplicationBuilderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests;

public class ODataApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseODataMessageSizeLimit_ThrowsArgumentNull_AppBuilder()
    {
        IApplicationBuilder builder = null;
        ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataMessageSizeLimit(), "app");
    }

    [Theory]
    [InlineData(false, null, 8)]
    [InlineData(true, null, 8)]
    [InlineData(false, 4, 4)]
    [InlineData(true, 4, 4)]
    [InlineData(false, 0, 0)]
    [InlineData(true, 0, 0)]
    [InlineData(false, 16, 8)]
    public async Task UseODataMessageSizeLimit_ProtectsMiddlewareWhenServerEnforcementIsUnavailable(bool readOnlyFeature, int? endpointLimit, int expectedLimit)
    {
        using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        ApplicationBuilder builder = new ApplicationBuilder(services);
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = "POST";
        using var requestBody = new MemoryStream(new byte[expectedLimit + 1]);
        context.Request.Body = requestBody;
        if (readOnlyFeature)
        {
            Mock<IHttpMaxRequestBodySizeFeature> feature = new Mock<IHttpMaxRequestBodySizeFeature>();
            feature.SetupGet(value => value.IsReadOnly).Returns(true);
            feature.SetupGet(value => value.MaxRequestBodySize).Returns((long?)null);
            context.Features.Set(feature.Object);
        }

        ODataMiniMetadata metadata = new ODataMiniMetadata();
        metadata.Options.SetMaxReceivedMessageSize(8);
        EndpointMetadataCollection endpointMetadata = endpointLimit.HasValue ? new EndpointMetadataCollection(metadata, new RequestSizeLimitAttribute(endpointLimit.Value)) : new EndpointMetadataCollection(metadata);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, endpointMetadata, "odata"));
        using var consumedBody = new MemoryStream();
        builder.UseODataMessageSizeLimit();
        builder.UseODataMessageSizeLimit();
        builder.Run(httpContext => httpContext.Request.Body.CopyToAsync(consumedBody));

        await builder.Build()(context);

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, context.Response.StatusCode);
        Assert.True(consumedBody.Length <= expectedLimit);
    }

    [Theory]
    [InlineData("GET", true)]
    [InlineData("POST", false)]
    public async Task UseODataMessageSizeLimit_LeavesRequestsOutsideItsScopeUnchanged(string method, bool odataEndpoint)
    {
        using ServiceProvider services = new ServiceCollection().BuildServiceProvider();
        ApplicationBuilder builder = new ApplicationBuilder(services);
        DefaultHttpContext context = new DefaultHttpContext();
        context.Request.Method = method;
        using var requestBody = new MemoryStream(new byte[9]);
        context.Request.Body = requestBody;
        if (odataEndpoint)
        {
            ODataMiniMetadata metadata = new ODataMiniMetadata();
            metadata.Options.SetMaxReceivedMessageSize(8);
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "odata"));
        }

        using var consumedBody = new MemoryStream();
        builder.UseODataMessageSizeLimit();
        builder.Run(httpContext => httpContext.Request.Body.CopyToAsync(consumedBody));

        await builder.Build()(context);

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(9, consumedBody.Length);
        Assert.Same(requestBody, context.Request.Body);
    }

    [Fact]
    public void UseODataBatching_ThrowsArgumentNull_AppBuilder()
    {
        // Arrange & Act & Assert
        IApplicationBuilder builder = null;
        ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataBatching(), "app");
    }

    [Fact]
    public void UseODataQueryRequest_ThrowsArgumentNull_AppBuilder()
    {
        // Arrange & Act & Assert
        IApplicationBuilder builder = null;
        ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataQueryRequest(), "app");
    }

    [Theory]
    [InlineData("/$query", true)]
    [InlineData("/$anyother", false)]
    public async Task UseODataQueryRequest_Calls_ODataQueryMiddleware(string requestPath, bool parserCalledExpected)
    {
        // Arrange
        bool parserCalled = false;
        Mock<IODataQueryRequestParser> parser = new Mock<IODataQueryRequestParser>();
        parser.Setup(p => p.CanParse(It.IsAny<HttpRequest>())).Returns(() =>
        {
            parserCalled = true;
            return false; // return false to skip the transform
        });

        IServiceCollection services = new ServiceCollection();
        services.TryAddEnumerable(ServiceDescriptor.Singleton(parser.Object));

        IServiceProvider sp = services.BuildServiceProvider();
        ApplicationBuilder builder = new ApplicationBuilder(serviceProvider: sp);

        DefaultHttpContext context = new DefaultHttpContext();
        context.RequestServices = sp;
        context.Request.Path = new PathString(requestPath);
        context.Request.Method = "Post";

        bool lastCalled = false;

        // Act
        builder.UseODataQueryRequest(); // first - middleware
        builder.Run(context =>  // second - middleware
        {
            lastCalled = true;
            return Task.CompletedTask;
        });

        await builder.Build().Invoke(context);

        // Assert
        Assert.Equal(parserCalledExpected, parserCalled);
        Assert.True(lastCalled);
    }

    [Fact]
    public void UseODataRouteDebug_ThrowsArgumentNull_AppBuilder()
    {
        // Arrange & Act & Assert
        IApplicationBuilder builder = null;
        ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataRouteDebug(), "app");
    }

    [Fact]
    public void UseODataRouteDebug_UsingPattern_ThrowsArgumentNull_AppBuilder()
    {
        // Arrange & Act & Assert
        IApplicationBuilder builder = null;
        ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataRouteDebug("$odata"), "app");
    }

    [Fact]
    public void UseODataRouteDebug_UsingPattern_ThrowsArgumentNull_RoutePattern()
    {
        // Arrange & Act & Assert
        IApplicationBuilder builder = new Mock<IApplicationBuilder>().Object;
        ExceptionAssert.ThrowsArgumentNull(() => builder.UseODataRouteDebug(null), "routePattern");
    }

    [Theory]
    [InlineData("/$odata2", true)]
    [InlineData("/$odata", false)]
    public async Task UseODataRouteDebug_Calls_ODataRouteDebugMiddleware(string requestPath, bool lastCalledExpected)
    {
        await VerifyRouteDebug(requestPath, lastCalledExpected, app => app.UseODataRouteDebug());
    }

    [Theory]
    [InlineData("/$odata2", false)]
    [InlineData("/$odata", true)]
    public async Task UseODataRouteDebug_UsingPattern_Calls_ODataRouteDebugMiddleware(string requestPath, bool lastCalledExpected)
    {
        await VerifyRouteDebug(requestPath, lastCalledExpected, app => app.UseODataRouteDebug("$odata2"));
    }

    private static async Task VerifyRouteDebug(string requestPath, bool lastCalledExpected, Action<ApplicationBuilder> config)
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<EndpointDataSource>(new DefaultEndpointDataSource(Array.Empty<Endpoint>())));
        IServiceProvider sp = services.BuildServiceProvider();
        ApplicationBuilder builder = new ApplicationBuilder(serviceProvider: sp);

        DefaultHttpContext context = new DefaultHttpContext();
        context.RequestServices = sp;
        context.Request.Path = new PathString(requestPath);

        bool lastCalled = false;

        // Act
        config(builder);// first - middleware

        builder.Run(context =>  // second - middleware
        {
            lastCalled = true;
            return Task.CompletedTask;
        });

        await builder.Build().Invoke(context);

        // Assert
        Assert.Equal(lastCalledExpected, lastCalled);
    }
}
