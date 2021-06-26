// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmChangedObjectCollectionTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_EdmType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmChangedObjectCollection(entityType: null), "entityType");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_List()
        {
            // Arrange & Act & Assert
            IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => new EdmChangedObjectCollection(entityType, changedObjectList: null), "list");
        }

        [Fact]
        public void GetEdmType_Returns_EdmTypeInitializedByCtor()
        {
            // Arrange
            IEdmEntityType _entityType = new EdmEntityType("NS", "Entity");
            var edmObject = new EdmChangedObjectCollection(_entityType);
            IEdmCollectionTypeReference collectionTypeReference = (IEdmCollectionTypeReference)edmObject.GetEdmType();

            // Act & Assert
            Assert.Same(_entityType, collectionTypeReference.ElementType().Definition);
        }
    }
}
