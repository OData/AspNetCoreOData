// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class ODataModelBinderTests
    {
        [Fact]
        public void BindModelAsyncODataModelBinder_ThrowsArgumentNull_BindingContext()
        {
            // Arrange & Act & Assert
            ODataModelBinder binder = new ODataModelBinder();
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindModelAsync(null), "bindingContext");
        }

        [Fact]
        public void BindModelAsyncODataModelBinder_ThrowsArgumentNull_ModelMetadata()
        {
            // Arrange
            ODataModelBinder binder = new ODataModelBinder();
            Mock<ModelBindingContext> mock = new Mock<ModelBindingContext>();
            mock.Setup(m => m.ModelState).Returns((ModelStateDictionary)null);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(() => binder.BindModelAsync(mock.Object),
                "bindingContext", "The binding context cannot have a null ModelMetadata. (Parameter 'bindingContext')");
        }
    }
}