//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaComplexObjectTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmDeltaComplexObjectTests
    {
        [Fact]
        public void CtorEdmDeltaComplexObject_ThrowsArgumentNull_IEdmEntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaComplexObject((IEdmComplexType)null), "edmType");
        }

        [Fact]
        public void CtorEdmDeltaComplexObject_ThrowsArgumentNull_IEdmEntityTypeAndNullable()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaComplexObject(null, true), "edmType");
        }

        [Fact]
        public void CtorEdmDeltaComplexObject_ThrowsArgumentNull_IEdmEntityTypeReference()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaComplexObject((IEdmComplexTypeReference)null), "type");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CtorEdmDeltaComplexObject_SetProperties(bool isNullable)
        {
            // Arrange
            IEdmComplexType complexType = new EdmComplexType("NS", "Complex");
            IEdmComplexTypeReference complex = new EdmComplexTypeReference(complexType, isNullable);

            // Act
            EdmDeltaComplexObject deltaObject = new EdmDeltaComplexObject(complex);

            // Assert
            Assert.Same(complexType, deltaObject.ExpectedEdmType);
            Assert.Same(complexType, deltaObject.ActualEdmType);
            Assert.Equal(DeltaItemKind.Resource, deltaObject.Kind);
        }
    }
}
