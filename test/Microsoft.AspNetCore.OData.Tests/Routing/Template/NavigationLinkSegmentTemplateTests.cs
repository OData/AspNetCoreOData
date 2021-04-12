// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class NavigationLinkSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Navigation()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new NavigationLinkSegmentTemplate(navigationProperty: null, navigationSource: null), "navigationProperty");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationLinkSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CommonNavigationTemplateProperties_ReturnsAsExpected()
        {
            // Assert
            EdmEntityType employee = new EdmEntityType("NS", "Employee");
            IEdmNavigationProperty navigation = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "DirectReports",
                Target = employee,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            NavigationLinkSegmentTemplate linkSegment = new NavigationLinkSegmentTemplate(navigation, null);

            // Act & Assert
            Assert.Equal("DirectReports", linkSegment.Literal);
            Assert.Equal(ODataSegmentKind.NavigationLink, linkSegment.Kind);
            Assert.False(linkSegment.IsSingle);
            Assert.Equal("Collection(NS.Employee)", linkSegment.EdmType.FullTypeName());
            Assert.Null(linkSegment.NavigationSource);
        }

        [Fact]
        public void TryTranslateNavigationSegmentTemplate_ReturnsODataNavigationSegment()
        {
            // Arrange
            EdmEntityType employee = new EdmEntityType("NS", "Employee");
            IEdmNavigationProperty navigation = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "DirectReports",
                Target = employee,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            NavigationLinkSegmentTemplate linkSegment = new NavigationLinkSegmentTemplate(navigation, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            bool ok = linkSegment.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            NavigationPropertyLinkSegment navLinkSegment = Assert.IsType<NavigationPropertyLinkSegment>(actual);
            Assert.Same(navigation, navLinkSegment.NavigationProperty);
            Assert.Null(navLinkSegment.NavigationSource);
        }
    }
}
