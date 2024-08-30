//-----------------------------------------------------------------------------
// <copyright file="EdmObjectHelperTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.OData.Edm;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value;

public class EdmObjectHelperTests
{
    [Fact]
    public void ConvertToEdmObject_Converts_ComplexCollection()
    {
        // Arrange
        EdmComplexType complexType = new EdmComplexType("NS", "Complex");
        IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(complexType, true)));
        var source = new List<EdmComplexObject>
        {
            new EdmComplexObject(complexType, true),
            new EdmComplexObject(complexType, true)
        };

        // Act
        IEdmObject obj = source.ConvertToEdmObject(collectionType);

        // Assert
        EdmComplexObjectCollection complexCollection = Assert.IsType<EdmComplexObjectCollection>(obj);
        Assert.Equal(2, complexCollection.Count);
    }
}
