// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class NullEdmComplexObjectTests
    {
        [Fact]
        public void CtorNullEdmComplexObject_ThrowsArgumentNull_EdmType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NullEdmComplexObject(null), "edmType");
        }

        [Fact]
        public void TryGetPropertyValue_ThrowsInvalidOperation()
        {
            // Arrange & Act & Assert
            EdmComplexType complex = new EdmComplexType("NS", "Complex");
            IEdmComplexTypeReference typeRef = new EdmComplexTypeReference(complex, false);
            NullEdmComplexObject obj = new NullEdmComplexObject(typeRef);
            ExceptionAssert.Throws<InvalidOperationException>(() => obj.TryGetPropertyValue("name", out object value),
                "Cannot get property 'name' of a null EDM object of type '[NS.Complex Nullable=False]'.");
        }

        [Fact]
        public void GetEdmType_ReturnsInputTypeReference()
        {
            // Arrange & Act
            IEdmComplexTypeReference typeRef = new Mock<IEdmComplexTypeReference>().Object;
            NullEdmComplexObject obj = new NullEdmComplexObject(typeRef);

            IEdmTypeReference actual = obj.GetEdmType();

            // Assert
            Assert.Same(typeRef, actual);
        }
    }
}
