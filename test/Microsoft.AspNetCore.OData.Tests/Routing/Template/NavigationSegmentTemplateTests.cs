//-----------------------------------------------------------------------------
// <copyright file="NavigationSegmentTemplateTests.cs" company=".NET Foundation">
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

public class NavigationSegmentTemplateTests
{
    [Fact]
    public void CtorNavigationSegmentTemplate_ThrowsArgumentNull_Navigation()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(
            () => new NavigationSegmentTemplate(navigationProperty: null, navigationSource: null), "navigationProperty");
    }

    [Fact]
    public void CtorNavigationSegmentTemplate_ThrowsArgumentNull_Segment()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => new NavigationSegmentTemplate(segment: null), "segment");
    }

    [Fact]
    public void CtorNavigationSegmentTemplate_SetsProperties()
    {
        // Arrange & Act
        NavigationSegmentTemplate navigationSegment = GetSegmentTemplate(out IEdmNavigationProperty navigation);

        // Assert
        Assert.NotNull(navigationSegment.Segment);
        Assert.Same(navigation, navigationSegment.NavigationProperty);
    }

    [Fact]
    public void GetTemplatesNavigationSegmentTemplate_ReturnsTemplates()
    {
        // Arrange
        NavigationSegmentTemplate navigationSegment = GetSegmentTemplate(out _);

        // Act & Assert
        IEnumerable<string> templates = navigationSegment.GetTemplates();
        string template = Assert.Single(templates);
        Assert.Equal("/DirectReports", template);
    }

    [Fact]
    public void TryTranslateSingletonSegmentTemplate_ThrowsArgumentNull_Context()
    {
        // Arrange
        NavigationSegmentTemplate navigationSegment = GetSegmentTemplate(out _);

        // Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => navigationSegment.TryTranslate(null), "context");
    }

    [Fact]
    public void TryTranslateNavigationSegmentTemplate_ReturnsODataNavigationSegment()
    {
        // Arrange
        NavigationSegmentTemplate navigationSegment = GetSegmentTemplate(out IEdmNavigationProperty navigation);
        ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

        // Act
        bool ok = navigationSegment.TryTranslate(context);

        // Assert
        Assert.True(ok);
        ODataPathSegment actual = Assert.Single(context.Segments);
        NavigationPropertySegment navSegment = Assert.IsType<NavigationPropertySegment>(actual);
        Assert.Same(navigation, navSegment.NavigationProperty);
        Assert.Null(navSegment.NavigationSource);
    }

    private static NavigationSegmentTemplate GetSegmentTemplate(out IEdmNavigationProperty navigation)
    {
        EdmEntityType employee = new EdmEntityType("NS", "Employee");
        navigation = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
        {
            Name = "DirectReports",
            Target = employee,
            TargetMultiplicity = EdmMultiplicity.Many
        });

        return new NavigationSegmentTemplate(navigation, null);
    }
}
