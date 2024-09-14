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

    private interface ITestService { }

    private class TestService : ITestService { }
}
