//-----------------------------------------------------------------------------
// <copyright file="ODataApplicationBuilderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
