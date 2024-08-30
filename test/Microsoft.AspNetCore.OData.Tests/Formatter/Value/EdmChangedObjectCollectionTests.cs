//-----------------------------------------------------------------------------
// <copyright file="EdmChangedObjectCollectionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Value;

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
    public void Ctor_ConfigureProperties()
    {
        // Arrange & Act & Assert
        IEdmEntityType entityType = new Mock<IEdmEntityType>().Object;
        IList<IEdmChangedObject> objects = new List<IEdmChangedObject>();
        EdmChangedObjectCollection collection = new EdmChangedObjectCollection(entityType, objects);
        Assert.Empty(collection);
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
