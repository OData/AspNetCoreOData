//-----------------------------------------------------------------------------
// <copyright file="ODataBodyModelBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataBodyModelBinderTests
    {
        [Fact]
        public async Task BindModelAsync_ThrowsArgumentNull_BindingContext()
        {
            // Arrange & Act & Assert
            ODataBodyModelBinder binder = new ODataBodyModelBinder();
            await ExceptionAssert.ThrowsArgumentNullAsync(() => binder.BindModelAsync(null), "bindingContext");
        }

        [Fact]
        public async Task BindModelAsync_ThrowsArgument_ModelMetadata()
        {
            // Arrange & Act & Assert
            ODataBodyModelBinder binder = new ODataBodyModelBinder();
            Mock<ModelBindingContext> context = new Mock<ModelBindingContext>();
            context.Setup(c => c.ModelMetadata).Returns((ModelMetadata)null);

            ArgumentException exception = await ExceptionAssert.ThrowsAsync<ArgumentException>(() => binder.BindModelAsync(context.Object));
            Assert.Equal("The binding context cannot have a null ModelMetadata. (Parameter 'bindingContext')", exception.Message);
        }

        [Fact]
        public async Task BindModelAsync_RetrievesResult_FromODataFeature()
        {
            // Arrange
            HttpContext httpContext = new DefaultHttpContext();
            ModelBindingContext context = new MyDefaultModelBindingContext(httpContext);
            ModelMetadataIdentity identity = ModelMetadataIdentity.ForProperty(typeof(ATest).GetProperty("Name"), typeof(ATest), typeof(ATest));
            Mock<ModelMetadata> modelMetadata = new Mock<ModelMetadata>(identity);
            context.ModelMetadata = modelMetadata.Object;

            ODataFeature odataFeature = httpContext.ODataFeature() as ODataFeature;
            odataFeature.BodyValues = new Dictionary<string, object>()
            {
                { "Name", "aValue" }
            };

            // Act
            ODataBodyModelBinder binder = new ODataBodyModelBinder();
            await binder.BindModelAsync(context);

            // Assert
            Assert.True(context.Result.IsModelSet);
            Assert.Equal("aValue", context.Result.Model);
        }

        private class ATest
        {
            public string Name { get; set; }
        }

        private class MyDefaultModelBindingContext : DefaultModelBindingContext
        {
            private HttpContext _context;
            public MyDefaultModelBindingContext(HttpContext context)
            {
                _context = context;
            }

            public override HttpContext HttpContext => _context;
        }
    }
}
