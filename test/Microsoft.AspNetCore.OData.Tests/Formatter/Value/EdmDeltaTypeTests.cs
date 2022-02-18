//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value
{
    public class EdmDeltaTypeTests
    {
        [Fact]
        public void CtorEdmDeltaType_ThrowsArgumentNull_EntityType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaType(null, DeltaItemKind.Resource), "entityType");
        }

        [Fact]
        public void CtorEdmDeltaType_SetsProperties()
        {
            // Arrange & Act
            IEdmEntityType typeRef = new Mock<IEdmEntityType>().Object;
            EdmDeltaType deltaType = new EdmDeltaType(typeRef, DeltaItemKind.Resource);

            // Assert
            Assert.Equal(EdmTypeKind.Entity, deltaType.TypeKind);
            Assert.Same(typeRef, deltaType.EntityType);
            Assert.Equal(DeltaItemKind.Resource, deltaType.DeltaKind);
        }
    }
}
