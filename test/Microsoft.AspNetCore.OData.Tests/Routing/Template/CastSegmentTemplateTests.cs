//-----------------------------------------------------------------------------
// <copyright file="CastSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template;

public class CastSegmentTemplateTests
{
    [Fact]
    public void CtorCastSegmentTemplate_ThrowsArgumentNull_CastType()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new CastSegmentTemplate(null, null, null), "castType");
    }

    [Fact]
    public void CtorCastSegmentTemplate_ThrowsArgumentNull_ExpectedType()
    {
        // Assert
        Mock<IEdmType> edmType = new Mock<IEdmType>();

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new CastSegmentTemplate(edmType.Object, null, null), "expectedType");
    }

    [Fact]
    public void CtorCastSegmentTemplate_ThrowsArgumentNull_TypeSegment()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new CastSegmentTemplate(null), "typeSegment");
    }

    [Fact]
    public void CtorCastSegmentTemplate_SetsProperties()
    {
        // Arrange & Act
        EdmEntityType baseType = new EdmEntityType("NS", "base");
        EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
        CastSegmentTemplate segment = new CastSegmentTemplate(subType, baseType, null);

        // Assert
        Assert.Same(subType, segment.CastType);
        Assert.Same(baseType, segment.ExpectedType);
        Assert.NotNull(segment.TypeSegment);

        // Act & Assert
        CastSegmentTemplate segment1 = new CastSegmentTemplate(segment.TypeSegment);
        Assert.Same(segment.TypeSegment, segment1.TypeSegment);
    }

    [Fact]
    public void GetTemplatesCastSegmentTemplate_ReturnsTemplates()
    {
        // Assert
        EdmEntityType baseType = new EdmEntityType("NS", "base");
        EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
        CastSegmentTemplate segment = new CastSegmentTemplate(subType, baseType, null);

        // Act & Assert
        IEnumerable<string> templates = segment.GetTemplates();
        string template = Assert.Single(templates);
        Assert.Equal("/NS.sub", template);
    }

    [Fact]
    public void TryTranslateCastSegmentTemplate_ThrowsArgumentNull_Context()
    {
        // Arrange
        EdmEntityType baseType = new EdmEntityType("NS", "base");
        EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
        CastSegmentTemplate segment = new CastSegmentTemplate(subType, baseType, null);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => segment.TryTranslate(null), "context");
    }

    [Fact]
    public void TryTranslateCastSegmentTemplate_ReturnsODataTypeSegment()
    {
        // Arrange
        EdmEntityType baseType = new EdmEntityType("NS", "base");
        EdmEntityType subType = new EdmEntityType("NS", "sub", baseType);
        CastSegmentTemplate template = new CastSegmentTemplate(subType, baseType, null);
        ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

        // Act
        bool ok = template.TryTranslate(context);

        // Assert
        Assert.True(ok);
        ODataPathSegment actual = Assert.Single(context.Segments);
        TypeSegment typeSegment = Assert.IsType<TypeSegment>(actual);
        Assert.Same(subType, typeSegment.EdmType);
    }
}
