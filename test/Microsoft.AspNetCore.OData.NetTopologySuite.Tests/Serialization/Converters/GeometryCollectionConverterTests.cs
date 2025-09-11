//-----------------------------------------------------------------------------
// <copyright file="GeometryCollectionConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyCollection = Microsoft.Spatial.GeographyCollection;
using MsGeographyLineString = Microsoft.Spatial.GeographyLineString;
using MsGeographyPoint = Microsoft.Spatial.GeographyPoint;
using MsGeometryCollection = Microsoft.Spatial.GeometryCollection;
using MsGeometryLineString = Microsoft.Spatial.GeometryLineString;
using MsGeometryPoint = Microsoft.Spatial.GeometryPoint;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class GeometryCollectionConverterTests
{
    private readonly GeometryCollectionConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryCollection_Primitive()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var gc = gf.CreateGeometryCollection(new NtsGeometry[]
        {
            gf.CreatePoint(new NtsCoordinate(0,0)),
            gf.CreateLineString(new[] { new NtsCoordinate(0,0), new NtsCoordinate(1,1) })
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryCollection, gc);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyCollection_Primitive()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var gc = gf.CreateGeometryCollection(new NtsGeometry[]
        {
            gf.CreatePoint(new NtsCoordinate(-97.6, 30.2)),
            gf.CreateLineString(new[] { new NtsCoordinate(-97.6,30.2), new NtsCoordinate(-97.7,30.3) })
        });

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyCollection, gc);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonCollectionGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(1, 2));

        // Act
        bool r1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryCollection, point);
        bool r2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyCollection, point);

        // Assert
        Assert.False(r1);
        Assert.False(r2);
    }

    [Fact]
    public void Convert_GeometryCollection_Returns_GeometryCollection_With_Srid_And_Items()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var gcNts = gf.CreateGeometryCollection(new NtsGeometry[]
        {
            gf.CreatePoint(new NtsCoordinate(0.0,0.0)),
            gf.CreateLineString(new[] { new NtsCoordinate(1.0,2.0), new NtsCoordinate(2.5,3.5) })
        });
        gcNts.SRID = 3857;

        IEdmPrimitiveTypeReference edmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryCollection, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(gcNts, edmType);

        // Assert
        var gcol = Assert.IsAssignableFrom<MsGeometryCollection>(spatial);
        Assert.Equal(3857, gcol.CoordinateSystem.EpsgId);
        Assert.False(gcol.IsEmpty);

        var items = gcol.Geometries.ToArray();
        Assert.Equal(2, items.Length);
        Assert.IsAssignableFrom<MsGeometryPoint>(items[0]);
        Assert.IsAssignableFrom<MsGeometryLineString>(items[1]);

        var p = (MsGeometryPoint)items[0];
        Assert.Equal(0.0, p.X, 6);
        Assert.Equal(0.0, p.Y, 6);

        var ls = (MsGeometryLineString)items[1];
        var pts = ls.Points.ToArray();
        Assert.Equal(2, pts.Length);
        Assert.Equal(1.0, pts[0].X, 6);
        Assert.Equal(2.0, pts[0].Y, 6);
        Assert.Equal(2.5, pts[1].X, 6);
        Assert.Equal(3.5, pts[1].Y, 6);
    }

    [Fact]
    public void Convert_GeographyCollection_Returns_GeographyCollection_With_Srid_And_Items()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var gcNts = gf.CreateGeometryCollection(new NtsGeometry[]
        {
            gf.CreatePoint(new NtsCoordinate(-97.617134, 30.222296)),
            gf.CreateLineString(new[]
            {
                new NtsCoordinate(-97.617134, 30.222296),
                new NtsCoordinate(-97.700000, 30.300000)
            })
        });
        gcNts.SRID = 4326;

        IEdmPrimitiveTypeReference edmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyCollection, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(gcNts, edmType);

        // Assert
        var gcol = Assert.IsAssignableFrom<MsGeographyCollection>(spatial);
        Assert.Equal(4326, gcol.CoordinateSystem.EpsgId);
        Assert.False(gcol.IsEmpty);

        var items = gcol.Geographies.ToArray();
        Assert.Equal(2, items.Length);
        Assert.IsAssignableFrom<MsGeographyPoint>(items[0]);
        Assert.IsAssignableFrom<MsGeographyLineString>(items[1]);

        var p = (MsGeographyPoint)items[0];
        Assert.Equal(30.222296, p.Latitude, 6);
        Assert.Equal(-97.617134, p.Longitude, 6);

        var ls = (MsGeographyLineString)items[1];
        var pts = ls.Points.ToArray();
        Assert.Equal(2, pts.Length);
        Assert.Equal(30.222296, pts[0].Latitude, 6);
        Assert.Equal(-97.617134, pts[0].Longitude, 6);
        Assert.Equal(30.300000, pts[1].Latitude, 6);
        Assert.Equal(-97.700000, pts[1].Longitude, 6);
    }

    [Fact]
    public void Convert_EmptyGeometryCollection_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var emptyGc = gf.CreateGeometryCollection(System.Array.Empty<NtsGeometry>());
        emptyGc.SRID = 4326;

        IEdmPrimitiveTypeReference edmType =
            EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyCollection, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(emptyGc, edmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
