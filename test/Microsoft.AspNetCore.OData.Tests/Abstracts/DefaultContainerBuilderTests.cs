//-----------------------------------------------------------------------------
// <copyright file="DefaultContainerBuilderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Xunit;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    public class DefaultContainerBuilderTests
    {
        [Fact]
        public void AddServiceWithImplementationType_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            DefaultContainerBuilder builder = new DefaultContainerBuilder();
            ExceptionAssert.ThrowsArgumentNull(
                () => builder.AddService(ServiceLifetime.Singleton, serviceType: null, implementationType: null), "serviceType");

            ExceptionAssert.ThrowsArgumentNull(
                () => builder.AddService(ServiceLifetime.Singleton, serviceType: typeof(int), implementationType: null), "implementationType");
        }

        [Fact]
        public void AddServiceWithImplementationFactory_ThrowsArgumentNull_ForInputParameters()
        {
            // Arrange & Act & Assert
            DefaultContainerBuilder builder = new DefaultContainerBuilder();
            ExceptionAssert.ThrowsArgumentNull(
                () => builder.AddService(ServiceLifetime.Singleton, serviceType: null, implementationFactory: null), "serviceType");

            ExceptionAssert.ThrowsArgumentNull(
                () => builder.AddService(ServiceLifetime.Singleton, serviceType: typeof(int), implementationFactory: null), "implementationFactory");
        }

        [Fact]
        public void AddService_WithImplementationType()
        {
            // Arrange
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Transient);

            // Act
            IServiceProvider container = builder.BuildContainer();

            // Assert
            Assert.NotNull(container.GetService<ITestService>());
        }

        [Fact]
        public void AddService_WithImplementationFactory()
        {
            // Arrange
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService>(ServiceLifetime.Transient, sp => new TestService());

            // Act
            IServiceProvider container = builder.BuildContainer();

            // Assert
            Assert.NotNull(container.GetService<ITestService>());
        }

        [Fact]
        public void AddSingletonService_Works()
        {
            // Arrange
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Singleton);
            IServiceProvider container = builder.BuildContainer();

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
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Transient);
            IServiceProvider container = builder.BuildContainer();

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
            IContainerBuilder builder = new DefaultContainerBuilder();
            builder.AddService<ITestService, TestService>(ServiceLifetime.Scoped);
            IServiceProvider container = builder.BuildContainer();

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
}
