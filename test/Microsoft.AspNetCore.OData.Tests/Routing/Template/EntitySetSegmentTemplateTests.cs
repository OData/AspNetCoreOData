//-----------------------------------------------------------------------------
// <copyright file="EntitySetSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template;

public class EntitySetSegmentTemplateTests
{
    [Fact]
    public void CtorEntitySetSegmentTemplate_ThrowsArgumentNull_EntitySet()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new EntitySetSegmentTemplate(entitySet: null), "entitySet");
    }

    [Fact]
    public void CtorEntitySetSegmentTemplate_ThrowsArgumentNull_EntitySetSegment()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new EntitySetSegmentTemplate(segment: null), "segment");
    }

    [Fact]
    public void CtorEntitySetSegmentTemplate_SetsProperties()
    {
        // Arrange & Act
        EntitySetSegmentTemplate segmentTemplate = GetSegmentTemplate(out EdmEntitySet entitySet);

        // Assert
        Assert.NotNull(segmentTemplate.Segment);
        Assert.Same(entitySet, segmentTemplate.EntitySet);
    }

    [Fact]
    public void GetTemplatesEntitySetSegmentTemplate_ReturnsTemplates()
    {
        // Arrange
        EntitySetSegmentTemplate segmentTemplate = GetSegmentTemplate(out _);

        // Act & Assert
        IEnumerable<string> templates = segmentTemplate.GetTemplates();
        string template = Assert.Single(templates);
        Assert.Equal("/entities", template);
    }

    [Fact]
    public void TryTranslateEntitySetSegmentTemplate_ThrowsArgumentNull_Context()
    {
        // Arrange
        EntitySetSegmentTemplate template = GetSegmentTemplate(out _);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => template.TryTranslate(null), "context");
    }

    [Fact]
    public void TryTranslateEntitySetSegmentTemplate_ReturnsODataEntitySetSegment()
    {
        // Arrange
        EntitySetSegmentTemplate template = GetSegmentTemplate(out EdmEntitySet entitySet);
        ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

        // Act
        bool ok = template.TryTranslate(context);

        // Assert
        Assert.True(ok);
        ODataPathSegment actual = Assert.Single(context.Segments);
        EntitySetSegment setSegment = Assert.IsType<EntitySetSegment>(actual);
        Assert.Same(entitySet, setSegment.EntitySet);
    }

    private static EntitySetSegmentTemplate GetSegmentTemplate(out EdmEntitySet entitySet)
    {
        EdmEntityType entityType = new EdmEntityType("NS", "entity");
        EdmEntityContainer container = new EdmEntityContainer("NS", "default");
        entitySet = new EdmEntitySet(container, "entities", entityType);
        return new EntitySetSegmentTemplate(entitySet);
    }
}
