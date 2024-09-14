//-----------------------------------------------------------------------------
// <copyright file="DefaultODataPathParserTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Parser;

public class DefaultODataPathParserTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("http://any")]
    public void CanParseEntitySetPathWithOrWithoutServiceRoot(string serviceRoot)
    {
        // Arrange
        IEdmModel model = GetEdmModel();
        IODataPathParser parser = new DefaultODataPathParser();
        Uri serviceRootUri = serviceRoot == null ? null : new Uri(serviceRoot);

        // Act
        ODataPath path = parser.Parse(model, serviceRootUri, new Uri("Customers", UriKind.RelativeOrAbsolute), null);

        // Assert
        Assert.NotNull(path);
        ODataPathSegment segment = Assert.Single(path);
        EntitySetSegment setSegment = Assert.IsType<EntitySetSegment>(segment);
        Assert.Equal("Customers", setSegment.EntitySet.Name);
    }

    private static EdmModel GetEdmModel()
    {
        EdmModel model = new EdmModel();
        EdmEntityType customer = new EdmEntityType("NS", "Customer");
        customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
        customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);

        EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
        entityContainer.AddEntitySet("Customers", customer);
        model.AddElement(entityContainer);
        return model;
    }
}
