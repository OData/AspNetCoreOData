// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmDeltaCollectionTypeTests
    {
        [Fact]
        public void CtorEdmDeltaCollectionType_ThrowsArgumentNull_Type()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaCollectionType(null), "typeReference");
        }

        [Fact]
        public void CtorEdmDeltaCollectionType_SetsProperties()
        {
            // Arrange & Act
            IEdmTypeReference typeRef = new Mock<IEdmTypeReference>().Object;
            EdmDeltaCollectionType deltaType = new EdmDeltaCollectionType(typeRef);

            // Assert
            Assert.Equal(EdmTypeKind.Collection, deltaType.TypeKind);
            Assert.Same(typeRef, deltaType.ElementType);
        }
    }
}
