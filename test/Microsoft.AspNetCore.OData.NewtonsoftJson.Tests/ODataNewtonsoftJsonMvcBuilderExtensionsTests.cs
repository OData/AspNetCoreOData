//-----------------------------------------------------------------------------
// <copyright file="ODataNewtonsoftJsonMvcBuilderExtensionsTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson.Tests
{
    public class ODataNewtonsoftJsonMvcBuilderExtensionsTests
    {
        [Fact]
        public void AddODataNewtonsoftJson_OnIMvcBuilder_Throws_NullBuilder()
        {
            // Arrange
            IMvcBuilder builder = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("builder", () => builder.AddODataNewtonsoftJson());
        }

        [Fact]
        public void AddODataNewtonsoftJson_OnIMvcCoreBuilder_Throws_NullBuilder()
        {
            // Arrange
            IMvcCoreBuilder builder = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>("builder", () => builder.AddODataNewtonsoftJson());
        }

        [Fact]
        public void AddODataNewtonsoftJson_OnMvcCoreBuilder_RegistersODataJsonConverts()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            IMvcCoreBuilder builder = new MyMvcCoreBuilder
            {
                Services = services
            };

            // Act
            builder.AddODataNewtonsoftJson();
            IServiceProvider provider = services.BuildServiceProvider();

            // Assert
            IOptions<MvcNewtonsoftJsonOptions> options = provider.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
            Assert.NotNull(options);

            MvcNewtonsoftJsonOptions jsonOptions = options.Value;
            Assert.Contains(jsonOptions.SerializerSettings.Converters, c => c is JSelectExpandWrapperConverter);
            Assert.Contains(jsonOptions.SerializerSettings.Converters, c => c is JDynamicTypeWrapperConverter);
            Assert.Contains(jsonOptions.SerializerSettings.Converters, c => c is JPageResultValueConverter);
            Assert.Contains(jsonOptions.SerializerSettings.Converters, c => c is JSingleResultValueConverter);
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
