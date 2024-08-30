//-----------------------------------------------------------------------------
// <copyright file="EdmDeltaCollectionTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value;

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
