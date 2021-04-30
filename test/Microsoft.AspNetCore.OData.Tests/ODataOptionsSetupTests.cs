// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataOptionsSetupTests
    {
        [Fact]
        public void ODataOptionsSetup_DoesNotSetup_ODataRoutingConventions()
        {
            // Arrange & Act
            ODataOptions options = GetODataOptions(false);

            // Assert
            Assert.Empty(options.Conventions);
        }

        [Fact]
        public void ODataOptionsSetup_Setup_ODataRoutingConventions()
        {
            // Arrange & Act
            ODataOptions options = GetODataOptions(true);

            // Assert
            Assert.Equal(11, options.Conventions.Count);

            // Test the following
            Assert.Contains(options.Conventions, e => e is AttributeRoutingConvention);
            Assert.Contains(options.Conventions, e => e is EntitySetRoutingConvention);
            Assert.Contains(options.Conventions, e => e is SingletonRoutingConvention);
        }

        private static ODataOptions GetODataOptions(bool withOData)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddControllers();

            services.TryAddSingleton<IODataPathTemplateParser, MytemplateTranslater>();
            services.AddTransient<ILoggerFactory, LoggerFactory>();
            services.Configure<ODataOptions>(opt => { });

            if (withOData)
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());
            }

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;
        }

        private class MytemplateTranslater : IODataPathTemplateParser
        {
            public ODataPathTemplate Parse(IEdmModel model, string odataPath, IServiceProvider requestProvider)
            {
                return new ODataPathTemplate();
            }
        }
    }
}
