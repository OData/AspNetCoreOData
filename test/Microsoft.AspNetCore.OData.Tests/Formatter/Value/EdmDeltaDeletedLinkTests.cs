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
    public class EdmDeltaDeletedLinkTests
    {
        [Fact]
        public void CtorEdmDeltaDeletedLink_ThrowsArgumentNull_TypeReference()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaDeletedLink(entityTypeReference: null), "typeReference");
        }

        [Fact]
        public void CtorEdmDeltaDeletedLink_ThrowsArgumentNull_EntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaDeletedLink(entityType: null), "entityType");

            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaDeletedLink(entityType: null, false), "entityType");
        }

        [Fact]
        public void KindProperty_Returns_DeltaItemKind()
        {
            // Arrange & Act
            Mock<IEdmEntityTypeReference> mock = new Mock<IEdmEntityTypeReference>();
            EdmDeltaDeletedLink edmDeletedLink = new EdmDeltaDeletedLink(mock.Object);

            // Assert
            Assert.Equal(DeltaItemKind.DeltaDeletedLink, edmDeletedLink.Kind);
        }

        [Fact]
        public void CtorEdmDeltaDeletedLink_Sets_PropertyValue()
        {
            // Arrange & Act
            Mock<IEdmEntityType> mock = new Mock<IEdmEntityType>();
            EdmDeltaDeletedLink edmDeletedLink = new EdmDeltaDeletedLink(mock.Object, true);

            // Assert
            Assert.Same(mock.Object, edmDeletedLink.EntityType);
            Assert.Same(mock.Object, edmDeletedLink.GetEdmType().Definition);
            Assert.True(edmDeletedLink.IsNullable);
        }
    }
}
