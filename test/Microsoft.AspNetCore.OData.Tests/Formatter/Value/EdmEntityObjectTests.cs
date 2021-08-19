//-----------------------------------------------------------------------------
// <copyright file="EdmEntityObjectTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmEntityObjectTests
    {
        [Fact]
        public void CtorEdmEntityObject_ThrowsArgumentNull_IEdmEntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEntityObject((IEdmEntityType)null), "edmType");
        }

        [Fact]
        public void CtorEdmEntityObject_ThrowsArgumentNull_IEdmEntityTypeAndNullable()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEntityObject(null, true), "edmType");
        }

        [Fact]
        public void CtorEdmEntityObject_ThrowsArgumentNull_IEdmEntityTypeReference()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEntityObject((IEdmEntityTypeReference)null), "type");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CtorEdmEntityObject_SetProperties(bool isNullable)
        {
            // Arrange
            IEdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmEntityTypeReference entity = new EdmEntityTypeReference(entityType, isNullable);

            // Act
            EdmEntityObject edmObject = new EdmEntityObject(entity);

            // Assert
            Assert.Same(entityType, edmObject.ExpectedEdmType);
            Assert.Same(entityType, edmObject.ActualEdmType);
            Assert.Equal(isNullable, edmObject.IsNullable);
        }
    }
}
