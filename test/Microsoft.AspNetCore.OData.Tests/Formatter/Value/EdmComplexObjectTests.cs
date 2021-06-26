// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmComplexObjectTests
    {
        [Fact]
        public void CtorEdmComplexObject_ThrowsArgumentNull_IEdmComplexType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmComplexObject((IEdmComplexType)null), "edmType");
        }

        [Fact]
        public void CtorEdmComplexObject_ThrowsArgumentNull_IEdmComplexTypeAndNullable()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmComplexObject(null, true), "edmType");
        }

        [Fact]
        public void CtorEdmComplexObject_ThrowsArgumentNull_IEdmComplexTypeReference()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmComplexObject((IEdmComplexTypeReference)null), "type");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CtorEdmComplexObject_SetProperties(bool isNullable)
        {
            // Arrange
            EdmComplexType complexType = new EdmComplexType("NS", "Complex");
            IEdmComplexTypeReference complex = new EdmComplexTypeReference(complexType, isNullable);

            // Act
            EdmComplexObject edmObject = new EdmComplexObject(complex);

            // Assert
            Assert.Same(complexType, edmObject.ExpectedEdmType);
            Assert.Same(complexType, edmObject.ActualEdmType);
            Assert.Equal(isNullable, edmObject.IsNullable);
        }
    }
}
