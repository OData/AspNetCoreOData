//-----------------------------------------------------------------------------
// <copyright file="MultiPointConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyMultiPoint = Microsoft.Spatial.GeographyMultiPoint;
using MsGeometryMultiPoint = Microsoft.Spatial.GeometryMultiPoint;
using MsGeographyPoint = Microsoft.Spatial.GeographyPoint;
using MsGeometryPoint = Microsoft.Spatial.GeometryPoint;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class MultiPointConverterTests
{
    private readonly MultiPointConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryMultiPoint()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var mp = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(1, 1)
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiPoint, mp);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyMultiPoint()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var mp = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.3)
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiPoint, mp);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonMultiPointGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(1, 2));

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiPoint, point);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiPoint, point);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_MultiPoint_With_NonMultiPoint_PrimitiveKind()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var mp = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(1, 1)
        });

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPoint, mp);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyLineString, mp);
        bool result3 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiLineString, mp);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    public void Convert_GeometryMultiPoint_Returns_GeometryMultiPoint_With_Srid_And_Points()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsMultiPoint = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(0.0, 0.0),
            new NtsCoordinate(1.0, 2.0),
            new NtsCoordinate(2.5, 3.5)
        });
        ntsMultiPoint.SRID = 3857;

        IEdmPrimitiveTypeReference geometryEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryMultiPoint, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsMultiPoint, geometryEdmType);

        // Assert
        var gmp = Assert.IsAssignableFrom<MsGeometryMultiPoint>(spatial);
        Assert.Equal(3857, gmp.CoordinateSystem.EpsgId);
        Assert.False(gmp.IsEmpty);

        // Validate vertices (X,Y order)
        MsGeometryPoint[] pts = gmp.Points.ToArray();
        Assert.Equal(3, pts.Length);
        Assert.Equal(0.0, pts[0].X, 6);
        Assert.Equal(0.0, pts[0].Y, 6);
        Assert.Equal(1.0, pts[1].X, 6);
        Assert.Equal(2.0, pts[1].Y, 6);
        Assert.Equal(2.5, pts[2].X, 6);
        Assert.Equal(3.5, pts[2].Y, 6);
    }

    [Fact]
    public void Convert_GeographyMultiPoint_Returns_GeographyMultiPoint_With_Srid_And_LatLon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsMultiPoint = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(-97.617134, 30.222296),
            new NtsCoordinate(-97.700000, 30.300000)
        });
        ntsMultiPoint.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyMultiPoint, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsMultiPoint, geographyEdmType);

        // Assert
        var gmp = Assert.IsAssignableFrom<MsGeographyMultiPoint>(spatial);
        Assert.Equal(4326, gmp.CoordinateSystem.EpsgId);
        Assert.False(gmp.IsEmpty);

        // Validate vertices (lat, lon order)
        MsGeographyPoint[] pts = gmp.Points.ToArray();
        Assert.Equal(2, pts.Length);
        Assert.Equal(30.222296, pts[0].Latitude, 6);
        Assert.Equal(-97.617134, pts[0].Longitude, 6);
        Assert.Equal(30.300000, pts[1].Latitude, 6);
        Assert.Equal(-97.700000, pts[1].Longitude, 6);
    }

    [Fact]
    public void Convert_EmptyMultiPoint_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsEmpty = gf.CreateMultiPoint(); // empty multipoint
        ntsEmpty.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyMultiPoint, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsEmpty, geographyEdmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
