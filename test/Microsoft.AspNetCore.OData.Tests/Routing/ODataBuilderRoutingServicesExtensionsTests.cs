// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing
{
    public class ODataBuilderRoutingServicesExtensionsTests
    {
        [Fact]
        public void AddODataRouting_AddODataRoutingServices()
        {
            // Assert
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            IODataBuilder builder = new DefaultODataBuilder(services);

            // Act
            builder.AddODataRouting();

            // Assert
            IServiceProvider provider = builder.Services.BuildServiceProvider();
            Assert.NotNull(provider);
            var conventions = provider.GetServices<IODataControllerActionConvention>();
            Assert.Equal(11, conventions.Count());
        }
    }
}
