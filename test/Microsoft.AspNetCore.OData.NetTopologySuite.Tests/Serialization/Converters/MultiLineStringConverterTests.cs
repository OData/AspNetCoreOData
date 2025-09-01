//-----------------------------------------------------------------------------
// <copyright file="MultiLineStringConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyMultiLineString = Microsoft.Spatial.GeographyMultiLineString;
using MsGeographyPoint = Microsoft.Spatial.GeographyPoint;
using MsGeometryMultiLineString = Microsoft.Spatial.GeometryMultiLineString;
using MsGeometryPoint = Microsoft.Spatial.GeometryPoint;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using NtsLineString = NetTopologySuite.Geometries.LineString;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class MultiLineStringConverterTests
{
    private readonly MultiLineStringConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryMultiLineString()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ls1 = gf.CreateLineString(new[] { new NtsCoordinate(0, 0), new NtsCoordinate(1, 1) });
        var mls = gf.CreateMultiLineString(new[] { ls1 });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiLineString, mls);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyMultiLineString()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ls1 = gf.CreateLineString(new[] { new NtsCoordinate(-97.6, 30.2), new NtsCoordinate(-97.7, 30.3) });
        var mls = gf.CreateMultiLineString(new[] { ls1 });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiLineString, mls);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonMultiLineStringGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ls = gf.CreateLineString(new[] { new NtsCoordinate(0, 0), new NtsCoordinate(1, 1) });

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiLineString, ls);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiLineString, ls);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_MultiLineString_With_NonMultiLine_PrimitiveKind()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ls1 = gf.CreateLineString(new[] { new NtsCoordinate(0, 0), new NtsCoordinate(1, 1) });
        var mls = gf.CreateMultiLineString(new[] { ls1 });

        // Act
        bool r1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryLineString, mls);
        bool r2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyLineString, mls);
        bool r3 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiPoint, mls);
        bool r4 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiPolygon, mls);

        // Assert
        Assert.False(r1);
        Assert.False(r2);
        Assert.False(r3);
        Assert.False(r4);
    }

    [Fact]
    public void Convert_GeometryMultiLineString_Returns_GeometryMultiLineString_With_Srid_And_Points()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ls1 = gf.CreateLineString(new[]
        {
            new NtsCoordinate(0.0, 0.0),
            new NtsCoordinate(1.0, 2.0)
        });
        var ls2 = gf.CreateLineString(new[]
        {
            new NtsCoordinate(2.5, 3.5),
            new NtsCoordinate(4.5, 5.5)
        });
        var ntsMls = gf.CreateMultiLineString(new NtsLineString[] { ls1, ls2 });
        ntsMls.SRID = 3857;

        IEdmPrimitiveTypeReference edmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryMultiLineString, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsMls, edmType);

        // Assert
        var gmls = Assert.IsAssignableFrom<MsGeometryMultiLineString>(spatial);
        Assert.Equal(3857, gmls.CoordinateSystem.EpsgId);
        Assert.False(gmls.IsEmpty);

        var lines = gmls.LineStrings.ToArray();
        Assert.Equal(2, lines.Length);

        // Line 1 (X,Y order)
        MsGeometryPoint[] l1pts = lines[0].Points.ToArray();
        Assert.Equal(2, l1pts.Length);
        Assert.Equal(0.0, l1pts[0].X, 6);
        Assert.Equal(0.0, l1pts[0].Y, 6);
        Assert.Equal(1.0, l1pts[1].X, 6);
        Assert.Equal(2.0, l1pts[1].Y, 6);

        // Line 2 (X,Y order)
        MsGeometryPoint[] l2pts = lines[1].Points.ToArray();
        Assert.Equal(2, l2pts.Length);
        Assert.Equal(2.5, l2pts[0].X, 6);
        Assert.Equal(3.5, l2pts[0].Y, 6);
        Assert.Equal(4.5, l2pts[1].X, 6);
        Assert.Equal(5.5, l2pts[1].Y, 6);
    }

    [Fact]
    public void Convert_GeographyMultiLineString_Returns_GeographyMultiLineString_With_Srid_And_LatLon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        // NTS uses (X=lon, Y=lat); Geography expects (lat,lon)
        var ls1 = gf.CreateLineString(new[]
        {
            new NtsCoordinate(-97.617134, 30.222296),
            new NtsCoordinate(-97.700000, 30.300000)
        });
        var ls2 = gf.CreateLineString(new[]
        {
            new NtsCoordinate(-97.800000, 30.400000),
            new NtsCoordinate(-97.900000, 30.500000)
        });
        var ntsMls = gf.CreateMultiLineString(new NtsLineString[] { ls1, ls2 });
        ntsMls.SRID = 4326;

        IEdmPrimitiveTypeReference edmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyMultiLineString, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsMls, edmType);

        // Assert
        var gmls = Assert.IsAssignableFrom<MsGeographyMultiLineString>(spatial);
        Assert.Equal(4326, gmls.CoordinateSystem.EpsgId);
        Assert.False(gmls.IsEmpty);

        var lines = gmls.LineStrings.ToArray();
        Assert.Equal(2, lines.Length);

        // Line 1 (lat, lon order)
        MsGeographyPoint[] l1pts = lines[0].Points.ToArray();
        Assert.Equal(2, l1pts.Length);
        Assert.Equal(30.222296, l1pts[0].Latitude, 6);
        Assert.Equal(-97.617134, l1pts[0].Longitude, 6);
        Assert.Equal(30.300000, l1pts[1].Latitude, 6);
        Assert.Equal(-97.700000, l1pts[1].Longitude, 6);

        // Line 2 (lat, lon order)
        MsGeographyPoint[] l2pts = lines[1].Points.ToArray();
        Assert.Equal(2, l2pts.Length);
        Assert.Equal(30.400000, l2pts[0].Latitude, 6);
        Assert.Equal(-97.800000, l2pts[0].Longitude, 6);
        Assert.Equal(30.500000, l2pts[1].Latitude, 6);
        Assert.Equal(-97.900000, l2pts[1].Longitude, 6);
    }

    [Fact]
    public void Convert_EmptyMultiLineString_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsEmpty = gf.CreateMultiLineString(System.Array.Empty<NtsLineString>());
        ntsEmpty.SRID = 4326;

        IEdmPrimitiveTypeReference edmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyMultiLineString, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsEmpty, edmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
