//-----------------------------------------------------------------------------
// <copyright file="MetadataSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template;

public class MetadataSegmentTemplateTests
{
    [Fact]
    public void GetTemplatesMetadataSegmentTemplate_ReturnsTemplates()
    {
        // Arrange & Act & Assert
        IEnumerable<string> templates = MetadataSegmentTemplate.Instance.GetTemplates();
        string template = Assert.Single(templates);
        Assert.Equal("/$metadata", template);
    }

    [Fact]
    public void TryTranslateMetadataSegmentTemplate_ThrowsArgumentNull_Context()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => MetadataSegmentTemplate.Instance.TryTranslate(null), "context");
    }

    [Fact]
    public void TryTranslateMetadataTemplate_ReturnsODataMetadataSegment()
    {
        // Arrange
        ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

        // Act
        bool ok = MetadataSegmentTemplate.Instance.TryTranslate(context);

        // Assert
        Assert.True(ok);
        ODataPathSegment segment = Assert.Single(context.Segments);
        Assert.Same(MetadataSegment.Instance, segment);
    }
}
