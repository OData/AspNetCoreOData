// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmDeltaLinkTests
    {
        [Fact]
        public void CtorEdmDeltaLink_ThrowsArgumentNull_TypeReference()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaLink(entityTypeReference: null), "typeReference");
        }

        [Fact]
        public void CtorEdmDeltaLink_ThrowsArgumentNull_EntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaLink(entityType: null), "entityType");

            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaLink(entityType: null, false), "entityType");
        }

        [Fact]
        public void KindProperty_Returns_DeltaItemKind()
        {
            // Arrange & Act
            Mock<IEdmEntityTypeReference> mock = new Mock<IEdmEntityTypeReference>();
            EdmDeltaLink edmLink = new EdmDeltaLink(mock.Object);

            // Assert
            Assert.Equal(DeltaItemKind.DeltaLink, edmLink.Kind);
        }

        [Fact]
        public void CtorEdmDeltaLink_Sets_PropertyValue()
        {
            // Arrange & Act
            Mock<IEdmEntityType> mock = new Mock<IEdmEntityType>();
            EdmDeltaLink edmLink = new EdmDeltaLink(mock.Object, true);

            // Assert
            Assert.Same(mock.Object, edmLink.EntityType);
            Assert.Same(mock.Object, edmLink.GetEdmType().Definition);
            Assert.True(edmLink.IsNullable);
        }
    }
}
