//-----------------------------------------------------------------------------
// <copyright file="EdmUntypedCollectionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value;

public class EdmUntypedCollectionTests
{
    [Fact]
    public void GetEdmType_OnEdmUntypedCollection_Returns_EdmType()
    {
        // Arrange
        EdmUntypedCollection untypedCollection = new EdmUntypedCollection();

        // Act
        IEdmTypeReference edmType = untypedCollection.GetEdmType();

        // Assert
        Assert.Equal("Collection(Edm.Untyped)", edmType.FullName());
    }
}
