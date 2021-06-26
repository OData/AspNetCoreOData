// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class NavigationLinkSegmentTemplateTests
    {
        private static IEdmEntityType _employee;
        private static IEdmNavigationProperty _navigation;

        static NavigationLinkSegmentTemplateTests()
        {
            EdmEntityType employee = new EdmEntityType("NS", "Employee");
            employee.AddKeys(employee.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            _navigation = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "DirectReports",
                Target = employee,
                TargetMultiplicity = EdmMultiplicity.Many
            });

            _employee = employee;
        }

        [Fact]
        public void CtorNavigationLinkSegmentTemplate_ThrowsArgumentNull_NavigationProperty()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new NavigationLinkSegmentTemplate(navigationProperty: null, navigationSource: null), "navigationProperty");
        }

        [Fact]
        public void CtorNavigationLinkSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationLinkSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorNavigationLinkSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            NavigationLinkSegmentTemplate linkSegment = new NavigationLinkSegmentTemplate(_navigation, null);

            // Assert
            Assert.Null(linkSegment.Key);
            Assert.Same(_navigation, linkSegment.NavigationProperty);
            Assert.Null(linkSegment.NavigationSource);
            Assert.NotNull(linkSegment.Segment);
        }

        [Fact]
        public void GetTemplatesNavigationLinkSegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            NavigationLinkSegmentTemplate linkSegment = new NavigationLinkSegmentTemplate(_navigation, null);

            // Act & Assert
            IEnumerable<string> templates = linkSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/DirectReports/$ref", template);

            // Act & Assert
            linkSegment.Key = KeySegmentTemplate.CreateKeySegment(_employee, null);
            templates = linkSegment.GetTemplates();
            Assert.Collection(templates,
                e => Assert.Equal("/DirectReports({key})/$ref", e),
                e => Assert.Equal("/DirectReports/{key}/$ref", e));
        }

        [Fact]
        public void TryTranslateNavigationLinkSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            NavigationLinkSegmentTemplate linkSegment = new NavigationLinkSegmentTemplate(_navigation, null);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => linkSegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateNavigationLinkSegmentTemplate_ReturnsODataNavigationSegment()
        {
            // Arrange
            EdmModel model = new EdmModel();
            model.AddElement(_employee);

            NavigationLinkSegmentTemplate linkSegment = new NavigationLinkSegmentTemplate(_navigation, null);
            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { id = "42" });
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary,
                Model = model
            };

            // Without key segment
            // Act & Assert
            bool ok = linkSegment.TryTranslate(context);

            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            NavigationPropertyLinkSegment navLinkSegment = Assert.IsType<NavigationPropertyLinkSegment>(actual);
            Assert.Same(_navigation, navLinkSegment.NavigationProperty);
            Assert.Null(navLinkSegment.NavigationSource);

            // With Key segment
            // Act & Assert
            context = new ODataTemplateTranslateContext
            {
                RouteValues = routeValueDictionary,
                Model = model
            };

            linkSegment.Key = KeySegmentTemplate.CreateKeySegment(_employee, null, "id");
            ok = linkSegment.TryTranslate(context);

            Assert.True(ok);
            Assert.Collection(context.Segments,
                e =>
                {
                    NavigationPropertyLinkSegment navLinkSegment = Assert.IsType<NavigationPropertyLinkSegment>(e);
                    Assert.Same(_navigation, navLinkSegment.NavigationProperty);
                    Assert.Null(navLinkSegment.NavigationSource);
                },
                e =>
                {
                    KeySegment keySegment = Assert.IsType<KeySegment>(e);
                    KeyValuePair<string, object> key = Assert.Single(keySegment.Keys);
                    Assert.Equal("Id", key.Key);
                    Assert.Equal(42, key.Value);
                });
        }
    }
}
