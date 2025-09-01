//-----------------------------------------------------------------------------
// <copyright file="LineStringConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyLineString = Microsoft.Spatial.GeographyLineString;
using MsGeographyPoint = Microsoft.Spatial.GeographyPoint;
using MsGeometryLineString = Microsoft.Spatial.GeometryLineString;
using MsGeometryPoint = Microsoft.Spatial.GeometryPoint;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class LineStringConverterTests
{
    private readonly LineStringConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryLineString()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var line = gf.CreateLineString(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(1, 1)
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryLineString, line);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyLineString()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var line = gf.CreateLineString(new[]
        {
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.3)
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyLineString, line);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonLineStringGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(1, 2));

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryLineString, point);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyLineString, point);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_LineString_With_NonLine_PrimitiveKind()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var line = gf.CreateLineString(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(1, 1)
        });

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPoint, line);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyPoint, line);
        bool result3 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPolygon, line);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    public void Convert_GeometryLineString_Returns_GeometryLineString_With_Srid_And_Points()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsLine = gf.CreateLineString(new[]
        {
            new NtsCoordinate(0.0, 0.0),
            new NtsCoordinate(1.0, 2.0),
            new NtsCoordinate(2.5, 3.5)
        });
        ntsLine.SRID = 3857;

        IEdmPrimitiveTypeReference geometryEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryLineString, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsLine, geometryEdmType);

        // Assert
        var gls = Assert.IsAssignableFrom<MsGeometryLineString>(spatial);
        Assert.Equal(3857, gls.CoordinateSystem.EpsgId);
        Assert.False(gls.IsEmpty);

        // Validate vertices (X,Y order)
        MsGeometryPoint[] pts = gls.Points.ToArray();
        Assert.Equal(3, pts.Length);
        Assert.Equal(0.0, pts[0].X, 6);
        Assert.Equal(0.0, pts[0].Y, 6);
        Assert.Equal(1.0, pts[1].X, 6);
        Assert.Equal(2.0, pts[1].Y, 6);
        Assert.Equal(2.5, pts[2].X, 6);
        Assert.Equal(3.5, pts[2].Y, 6);
    }

    [Fact]
    public void Convert_GeographyLineString_Returns_GeographyLineString_With_Srid_And_LatLon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        // X = lon, Y = lat in NTS; Geography expects (lat, lon)
        var ntsLine = gf.CreateLineString(new[]
        {
            new NtsCoordinate(-97.617134, 30.222296),
            new NtsCoordinate(-97.700000, 30.300000)
        });
        ntsLine.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyLineString, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsLine, geographyEdmType);

        // Assert
        var gls = Assert.IsAssignableFrom<MsGeographyLineString>(spatial);
        Assert.Equal(4326, gls.CoordinateSystem.EpsgId);
        Assert.False(gls.IsEmpty);

        // Validate vertices (lat, lon order)
        MsGeographyPoint[] pts = gls.Points.ToArray();
        Assert.Equal(2, pts.Length);
        Assert.Equal(30.222296, pts[0].Latitude, 6);
        Assert.Equal(-97.617134, pts[0].Longitude, 6);
        Assert.Equal(30.300000, pts[1].Latitude, 6);
        Assert.Equal(-97.700000, pts[1].Longitude, 6);
    }

    [Fact]
    public void Convert_EmptyLineString_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsEmpty = gf.CreateLineString(System.Array.Empty<NtsCoordinate>());
        ntsEmpty.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyLineString, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsEmpty, geographyEdmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
