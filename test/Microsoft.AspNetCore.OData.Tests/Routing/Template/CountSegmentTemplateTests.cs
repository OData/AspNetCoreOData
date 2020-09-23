// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Template
{
    public class CountSegmentTemplateTests
    {
        [Fact]
        public void CommonCountProperties_ReturnsAsExpected()
        {
            // Assert & Act & Assert
            Assert.Equal("$count", CountSegmentTemplate.Instance.Literal);
            Assert.Equal(ODataSegmentKind.Count, CountSegmentTemplate.Instance.Kind);
            Assert.True(CountSegmentTemplate.Instance.IsSingle);
            Assert.Equal("Edm.Int32", CountSegmentTemplate.Instance.EdmType.FullTypeName());
            Assert.Null(CountSegmentTemplate.Instance.NavigationSource);
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
