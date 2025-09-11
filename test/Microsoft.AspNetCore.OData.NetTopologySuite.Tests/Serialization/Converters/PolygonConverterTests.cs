//-----------------------------------------------------------------------------
// <copyright file="PolygonConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyPolygon = Microsoft.Spatial.GeographyPolygon;
using MsGeometryPolygon = Microsoft.Spatial.GeometryPolygon;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class PolygonConverterTests
{
    private readonly PolygonConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryPolygon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var polygon = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(0, 1),
            new NtsCoordinate(1, 1),
            new NtsCoordinate(1, 0),
            new NtsCoordinate(0, 0)
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPolygon, polygon);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyPolygon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var polygon = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.7, 30.2),
            new NtsCoordinate(-97.7, 30.3),
            new NtsCoordinate(-97.6, 30.3),
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.2)
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyPolygon, polygon);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonPolygonGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(1, 2));

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPolygon, point);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyPolygon, point);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_Polygon_With_NonPolygon_PrimitiveKind()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var polygon = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(0, 1),
            new NtsCoordinate(1, 1),
            new NtsCoordinate(1, 0),
            new NtsCoordinate(0, 0)
        });

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPoint, polygon);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyLineString, polygon);
        bool result3 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiPolygon, polygon);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    public void Convert_GeometryPolygon_Returns_GeometryPolygon_With_Srid()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsPolygon = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0.0, 0.0),
            new NtsCoordinate(0.0, 1.0),
            new NtsCoordinate(1.0, 1.0),
            new NtsCoordinate(1.0, 0.0),
            new NtsCoordinate(0.0, 0.0)
        });
        ntsPolygon.SRID = 3857;

        IEdmPrimitiveTypeReference geometryEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryPolygon, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsPolygon, geometryEdmType);

        // Assert
        var gp = Assert.IsAssignableFrom<MsGeometryPolygon>(spatial);
        Assert.Equal(3857, gp.CoordinateSystem.EpsgId);
        Assert.False(gp.IsEmpty);
    }

    [Fact]
    public void Convert_GeographyPolygon_Returns_GeographyPolygon_With_Srid()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        // NTS uses (X=lon, Y=lat) but geography expects (lat,lon). Mapping is handled by converter.
        var ntsPolygon = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.7, 30.2),
            new NtsCoordinate(-97.7, 30.3),
            new NtsCoordinate(-97.6, 30.3),
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.2)
        });
        ntsPolygon.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyPolygon, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsPolygon, geographyEdmType);

        // Assert
        var gp = Assert.IsAssignableFrom<MsGeographyPolygon>(spatial);
        Assert.Equal(4326, gp.CoordinateSystem.EpsgId);
        Assert.False(gp.IsEmpty);
    }

    [Fact]
    public void Convert_EmptyPolygon_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsEmpty = gf.CreatePolygon(); // empty polygon
        ntsEmpty.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyPolygon, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsEmpty, geographyEdmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
