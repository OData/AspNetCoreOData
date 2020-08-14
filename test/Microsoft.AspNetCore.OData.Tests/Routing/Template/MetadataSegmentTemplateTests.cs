// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class MetadataSegmentTemplateTests
    {
        [Fact]
        public void MetadataCommonPropertiesReturnsAsExpected()
        {
            // Assert & Act & Assert
            Assert.Equal("$metadata", MetadataSegmentTemplate.Instance.Literal);
            Assert.Equal(ODataSegmentKind.Metadata, MetadataSegmentTemplate.Instance.Kind);
            Assert.True(MetadataSegmentTemplate.Instance.IsSingle);
            Assert.Null(MetadataSegmentTemplate.Instance.EdmType);
            Assert.Null(MetadataSegmentTemplate.Instance.NavigationSource);
        }

        [Fact]
        public void TranslateMetadataTemplateReturnsAsExpected()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment segment = MetadataSegmentTemplate.Instance.Translate(context);

            // Assert
            Assert.Same(MetadataSegment.Instance, segment);
        }
    }
}
