//-----------------------------------------------------------------------------
// <copyright file="DefaultContainerBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts;

public class DefaultContainerBuilderTests
{
    [Fact]
    public void AddServiceWithImplementationType_ThrowsArgumentNull_ForInputParameters()
    {
        // Arrange & Act & Assert
        IServiceCollection services = new ServiceCollection();
        ExceptionAssert.ThrowsArgumentNull(
            () => services.AddSingleton(serviceType: null, implementationType: null), "serviceType");

        ExceptionAssert.ThrowsArgumentNull(
            () => services.AddSingleton(serviceType: typeof(int), implementationType: null), "implementationType");
    }

    [Fact]
    public void AddServiceWithImplementationFactory_ThrowsArgumentNull_ForInputParameters()
    {
        // Arrange & Act & Assert
        IServiceCollection services = new ServiceCollection();
        ExceptionAssert.ThrowsArgumentNull(
            () => services.AddSingleton(serviceType: null, implementationFactory: null), "serviceType");

        ExceptionAssert.ThrowsArgumentNull(
            () => services.AddSingleton(serviceType: typeof(int), implementationFactory: null), "implementationFactory");
    }

    [Fact]
    public void AddService_WithImplementationType()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();

        // Act
        IServiceProvider container = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact]
    public void AddService_WithImplementationFactory()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<ITestService>(sp => new TestService());

        // Act
        IServiceProvider container = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(container.GetService<ITestService>());
    }

    [Fact]
    public void AddSingletonService_Works()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        IServiceProvider container = services.BuildServiceProvider();

        // Act
        ITestService o1 = container.GetService<ITestService>();
        ITestService o2 = container.GetService<ITestService>();

        // Assert
        Assert.NotNull(o1);
        Assert.Equal(o1, o2);
    }

    [Fact]
    public void AddTransientService_Works()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddTransient<ITestService, TestService>();
        IServiceProvider container = services.BuildServiceProvider();

        // Act
        ITestService o1 = container.GetService<ITestService>();
        ITestService o2 = container.GetService<ITestService>();

        // Assert
        Assert.NotNull(o1);
        Assert.NotNull(o2);
        Assert.NotEqual(o1, o2);
    }

    [Fact]
    public void AddScopedService_Works()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddScoped<ITestService, TestService>();
        IServiceProvider container = services.BuildServiceProvider();

        // Act
        IServiceProvider scopedContainer1 = container.GetRequiredService<IServiceScopeFactory>()
            .CreateScope().ServiceProvider;
        ITestService o11 = scopedContainer1.GetService<ITestService>();
        ITestService o12 = scopedContainer1.GetService<ITestService>();

        // Assert
        Assert.NotNull(o11);
        Assert.NotNull(o12);
        Assert.Equal(o11, o12);

        IServiceProvider scopedContainer2 = container.GetRequiredService<IServiceScopeFactory>()
            .CreateScope().ServiceProvider;
        ITestService o21 = scopedContainer2.GetService<ITestService>();
        ITestService o22 = scopedContainer2.GetService<ITestService>();

        Assert.NotNull(o21);
        Assert.NotNull(o22);
        Assert.Equal(o21, o22);

        Assert.NotEqual(o11, o21);
    }

    [Fact]
    public void MessageReaderIsScoped()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddDefaultWebApiServices();
        IServiceProvider container = services.BuildServiceProvider();

        // Act
        IServiceProvider scopedContainer1 = container.GetRequiredService<IServiceScopeFactory>()
            .CreateScope().ServiceProvider;
        ODataMessageReaderSettings reader11 = scopedContainer1.GetService<ODataMessageReaderSettings>();
        ODataMessageReaderSettings reader12 = scopedContainer1.GetService<ODataMessageReaderSettings>();

        // Assert
        Assert.NotNull(reader11);
        Assert.NotNull(reader12);
        Assert.Equal(reader11, reader12);

        IServiceProvider scopedContainer2 = container.GetRequiredService<IServiceScopeFactory>()
            .CreateScope().ServiceProvider;
        ODataMessageReaderSettings reader21 = scopedContainer2.GetService<ODataMessageReaderSettings>();
        ODataMessageReaderSettings reader22 = scopedContainer2.GetService<ODataMessageReaderSettings>();

        Assert.NotNull(reader21);
        Assert.NotNull(reader22);
        Assert.Equal(reader21, reader22);

        Assert.NotEqual(reader11, reader21);
    }


    [Fact]
    public void MessageWriterIsScoped()
    {
        // Arrange
        IServiceCollection services = new ServiceCollection();
        services.AddDefaultWebApiServices();
        IServiceProvider container = services.BuildServiceProvider();

        // Act
        IServiceProvider scopedContainer1 = container.GetRequiredService<IServiceScopeFactory>()
            .CreateScope().ServiceProvider;
        ODataMessageWriterSettings writer11 = scopedContainer1.GetService<ODataMessageWriterSettings>();
        ODataMessageWriterSettings writer12 = scopedContainer1.GetService<ODataMessageWriterSettings>();

        // Assert
        Assert.NotNull(writer11);
        Assert.NotNull(writer12);
        Assert.Equal(writer11, writer12);

        IServiceProvider scopedContainer2 = container.GetRequiredService<IServiceScopeFactory>()
            .CreateScope().ServiceProvider;
        ODataMessageWriterSettings writer21 = scopedContainer2.GetService<ODataMessageWriterSettings>();
        ODataMessageWriterSettings writer22 = scopedContainer2.GetService<ODataMessageWriterSettings>();

        Assert.NotNull(writer21);
        Assert.NotNull(writer22);
        Assert.Equal(writer21, writer22);

        Assert.NotEqual(writer11, writer21);
    }

    private interface ITestService { }

    private class TestService : ITestService { }
}
