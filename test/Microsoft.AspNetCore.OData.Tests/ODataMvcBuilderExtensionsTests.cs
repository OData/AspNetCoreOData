// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataMvcBuilderExtensionsTests
    {
        [Fact]
        public void AddOData_ThrowsArgumentNull_Builder()
        {
            // Arrange
            IMvcBuilder builder = null;
            Action<ODataOptions> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "builder");
        }

        [Fact]
        public void AddOData_ThrowsArgumentNull_SetupAction()
        {
            // Arrange
            IMvcBuilder builder = new Mock<IMvcBuilder>().Object;
            Action<ODataOptions> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "setupAction");
        }

        [Fact]
        public void AddOData_ForServiceProvider_ThrowsArgumentNull_Builder()
        {
            // Arrange
            IMvcBuilder builder = null;
            Action<ODataOptions, IServiceProvider> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "builder");
        }

        [Fact]
        public void AddOData_ForServiceProvider_ThrowsArgumentNull_SetupAction()
        {
            // Arrange
            IMvcBuilder builder = new Mock<IMvcBuilder>().Object;
            Action<ODataOptions, IServiceProvider> setupAction = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => builder.AddOData(setupAction), "setupAction");
        }

        [Fact]
        public void AddOData_RegistersRequiredServicesIdempotently()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddControllers().AddOData().AddOData();

            // Assert
            var registerd = services.Where(s => s.ServiceType == typeof(IAssemblyResolver));
            Assert.Single(registerd);
        }

        [Fact]
        public void AddOData_RegistersODataOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddControllers().AddOData();
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            Assert.Empty(odataOptions.RouteComponents);
        }

        [Fact]
        public void AddODataWithSetup_RegistersODataOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            IEdmModel coreModel = EdmCoreModel.Instance;

            // Act
            services.AddControllers().AddOData(opt => opt.AddModel("odata", EdmCoreModel.Instance));
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            var model = Assert.Single(odataOptions.RouteComponents);
            Assert.Equal("odata", model.Key);
            Assert.Same(coreModel, model.Value.EdmModel);
            Assert.NotNull(model.Value.ServiceProvider);
        }

        [Fact]
        public void AddODataWithSetup_RegistersODataOptionsWithServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            IEdmModel coreModel = EdmCoreModel.Instance;

            services.AddSingleton(_ => coreModel);

            // Act
            services.AddControllers().AddOData((options, serviceProvider) =>
            {
                var edmModel = serviceProvider.GetRequiredService<IEdmModel>();
                options.AddModel("odata", edmModel);
            });

            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            var model = Assert.Single(odataOptions.RouteComponents);
            Assert.Equal("odata", model.Key);
            Assert.Same(coreModel, model.Value.EdmModel);
            Assert.NotNull(model.Value.ServiceProvider);
        }

        /*
        [Fact]
        public void AddConvention_RegistersODataRoutingConvention()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLogging();
            services.AddOData().AddConvention<MyConvention>();
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IEnumerable<IODataControllerActionConvention> conventions = provider.GetServices<IODataControllerActionConvention>();
            Assert.NotNull(conventions);

            var registeredConventions = conventions.Where(c => c.Order == int.MaxValue);
            var registeredConvention = Assert.Single(registeredConventions);

            Assert.IsType<MyConvention>(registeredConvention);
        }

        [Fact]
        public void ReplaceConventions_ReplaceBuiltInRoutingConvention()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddLogging();
            services.AddOData().ReplaceConventions(typeof(MyConvention), typeof(MetadataRoutingConvention));

            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IODataControllerActionConvention[] conventions = provider.GetServices<IODataControllerActionConvention>().ToArray();
            Assert.NotNull(conventions);

            Assert.Equal(2, conventions.Length);
            Assert.IsType<MyConvention>(conventions[0]);
            Assert.IsType<MetadataRoutingConvention>(conventions[1]);
        }
        */
    }

    public class MyConvention : IODataControllerActionConvention
    {
        public int Order => int.MaxValue;

        public bool AppliesToAction(ODataControllerActionContext context) => true;

        public bool AppliesToController(ODataControllerActionContext context) => true;
    }

    public class MyMvcBuilder : IMvcBuilder
    {
        public ApplicationPartManager PartManager { get; set; }

        public IServiceCollection Services { get; set; }
    }
}
