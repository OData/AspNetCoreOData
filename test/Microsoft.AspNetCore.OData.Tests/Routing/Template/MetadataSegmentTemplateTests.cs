// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class MetadataSegmentTemplateTests
    {
        [Fact]
        public void CommonMetadataTemplateProperties_ReturnsAsExpected()
        {
            // Assert & Act & Assert
            Assert.Equal("$metadata", MetadataSegmentTemplate.Instance.Literal);
            Assert.Equal(ODataSegmentKind.Metadata, MetadataSegmentTemplate.Instance.Kind);
            Assert.True(MetadataSegmentTemplate.Instance.IsSingle);
            Assert.Null(MetadataSegmentTemplate.Instance.EdmType);
            Assert.Null(MetadataSegmentTemplate.Instance.NavigationSource);
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
}
