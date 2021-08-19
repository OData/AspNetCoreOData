//-----------------------------------------------------------------------------
// <copyright file="ODataPrimitiveValueMediaTypeMappingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.MediaType;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.MediaType
{
    public class ODataPrimitiveValueMediaTypeMappingTests
    {
        private static ODataPrimitiveValueMediaTypeMapping Mapping = new ODataPrimitiveValueMediaTypeMapping();

        [Fact]
        public void Ctor_SetMediaType()
        {
            // Arrange & Act & Assert
            Assert.Equal("text/plain", Mapping.MediaType.MediaType);
        }

        [Fact]
        public void TryMatchMediaType_ThrowsArgumentNull_WhenRequestIsNull()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => Mapping.TryMatchMediaType(null), "request");
        }

        [Fact]
        public void TryMatchMediaType_DoesnotMatchRequest_WithNonODataRequest()
        {
            // Arrange
            HttpRequest request = new DefaultHttpContext().Request;

            // Act
            double mapResult = Mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Fact]
        public void TryMatchMediaType_DoesnotMatchRequest_WithBinaryPropertyRequest()
        {
            // Arrange
            EdmEntityType entity = new EdmEntityType("NS", "Entity");
            EdmStructuralProperty property = entity.AddStructuralProperty("Photo", EdmCoreModel.Instance.GetStream(true));
            PropertySegment propertySegment = new PropertySegment(property);

            HttpRequest request = new DefaultHttpContext().Request;
            request.ODataFeature().Path = new ODataPath(propertySegment);

            // Act
            double mapResult = Mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(0, mapResult);
        }

        [Theory]
        [InlineData(EdmPrimitiveTypeKind.Int16)]
        [InlineData(EdmPrimitiveTypeKind.Int32)]
        [InlineData(EdmPrimitiveTypeKind.String)]
        [InlineData(EdmPrimitiveTypeKind.Double)]
        public void TryMatchMediaType_MatchRequest_WithPrimitiveRawValueRequest(EdmPrimitiveTypeKind kind)
        {
            // Arrange
            EdmEntityType entity = new EdmEntityType("NS", "Entity");
            EdmStructuralProperty property = entity.AddStructuralProperty("Age",
                EdmCoreModel.Instance.GetPrimitive(kind, true));
            PropertySegment propertySegment = new PropertySegment(property);
            ValueSegment valueSegment = new ValueSegment(property.Type.Definition);

            HttpRequest request = new DefaultHttpContext().Request;
            request.ODataFeature().Path = new ODataPath(propertySegment, valueSegment);

            // Act
            double mapResult = Mapping.TryMatchMediaType(request);

            // Assert
            Assert.Equal(1.0, mapResult);
        }
    }
}
