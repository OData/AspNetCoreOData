// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class CountSegmentTemplateTests
    {
        [Fact]
        public void GetTemplatesCountSegmentTemplate_ReturnsTemplates()
        {
            // Assert & Act & Assert
            IEnumerable<string> templates = CountSegmentTemplate.Instance.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/$count", template);
        }

        [Fact]
        public void TryTranslateCountSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => CountSegmentTemplate.Instance.TryTranslate(null), "context");
        }

        [Fact]
        public void TryTranslateCountSegmentTemplate_ReturnsODataCountSegment()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            bool ok = CountSegmentTemplate.Instance.TryTranslate(context);

            // Assert
            Assert.True(ok);
            ODataPathSegment actual = Assert.Single(context.Segments);
            CountSegment countSegment = Assert.IsType<CountSegment>(actual);
            Assert.Same(CountSegment.Instance, countSegment);
        }
    }
}
