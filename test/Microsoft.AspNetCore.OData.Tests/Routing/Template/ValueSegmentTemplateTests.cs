// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public void Ctor_ThrowsArgumentNull_Segment()
        {
            // Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ValueSegmentTemplate(segment: null), "segment");
        }

        [Fact]
        public void CommonValueSegmentTemplateProperties_ReturnsAsExpected()
        {
            // Assert
            IEdmPrimitiveType primitive = EdmCoreModel.Instance.GetPrimitiveType(EdmPrimitiveTypeKind.Int32);
            ValueSegmentTemplate valueSegment = new ValueSegmentTemplate(primitive);

            // Act & Assert
            Assert.Equal("$value", valueSegment.Literal);
            Assert.Equal(ODataSegmentKind.Value, valueSegment.Kind);
            Assert.True(valueSegment.IsSingle);
            Assert.Same(primitive, valueSegment.EdmType);
            Assert.Null(valueSegment.NavigationSource);
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
