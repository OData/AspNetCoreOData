// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class CountSegmentTemplateTests
    {
        [Fact]
        public void CountCommonPropertiesReturnsAsExpected()
        {
            // Assert & Act & Assert
            Assert.Equal("$count", MetadataSegmentTemplate.Instance.Literal);
            Assert.Equal(ODataSegmentKind.Count, MetadataSegmentTemplate.Instance.Kind);
            Assert.True(MetadataSegmentTemplate.Instance.IsSingle);
            Assert.Null(MetadataSegmentTemplate.Instance.EdmType);
            Assert.Null(MetadataSegmentTemplate.Instance.NavigationSource);
        }

        [Fact]
        public void Translate_ReturnsODataActionImportSegment()
        {
            // Arrange
            ODataTemplateTranslateContext context = new ODataTemplateTranslateContext();

            // Act
            ODataPathSegment actual = CountSegmentTemplate.Instance.Translate(context);

            // Assert
            Assert.NotNull(actual);
            CountSegment countSegment = Assert.IsType<CountSegment>(actual);
            Assert.Same(CountSegment.Instance, countSegment);
        }
    }
}
