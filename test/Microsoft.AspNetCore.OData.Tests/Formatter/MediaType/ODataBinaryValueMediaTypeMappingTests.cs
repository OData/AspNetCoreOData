//-----------------------------------------------------------------------------
// <copyright file="ODataBinaryValueMediaTypeMappingTests.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.Tests.Formatter.MediaType;

public class ODataBinaryValueMediaTypeMappingTests
{
    private static ODataBinaryValueMediaTypeMapping Mapping = new ODataBinaryValueMediaTypeMapping();

    [Fact]
    public void Ctor_SetMediaType()
    {
        // Arrange & Act & Assert
        Assert.Equal("application/octet-stream", Mapping.MediaType.MediaType);
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
    public void TryMatchMediaType_MatchRequest_WithBinaryPrimitivePropertyRequest()
    {
        // Arrange
        EdmEntityType entity = new EdmEntityType("NS", "Entity");
        EdmStructuralProperty property = entity.AddStructuralProperty("Data", EdmCoreModel.Instance.GetBinary(true));
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
