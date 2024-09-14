//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaResourceObjectTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value;

public class EdmDeltaResourceObjectTests
{
    [Fact]
    public void CtorEdmDeltaResourceObject_ThrowsArgumentNull_IEdmEntityType()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaResourceObject((IEdmEntityType)null), "edmType");
    }

    [Fact]
    public void CtorEdmDeltaResourceObject_ThrowsArgumentNull_IEdmEntityTypeAndNullable()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaResourceObject(null, true), "edmType");
    }

    [Fact]
    public void CtorEdmDeltaResourceObject_ThrowsArgumentNull_IEdmEntityTypeReference()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new EdmDeltaResourceObject((IEdmEntityTypeReference)null), "type");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CtorEdmDeltaResourceObject_SetProperties(bool isNullable)
    {
        // Arrange
        IEdmEntityType entityType = new EdmEntityType("NS", "Entity");
        IEdmEntityTypeReference entity = new EdmEntityTypeReference(entityType, isNullable);

        // Act
        EdmDeltaResourceObject deltaObject = new EdmDeltaResourceObject(entity);

        // Assert
        Assert.Same(entityType, deltaObject.ExpectedEdmType);
        Assert.Same(entityType, deltaObject.ActualEdmType);
        Assert.Equal(DeltaItemKind.Resource, deltaObject.DeltaKind);
    }
}
