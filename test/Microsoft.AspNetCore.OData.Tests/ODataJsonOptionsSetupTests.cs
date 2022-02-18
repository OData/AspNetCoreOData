//-----------------------------------------------------------------------------
// <copyright file="ODataJsonOptionsSetupTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataJsonOptionsSetupTests
    {
        [Fact]
        public void ConfigureODataJsonOptionsSetup_ThrowsArgumentNull_Options()
        {
            // Arrange
            ODataJsonOptionsSetup setup = new ODataJsonOptionsSetup();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => setup.Configure(null), "options");
        }

        [Fact]
        public void ODataJsonOptionsSetup_DoesNotSetup_ODataJsonConverters()
        {
            // Arrange & Act
            JsonOptions options = GetJsonOptions(false);

            // Assert
            Assert.Empty(options.JsonSerializerOptions.Converters);
        }

        [Fact]
        public void ODataJsonOptionsSetup_Setup_ODataJsonConverters()
        {
            // Arrange & Act
            JsonOptions options = GetJsonOptions(true);

            // Assert
            Assert.Equal(4, options.JsonSerializerOptions.Converters.Count);

            Assert.Collection(options.JsonSerializerOptions.Converters,
                e => Assert.IsType<SelectExpandWrapperConverter>(e),
                e => Assert.IsType<PageResultValueConverter>(e),
                e => Assert.IsType<DynamicTypeWrapperConverter>(e),
                e => Assert.IsType<SingleResultValueConverter>(e));
        }

        private static JsonOptions GetJsonOptions(bool withOData)
        {
            IServiceCollection services = new ServiceCollection();

            services.AddControllers();

            if (withOData)
            {
                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<JsonOptions>, ODataJsonOptionsSetup>());
            }

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IOptions<JsonOptions>>().Value;
        }
    }
}
