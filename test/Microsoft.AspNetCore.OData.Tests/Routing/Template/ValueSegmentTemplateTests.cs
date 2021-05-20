// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class ValueSegmentTemplateTests
    {
        [Fact]
        public void CtorValueSegmentTemplate_ThrowsArgumentNull_Segment()
        {
            // Assert & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ValueSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void GetTemplatesValueSegmentTemplate_ReturnsTemplates()
        {
            // Assert
            IEdmPrimitiveType primitive = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
            ValueSegmentTemplate valueSegment = new ValueSegmentTemplate(primitive);

            // Act & Assert
            IEnumerable<string> templates = valueSegment.GetTemplates();
            string template = Assert.Single(templates);
            Assert.Equal("/$value", template);
        }

        [Fact]
        public void TryTranslateValueSegmentTemplate_ThrowsArgumentNull_Context()
        {
            // Arrange
            IEdmPrimitiveType primitive = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            ValueSegmentTemplate valueSegment = new ValueSegmentTemplate(primitive);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => valueSegment.TryTranslate(null), "context");
        }

        [Theory]
        [InlineData(EdmPrimitiveTypeKind.Int32)]
        [InlineData(EdmPrimitiveTypeKind.String)]
        [InlineData(EdmPrimitiveTypeKind.Double)]
        [InlineData(EdmPrimitiveTypeKind.Guid)]
        [InlineData(EdmPrimitiveTypeKind.Date)]
        public void TryTranslateValueSegmentTemplate_ReturnsValueSegment(EdmPrimitiveTypeKind kind)
        {
            // Arrange
            IEdmPrimitiveType primitive = EdmCoreModel.Instance.GetPrimitiveType(kind);
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();
            ValueSegmentTemplate valueSegment = new ValueSegmentTemplate(primitive);

            // Act
            Assert.True(valueSegment.TryTranslate(context));

            // Assert
            ODataPathSegment segment = Assert.Single(context.Segments);
            ValueSegment odataValueSegment = Assert.IsType<ValueSegment>(segment);
            Assert.Same(primitive, odataValueSegment.EdmType);
        }
    }
}
