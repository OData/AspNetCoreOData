//-----------------------------------------------------------------------------
// <copyright file="SpatialConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyCollection = Microsoft.Spatial.GeographyCollection;
using MsGeographyLineString = Microsoft.Spatial.GeographyLineString;
using MsGeographyMultiLineString = Microsoft.Spatial.GeographyMultiLineString;
using MsGeographyMultiPoint = Microsoft.Spatial.GeographyMultiPoint;
using MsGeographyMultiPolygon = Microsoft.Spatial.GeographyMultiPolygon;
using MsGeographyPoint = Microsoft.Spatial.GeographyPoint;
using MsGeographyPolygon = Microsoft.Spatial.GeographyPolygon;
using MsGeometryCollection = Microsoft.Spatial.GeometryCollection;
using MsGeometryLineString = Microsoft.Spatial.GeometryLineString;
using MsGeometryMultiLineString = Microsoft.Spatial.GeometryMultiLineString;
using MsGeometryMultiPoint = Microsoft.Spatial.GeometryMultiPoint;
using MsGeometryMultiPolygon = Microsoft.Spatial.GeometryMultiPolygon;
using MsGeometryPoint = Microsoft.Spatial.GeometryPoint;
using MsGeometryPolygon = Microsoft.Spatial.GeometryPolygon;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using NtsLineString = NetTopologySuite.Geometries.LineString;
using NtsMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using NtsMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using NtsMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using NtsPoint = NetTopologySuite.Geometries.Point;
using NtsPolygon = NetTopologySuite.Geometries.Polygon;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class SpatialConverterTests
{
    private static IEdmPrimitiveTypeReference Edm(EdmPrimitiveTypeKind kind) =>
        EdmCoreModel.Instance.GetPrimitive(kind, isNullable: false);

    [Fact]
    public void Convert_Geometry_Point_Maps_XY_And_Srid()
    {
        var gf = NtsGeometryFactory.Default;
        NtsPoint nts = gf.CreatePoint(new NtsCoordinate(12.34, 56.78));
        nts.SRID = 3857;

        var converter = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryPoint), nts.SRID);

        ISpatial spatial = converter.Convert(nts);

        var gp = Assert.IsAssignableFrom<MsGeometryPoint>(spatial);
        Assert.Equal(12.34, gp.X, 6);
        Assert.Equal(56.78, gp.Y, 6);
        Assert.Equal(3857, gp.CoordinateSystem.EpsgId);
        Assert.False(gp.IsEmpty);
    }

    [Fact]
    public void Convert_Geography_Point_Maps_LatLon_And_Srid()
    {
        var gf = NtsGeometryFactory.Default;
        NtsPoint nts = gf.CreatePoint(new NtsCoordinate(-97.617134, 30.222296));
        nts.SRID = 4326;

        var converter = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyPoint), nts.SRID);

        ISpatial spatial = converter.Convert(nts);

        var gp = Assert.IsAssignableFrom<MsGeographyPoint>(spatial);
        Assert.Equal(30.222296, gp.Latitude, 6);
        Assert.Equal(-97.617134, gp.Longitude, 6);
        Assert.Equal(4326, gp.CoordinateSystem.EpsgId);
        Assert.False(gp.IsEmpty);
    }

    [Fact]
    public void Convert_LineString_Geometry_Maps_XY()
    {
        var gf = NtsGeometryFactory.Default;
        NtsLineString nts = gf.CreateLineString(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(1, 2),
            new NtsCoordinate(2.5, 3.5)
        });
        nts.SRID = 0;

        var converter = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryLineString), nts.SRID);

        ISpatial spatial = converter.Convert(nts);

        var ls = Assert.IsAssignableFrom<MsGeometryLineString>(spatial);
        Assert.False(ls.IsEmpty);
        var pts = ls.Points.ToArray();
        Assert.Equal(3, pts.Length);
        Assert.Equal(0, pts[0].X, 6); Assert.Equal(0, pts[0].Y, 6);
        Assert.Equal(1, pts[1].X, 6); Assert.Equal(2, pts[1].Y, 6);
        Assert.Equal(2.5, pts[2].X, 6); Assert.Equal(3.5, pts[2].Y, 6);
    }

    [Fact]
    public void Convert_LineString_Geography_Maps_LatLon()
    {
        var gf = NtsGeometryFactory.Default;
        NtsLineString nts = gf.CreateLineString(new[]
        {
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.3)
        });
        nts.SRID = 4326;

        var converter = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyLineString), nts.SRID);

        ISpatial spatial = converter.Convert(nts);

        var ls = Assert.IsAssignableFrom<MsGeographyLineString>(spatial);
        var pts = ls.Points.ToArray();
        Assert.Equal(2, pts.Length);
        Assert.Equal(30.2, pts[0].Latitude, 6); Assert.Equal(-97.6, pts[0].Longitude, 6);
        Assert.Equal(30.3, pts[1].Latitude, 6); Assert.Equal(-97.7, pts[1].Longitude, 6);
    }

    [Fact]
    public void Convert_Polygon_Geometry_Simple_Shell()
    {
        var gf = NtsGeometryFactory.Default;
        NtsPolygon nts = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(0, 1),
            new NtsCoordinate(1, 1),
            new NtsCoordinate(1, 0),
            new NtsCoordinate(0, 0)
        });
        nts.SRID = 3857;

        var converter = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryPolygon), nts.SRID);

        ISpatial spatial = converter.Convert(nts);

        var p = Assert.IsAssignableFrom<MsGeometryPolygon>(spatial);
        Assert.Equal(3857, p.CoordinateSystem.EpsgId);
        Assert.False(p.IsEmpty);
    }

    [Fact]
    public void Convert_Polygon_Geography_Simple_Shell()
    {
        var gf = NtsGeometryFactory.Default;
        NtsPolygon nts = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.7, 30.2),
            new NtsCoordinate(-97.7, 30.3),
            new NtsCoordinate(-97.6, 30.3),
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.2)
        });
        nts.SRID = 4326;

        var converter = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyPolygon), nts.SRID);

        ISpatial spatial = converter.Convert(nts);

        var p = Assert.IsAssignableFrom<MsGeographyPolygon>(spatial);
        Assert.Equal(4326, p.CoordinateSystem.EpsgId);
        Assert.False(p.IsEmpty);
    }

    [Fact]
    public void Convert_MultiPoint_Geometry_And_Geography()
    {
        var gf = NtsGeometryFactory.Default;
        NtsMultiPoint ntsGeom = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(0, 0),
            new NtsCoordinate(1, 2)
        });
        ntsGeom.SRID = 0;

        NtsMultiPoint ntsGeog = gf.CreateMultiPointFromCoords(new[]
        {
            new NtsCoordinate(-97.6, 30.2),
            new NtsCoordinate(-97.7, 30.3)
        });
        ntsGeog.SRID = 4326;

        var converterGeom = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryMultiPoint), ntsGeom.SRID);
        var converterGeog = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyMultiPoint), ntsGeog.SRID);

        var gmp = Assert.IsAssignableFrom<MsGeometryMultiPoint>(converterGeom.Convert(ntsGeom));
        var ggp = Assert.IsAssignableFrom<MsGeographyMultiPoint>(converterGeog.Convert(ntsGeog));

        var gPts = gmp.Points.ToArray();
        Assert.Equal(2, gPts.Length);
        Assert.Equal(0, gPts[0].X, 6); Assert.Equal(0, gPts[0].Y, 6);
        Assert.Equal(1, gPts[1].X, 6); Assert.Equal(2, gPts[1].Y, 6);

        var ggPts = ggp.Points.ToArray();
        Assert.Equal(2, ggPts.Length);
        Assert.Equal(30.2, ggPts[0].Latitude, 6); Assert.Equal(-97.6, ggPts[0].Longitude, 6);
        Assert.Equal(30.3, ggPts[1].Latitude, 6); Assert.Equal(-97.7, ggPts[1].Longitude, 6);
    }

    [Fact]
    public void Convert_MultiLineString_Geometry_And_Geography()
    {
        var gf = NtsGeometryFactory.Default;
        NtsLineString ls1 = gf.CreateLineString(new[] { new NtsCoordinate(0, 0), new NtsCoordinate(1, 1) });
        NtsLineString ls2 = gf.CreateLineString(new[] { new NtsCoordinate(2, 2), new NtsCoordinate(3, 3) });
        NtsMultiLineString ntsGeom = gf.CreateMultiLineString(new[] { ls1, ls2 });
        ntsGeom.SRID = 3857;

        NtsLineString gls1 = gf.CreateLineString(new[] { new NtsCoordinate(-97.6, 30.2), new NtsCoordinate(-97.7, 30.3) });
        NtsLineString gls2 = gf.CreateLineString(new[] { new NtsCoordinate(-97.8, 30.4), new NtsCoordinate(-97.9, 30.5) });
        NtsMultiLineString ntsGeog = gf.CreateMultiLineString(new[] { gls1, gls2 });
        ntsGeog.SRID = 4326;

        var converterGeom = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryMultiLineString), ntsGeom.SRID);
        var converterGeog = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyMultiLineString), ntsGeog.SRID);

        var gmls = Assert.IsAssignableFrom<MsGeometryMultiLineString>(converterGeom.Convert(ntsGeom));
        var ggls = Assert.IsAssignableFrom<MsGeographyMultiLineString>(converterGeog.Convert(ntsGeog));

        Assert.Equal(2, gmls.LineStrings.Count());
        Assert.Equal(2, ggls.LineStrings.Count());
    }

    [Fact]
    public void Convert_MultiPolygon_Geometry_And_Geography()
    {
        var gf = NtsGeometryFactory.Default;

        NtsPolygon p1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(0,0), new NtsCoordinate(0,1), new NtsCoordinate(1,1), new NtsCoordinate(1,0), new NtsCoordinate(0,0)
        });
        NtsPolygon p2 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(2,2), new NtsCoordinate(2,3), new NtsCoordinate(3,3), new NtsCoordinate(3,2), new NtsCoordinate(2,2)
        });
        NtsMultiPolygon ntsGeom = gf.CreateMultiPolygon(new[] { p1, p2 });
        ntsGeom.SRID = 3857;

        NtsPolygon gp1 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.7,30.2), new NtsCoordinate(-97.7,30.3), new NtsCoordinate(-97.6,30.3),
            new NtsCoordinate(-97.6,30.2), new NtsCoordinate(-97.7,30.2)
        });
        NtsPolygon gp2 = gf.CreatePolygon(new[]
        {
            new NtsCoordinate(-97.5,30.1), new NtsCoordinate(-97.5,30.15), new NtsCoordinate(-97.45,30.15),
            new NtsCoordinate(-97.45,30.1), new NtsCoordinate(-97.5,30.1)
        });
        NtsMultiPolygon ntsGeog = gf.CreateMultiPolygon(new[] { gp1, gp2 });
        ntsGeog.SRID = 4326;

        var converterGeom = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryMultiPolygon), ntsGeom.SRID);
        var converterGeog = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyMultiPolygon), ntsGeog.SRID);

        var gmp = Assert.IsAssignableFrom<MsGeometryMultiPolygon>(converterGeom.Convert(ntsGeom));
        var ggp = Assert.IsAssignableFrom<MsGeographyMultiPolygon>(converterGeog.Convert(ntsGeog));

        Assert.Equal(2, gmp.Polygons.Count());
        Assert.Equal(2, ggp.Polygons.Count());
    }

    [Fact]
    public void Convert_GeometryCollection_Geometry_And_Geography_Mixed_Nested()
    {
        var gf = NtsGeometryFactory.Default;

        // Geometry collection with nested collection
        NtsPoint p = gf.CreatePoint(new NtsCoordinate(0, 0));
        NtsLineString ls = gf.CreateLineString(new[] { new NtsCoordinate(1, 2), new NtsCoordinate(2, 3) });
        NtsGeometryCollection nested = gf.CreateGeometryCollection(new NtsGeometry[] { gf.CreatePoint(new NtsCoordinate(5, 5)) });
        NtsGeometryCollection ntsGeomCol = gf.CreateGeometryCollection(new NtsGeometry[] { p, ls, nested });
        ntsGeomCol.SRID = 0;

        var convGeom = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeometryCollection), ntsGeomCol.SRID);
        var resultGeom = Assert.IsAssignableFrom<MsGeometryCollection>(convGeom.Convert(ntsGeomCol));
        var itemsG = resultGeom.Geometries.ToArray();
        Assert.Equal(3, itemsG.Length);
        Assert.IsAssignableFrom<MsGeometryPoint>(itemsG[0]);
        Assert.IsAssignableFrom<MsGeometryLineString>(itemsG[1]);
        Assert.IsAssignableFrom<MsGeometryCollection>(itemsG[2]);

        // Geography collection
        NtsPoint gp = gf.CreatePoint(new NtsCoordinate(-97.617134, 30.222296));
        NtsLineString gls = gf.CreateLineString(new[] {
            new NtsCoordinate(-97.617134, 30.222296), new NtsCoordinate(-97.7, 30.3)
        });
        NtsGeometryCollection gNested = gf.CreateGeometryCollection(new NtsGeometry[] { gf.CreatePoint(new NtsCoordinate(-97.8, 30.4)) });
        NtsGeometryCollection ntsGeogCol = gf.CreateGeometryCollection(new NtsGeometry[] { gp, gls, gNested });
        ntsGeogCol.SRID = 4326;

        var convGeog = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyCollection), ntsGeogCol.SRID);
        var resultGeog = Assert.IsAssignableFrom<MsGeographyCollection>(convGeog.Convert(ntsGeogCol));
        var itemsGeog = resultGeog.Geographies.ToArray();
        Assert.Equal(3, itemsGeog.Length);
        Assert.IsAssignableFrom<MsGeographyPoint>(itemsGeog[0]);
        Assert.IsAssignableFrom<MsGeographyLineString>(itemsGeog[1]);
        Assert.IsAssignableFrom<MsGeographyCollection>(itemsGeog[2]);
    }

    [Fact]
    public void Convert_Empty_Shapes_Return_Empty()
    {
        var gf = NtsGeometryFactory.Default;

        // Empty point
        NtsPoint ep = gf.CreatePoint();
        ep.SRID = 4326;
        var convP = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyPoint), ep.SRID);
        Assert.True(convP.Convert(ep).IsEmpty);

        // Empty line string
        NtsLineString els = gf.CreateLineString(Array.Empty<NtsCoordinate>());
        els.SRID = 4326;
        var convLs = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyLineString), els.SRID);
        Assert.True(convLs.Convert(els).IsEmpty);

        // Empty polygon
        NtsPolygon epoly = gf.CreatePolygon();
        epoly.SRID = 4326;
        var convPoly = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyPolygon), epoly.SRID);
        Assert.True(convPoly.Convert(epoly).IsEmpty);

        // Empty geometry collection
        NtsGeometryCollection egc = gf.CreateGeometryCollection(Array.Empty<NtsGeometry>());
        egc.SRID = 4326;
        var convGc = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyCollection), egc.SRID);
        Assert.True(convGc.Convert(egc).IsEmpty);
    }

    [Fact]
    public void Convert_NullGeometry_Throws_ArgumentNull()
    {
        var conv = SpatialConverter.For(Edm(EdmPrimitiveTypeKind.GeographyPoint), 4326);
        Assert.Throws<ArgumentNullException>(() => conv.Convert((NtsGeometry)null));
    }

    [Fact]
    public void For_NullPrimitiveType_Throws_ArgumentNullException()
    {
        // Act
        var ex = Assert.Throws<ArgumentNullException>(() => SpatialConverter.For((IEdmPrimitiveTypeReference)null!, 4326));

        // Assert
        Assert.Equal("primitiveType", ex.ParamName);
    }

    [Fact]
    public void For_NonSpatialPrimitive_Throws_InvalidOperationException()
    {
        // Arrange: use a non-spatial EDM primitive kind (e.g., String)
        IEdmPrimitiveTypeReference nonSpatial = EdmCoreModel.Instance.GetString(isNullable: false);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => SpatialConverter.For(nonSpatial, 0));
    }
}
