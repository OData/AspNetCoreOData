// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class NavigationLinkTemplateSegmentTemplateTests
    {
        private static IEdmModel _model;
        private static IEdmStructuredType _employeeType;
        private static IEdmEntitySet _entitySet;
        private static IEdmNavigationProperty _directReportsNav;
        private static IEdmNavigationProperty _subordinatesNav;

        static NavigationLinkTemplateSegmentTemplateTests()
        {
            EdmModel model = new EdmModel();

            // Employee type
            EdmEntityType employee = new EdmEntityType("NS", "Employee");
            EdmStructuralProperty idProperty = employee.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            employee.AddKeys(idProperty);
            model.AddElement(employee);
            _employeeType = employee;

            // Entity container
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);

            // Navigation property
            _directReportsNav = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "DirectReports",
                Target = employee,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            EdmEntitySet employees = container.AddEntitySet("Employees", employee);
            employees.AddNavigationTarget(_directReportsNav, employees);

            _subordinatesNav = employee.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Subordinates",
                Target = employee,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            employees.AddNavigationTarget(_subordinatesNav, employees);

            _entitySet = employees;
            _model = model;
        }

        [Fact]
        public void CtorNavigationRefSegmentTemplate_ThrowsArgumentNull_DeclaringType()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new NavigationLinkTemplateSegmentTemplate(declaringType: null, navigationSource: null), "declaringType");
        }

        [Fact]
        public void CtorNavigationRefSegmentTemplate_ThrowsArgumentNull_NavigationSource()
        {
            // Arrange & Act & Assert
            IEdmStructuredType structuredType = new Mock<IEdmStructuredType>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => new NavigationLinkTemplateSegmentTemplate(structuredType, null), "navigationSource");
        }

        [Fact]
        public void CtorNavigationRefSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            IEdmStructuredType structuredType = new Mock<IEdmStructuredType>().Object;
            IEdmNavigationSource navigationSource = new Mock<IEdmNavigationSource>().Object;

            NavigationLinkTemplateSegmentTemplate navigationRefSegment = new NavigationLinkTemplateSegmentTemplate(structuredType, navigationSource);

            // Assert
            Assert.Same(structuredType, navigationRefSegment.DeclaringType);
            Assert.Same(navigationSource, navigationRefSegment.NavigationSource);
        }

        [Fact]
        public void GetTemplatesNavigationRefSegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            NavigationLinkTemplateSegmentTemplate navigationSegment = new NavigationLinkTemplateSegmentTemplate(_employeeType, _entitySet);

            // Act & Assert
            IEnumerable<string> templates = navigationSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/{navigationProperty}/$ref", template);

            // Arrange
            navigationSegment = new NavigationLinkTemplateSegmentTemplate(_employeeType, _entitySet)
            {
                RelatedKey = "relatedId"
            };

            // Act & Assert
            templates = navigationSegment.GetTemplates();
            Assert.Collection(templates,
                e => Assert.Equal("/{navigationProperty}({relatedId})/$ref", e),
                e => Assert.Equal("/{navigationProperty}/{relatedId}/$ref", e));
        }

        [Fact]
        public void TryTranslateSingletonSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            IEdmStructuredType structuredType = new Mock<IEdmStructuredType>().Object;
            IEdmNavigationSource navigationSource = new Mock<IEdmNavigationSource>().Object;

            NavigationLinkTemplateSegmentTemplate navigationRefSegment = new NavigationLinkTemplateSegmentTemplate(structuredType, navigationSource);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => navigationRefSegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateNavigationSegmentTemplate_ReturnsODataNavigationLinkSegment()
        {
            // Arrange
            NavigationLinkTemplateSegmentTemplate navigationSegment = new NavigationLinkTemplateSegmentTemplate(_employeeType, _entitySet);

            RouteValueDictionary routeValueDictionary = new RouteValueDictionary(new { navigationProperty = "DirectReports" });

            HttpContext httpContext = new DefaultHttpContext();
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext(httpContext, routeValueDictionary, _model);

            // Act
            bool ok = navigationSegment.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            NavigationPropertyLinkSegment navSegment = Assert.IsType<NavigationPropertyLinkSegment>(actual);
            Assert.Same(_directReportsNav, navSegment.NavigationProperty);
            Assert.Same(_entitySet, navSegment.NavigationSource);
        }
    }
}
