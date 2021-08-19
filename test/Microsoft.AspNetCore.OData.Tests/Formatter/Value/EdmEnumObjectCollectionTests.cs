//-----------------------------------------------------------------------------
// <copyright file="EdmEnumObjectCollectionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmEnumObjectCollectionTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEnumObjectCollection(edmType: null), "edmType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_List()
        {
            IEdmCollectionTypeReference edmType = new Mock<IEdmCollectionTypeReference>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => new EdmEnumObjectCollection(edmType, list: null), "list");
        }

        [Fact]
        public void Ctor_ThrowsArgument_UnexpectedElementType()
        {
            // Arrange
            IEdmTypeReference elementType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);

            // Act
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType));

            // Assert
            ExceptionAssert.ThrowsArgument(() => new EdmEnumObjectCollection(collectionType), "edmType",
            "The element type '[Edm.Int32 Nullable=True]' of the given collection type '[Collection([Edm.Int32 Nullable=True]) Nullable=True]' " +
            "is not of the type 'IEdmEnumType'.");
        }

        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            // Arrange
            IEdmTypeReference elementType = new EdmEnumTypeReference(new EdmEnumType("NS", "Enum"), isNullable: false);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(elementType));

            // Act
            var edmObject = new EdmEnumObjectCollection(collectionType);

            // Assert
            Assert.Same(collectionType, edmObject.GetEdmType());
        }
    }
}
