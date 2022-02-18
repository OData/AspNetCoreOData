//-----------------------------------------------------------------------------
// <copyright file="DynamicSegmentTemplateTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class DynamicSegmentTemplateTests
    {
        [Fact]
        public void CtorDynamicSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new DynamicSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CtorDynamicSegmentTemplate_SetsProperties()
        {
            // Arrange & Act
            DynamicPathSegment segment = new DynamicPathSegment("dynamic");
            DynamicSegmentTemplate dynamicSegment = new DynamicSegmentTemplate(segment);

            // Assert
            Assert.Same(segment, dynamicSegment.Segment);
        }

        [Fact]
        public void GetTemplatesDynamicSegmentTemplate_ReturnsTemplates()
        {
            // Arrange
            DynamicPathSegment segment = new DynamicPathSegment("dynamic");
            DynamicSegmentTemplate dynamicSegment = new DynamicSegmentTemplate(segment);

            // Act & Assert
            IEnumerable<string> templates = dynamicSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/dynamic", template);
        }

        [Fact]
        public void TryTranslateDynamicSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            DynamicPathSegment segment = new DynamicPathSegment("dynamic");
            DynamicSegmentTemplate dynamicSegment = new DynamicSegmentTemplate(segment);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => dynamicSegment.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateDynamicSegmentTemplate_ReturnsODataCountSegment()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            DynamicPathSegment segment = new DynamicPathSegment("dynamic");
            DynamicSegmentTemplate dynamicSegment = new DynamicSegmentTemplate(segment);

            // Act
            bool ok = dynamicSegment.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            DynamicPathSegment actualSegment = Assert.IsType<DynamicPathSegment>(actual);
            Assert.Same(segment, actualSegment);
        }
    }
}
