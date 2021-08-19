//-----------------------------------------------------------------------------
// <copyright file="ODataMvcCoreBuilderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataMvcCoreBuilderExtensionsTests
    {
        [Fact]
        public void AddOData_Throws_NullBuilder()
        {
            // Arrange
            IMvcCoreBuilder builder = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(), "builder");
        }

        [Fact]
        public void AddOData_Throws_NullSetupAction()
        {
            // Arrange
            IMvcCoreBuilder builder = new MyMvcCoreBuilder();
            Action<ODataOptions> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "setupAction");
        }

        [Fact]
        public void AddOData_Throws_NullBuilderWithServiceProvider()
        {
            // Arrange
            IMvcCoreBuilder builder = null;
            Action<ODataOptions, IServiceProvider> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "builder");
        }

        [Fact]
        public void AddOData_Throws_NullSetupActionWithServiceProvider()
        {
            // Arrange
            IMvcCoreBuilder builder = new MyMvcCoreBuilder();
            Action<ODataOptions, IServiceProvider> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "setupAction");
        }

        [Fact]
        public void AddOData_OnMvcCoreBuilder_RegistersODataOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            IMvcCoreBuilder builder = new MyMvcCoreBuilder
            {
                Services = services
            };

            // Act
            builder.AddOData();
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            Assert.Empty(odataOptions.RouteComponents);
        }

        [Fact]
        public void AddODataWithSetup_OnMvcCoreBuilder_RegistersODataOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            IMvcCoreBuilder builder = new MyMvcCoreBuilder
            {
                Services = services
            };

            IEdmModel coreModel = EdmCoreModel.Instance;

            // Act
            builder.AddOData(opt => opt.AddRouteComponents("odata", EdmCoreModel.Instance));
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            var model = Assert.Single(odataOptions.RouteComponents);
            Assert.Equal("odata", model.Key);
            Assert.Same(coreModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
        }

        [Fact]
        public void AddODataWithSetup_RegistersODataOptionsWithServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            IEdmModel coreModel = EdmCoreModel.Instance;

            services.AddSingleton(_ => coreModel);
            IMvcCoreBuilder builder = new MyMvcCoreBuilder
            {
                Services = services
            };

            // Act
            builder.AddOData((options, serviceProvider) =>
            {
                var edmModel = serviceProvider.GetRequiredService<IEdmModel>();
                options.AddRouteComponents("odata", edmModel);
            });

            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            var model = Assert.Single(odataOptions.RouteComponents);
            Assert.Equal("odata", model.Key);
            Assert.Same(coreModel, model.Value.Item1);
            Assert.NotNull(model.Value.Item2);
        }
    }

    internal class MyMvcCoreBuilder : IMvcCoreBuilder
    {
        /// <inheritdoc />
        public ApplicationPartManager PartManager { get; set; }

        /// <inheritdoc />
        public IServiceCollection Services { get; set; }
    }
}
