// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddOData_RegistersRequiredServicesIdempotently()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddOData();
            services.AddOData();

            // Assert
            var registerd = services.Where(s => s.ServiceType == typeof(IPerRouteContainer));
            Assert.Single(registerd);
        }

        [Fact]
        public void AddOData_RegistersODataOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            IEdmModel coreModel = EdmCoreModel.Instance;

            // Act
            services.AddOData(opt => opt.AddModel("odata", EdmCoreModel.Instance));
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<ODataOptions> options = provider.GetService<IOptions<ODataOptions>>();
            Assert.NotNull(options);

            ODataOptions odataOptions = options.Value;
            var model = Assert.Single(odataOptions.Models);
            Assert.Equal("odata", model.Key);
            Assert.Same(coreModel, model.Value.Item1);
            Assert.Null(model.Value.Item2);
        }

        [Fact]
        public void AddConvention_RegistersODataRoutingConvention()
        {
            // Arrange
            var services = new ServiceCollection();
            IEdmModel coreModel = EdmCoreModel.Instance;

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
    }

    public class MyConvention : IODataControllerActionConvention
    {
        public int Order => int.MaxValue;

        public bool AppliesToAction(ODataControllerActionContext context) => true;

        public bool AppliesToController(ODataControllerActionContext context) => true;
    }
}
