//-----------------------------------------------------------------------------
// <copyright file="MultiPolygonConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyMultiPolygon = Microsoft.Spatial.GeographyMultiPolygon;
using MsGeographyPolygon = Microsoft.Spatial.GeographyPolygon;
using MsGeometryMultiPolygon = Microsoft.Spatial.GeometryMultiPolygon;
using MsGeometryPolygon = Microsoft.Spatial.GeometryPolygon;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using NtsPolygon = NetTopologySuite.Geometries.Polygon;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class MultiPolygonConverterTests
{
    private readonly MultiPolygonConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryMultiPolygon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var p1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(0, 1),
            new NtsCoordinate(1, 1),
            new NtsCoordinate(1, 0),
            new NtsCoordinate(0, 0)
        });
        var mp = gf.CreateMultiPolygon(new NtsPolygon[] { p1 });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiPolygon, mp);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyMultiPolygon()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var p1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.7, 30.2),
            new NtsCoordinate(-97.7, 30.3),
            new NtsCoordinate(-97.6, 30.3),
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.2)
        });
        var mp = gf.CreateMultiPolygon(new NtsPolygon[] { p1 });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiPolygon, mp);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonMultiPolygonGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var singlePolygon = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(0, 1),
            new NtsCoordinate(1, 1),
            new NtsCoordinate(1, 0),
            new NtsCoordinate(0, 0)
        });

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiPolygon, singlePolygon);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyMultiPolygon, singlePolygon);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_MultiPolygon_With_NonMultiPolygon_PrimitiveKind()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var p1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(0, 1),
            new NtsCoordinate(1, 1),
            new NtsCoordinate(1, 0),
            new NtsCoordinate(0, 0)
        });
        var mp = gf.CreateMultiPolygon(new NtsPolygon[] { p1 });

        // Act
        bool r1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPolygon, mp);
        bool r2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyLineString, mp);
        bool r3 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryMultiLineString, mp);

        // Assert
        Assert.False(r1);
        Assert.False(r2);
        Assert.False(r3);
    }

    [Fact]
    public void Convert_GeometryMultiPolygon_Returns_GeometryMultiPolygon_With_Srid_And_Polygons()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;

        var p1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0.0, 0.0),
            new NtsCoordinate(0.0, 1.0),
            new NtsCoordinate(1.0, 1.0),
            new NtsCoordinate(1.0, 0.0),
            new NtsCoordinate(0.0, 0.0)
        });

        var p2 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(2.0, 2.0),
            new NtsCoordinate(2.0, 3.0),
            new NtsCoordinate(3.0, 3.0),
            new NtsCoordinate(3.0, 2.0),
            new NtsCoordinate(2.0, 2.0)
        });

        var ntsMp = gf.CreateMultiPolygon(new NtsPolygon[] { p1, p2 });
        ntsMp.SRID = 3857;

        IEdmPrimitiveTypeReference geometryEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryMultiPolygon, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsMp, geometryEdmType);

        // Assert
        var gmp = Assert.IsAssignableFrom<MsGeometryMultiPolygon>(spatial);
        Assert.Equal(3857, gmp.CoordinateSystem.EpsgId);
        Assert.False(gmp.IsEmpty);

        MsGeometryPolygon[] polys = gmp.Polygons.ToArray();
        Assert.Equal(2, polys.Length);
    }

    [Fact]
    public void Convert_GeographyMultiPolygon_Returns_GeographyMultiPolygon_With_Srid_And_Polygons()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;

        var p1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.7, 30.2),
            new NtsCoordinate(-97.7, 30.3),
            new NtsCoordinate(-97.6, 30.3),
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.2)
        });

        var p2 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.5, 30.1),
            new NtsCoordinate(-97.5, 30.15),
            new NtsCoordinate(-97.45, 30.15),
            new NtsCoordinate(-97.45, 30.1),
            new NtsCoordinate(-97.5, 30.1)
        });

        var ntsMp = gf.CreateMultiPolygon(new NtsPolygon[] { p1, p2 });
        ntsMp.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyMultiPolygon, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsMp, geographyEdmType);

        // Assert
        var gmp = Assert.IsAssignableFrom<MsGeographyMultiPolygon>(spatial);
        Assert.Equal(4326, gmp.CoordinateSystem.EpsgId);
        Assert.False(gmp.IsEmpty);

        MsGeographyPolygon[] polys = gmp.Polygons.ToArray();
        Assert.Equal(2, polys.Length);
    }

    [Fact]
    public void Convert_EmptyMultiPolygon_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsEmpty = gf.CreateMultiPolygon(); // empty multipolygon
        ntsEmpty.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyMultiPolygon, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsEmpty, geographyEdmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
