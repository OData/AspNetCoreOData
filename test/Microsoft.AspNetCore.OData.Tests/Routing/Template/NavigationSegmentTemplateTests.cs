// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class NavigationSegmentTemplateTests
    {
        [Fact]
        public void Ctor_ThrowsArgumentNull_Navigation()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new NavigationSegmentTemplate(navigationProperty: null, navigationSource: null), "navigationProperty");
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationSegmentTemplate(segment: null), "segment");
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

            NavigationSegmentTemplate navigationSegment = new NavigationSegmentTemplate(navigation, null);

            // Act & Assert
            Assert.Equal("DirectReports", navigationSegment.Literal);
            Assert.Equal(ODataSegmentKind.Navigation, navigationSegment.Kind);
            Assert.False(navigationSegment.IsSingle);
            Assert.Equal("Collection(NS.Employee)", navigationSegment.EdmType.FullTypeName());
            Assert.Null(navigationSegment.NavigationSource);
        }

        [Fact]
        public void TranslateNavigationSegmentTemplate_ReturnsODataNavigationSegment()
        {
            // Arrange
            EdmEntityType employee = new EdmEntityType("NS", "Employee");
            IEdmNavigationProperty navigation = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "DirectReports",
                Target = employee,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            NavigationSegmentTemplate navigationSegment = new NavigationSegmentTemplate(navigation, null);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = navigationSegment.Translate(context);

            // Assert
            Assert.NotNull(actual);
            NavigationPropertySegment navSegment = Assert.IsType<NavigationPropertySegment>(actual);
            Assert.Same(navigation, navSegment.NavigationProperty);
            Assert.Null(navSegment.NavigationSource);
        }
    }
}
