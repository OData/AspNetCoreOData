//-----------------------------------------------------------------------------
// <copyright file="PointConverterTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyPoint = Microsoft.Spatial.GeographyPoint;
using MsGeometryPoint = Microsoft.Spatial.GeometryPoint;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class PointConverterTests
{
    private readonly PointConverter _converter = new();

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeometryPoint()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(10, 20));

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPoint, point);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrue_For_GeographyPoint()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(-97.617134, 30.222296));

        // Act
        bool result = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyPoint, point);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_NonPointGeometry()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var line = gf.CreateLineString(new[]
        {
            new NtsCoordinate(0,0),
            new NtsCoordinate(1,1)
        });

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryPoint, line);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.GeographyPoint, line);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
    }

    [Fact]
    public void CanConvert_ReturnsFalse_For_Point_With_NonPoint_PrimitiveKind()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var point = gf.CreatePoint(new NtsCoordinate(1, 2));

        // Act
        bool result1 = _converter.CanConvert(EdmPrimitiveTypeKind.Geometry, point);
        bool result2 = _converter.CanConvert(EdmPrimitiveTypeKind.Geography, point);
        bool result3 = _converter.CanConvert(EdmPrimitiveTypeKind.GeometryLineString, point);

        // Assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
    }

    [Fact]
    public void Convert_GeometryPoint_Returns_GeometryPoint_With_XY_And_Srid()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsPoint = gf.CreatePoint(new NtsCoordinate(12.34, 56.78));
        ntsPoint.SRID = 3857; // arbitrary non-default SRID for geometry

        IEdmPrimitiveTypeReference geometryEdmType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryPoint, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsPoint, geometryEdmType);

        // Assert
        var gp = Assert.IsAssignableFrom<MsGeometryPoint>(spatial);
        Assert.Equal(12.34, gp.X, 5);
        Assert.Equal(56.78, gp.Y, 5);
        Assert.Equal(3857, gp.CoordinateSystem.EpsgId);
        Assert.False(gp.IsEmpty);
    }

    [Fact]
    public void Convert_GeographyPoint_Returns_GeographyPoint_With_LatLon_And_Srid()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsPoint = gf.CreatePoint(new NtsCoordinate(-97.617134, 30.222296)); // X = lon, Y = lat
        ntsPoint.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyPoint, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsPoint, geographyEdmType);

        // Assert
        var gp = Assert.IsAssignableFrom<MsGeographyPoint>(spatial);
        Assert.Equal(30.222296, gp.Latitude, 6);   // Y -> Latitude
        Assert.Equal(-97.617134, gp.Longitude, 6); // X -> Longitude
        Assert.Equal(4326, gp.CoordinateSystem.EpsgId);
        Assert.False(gp.IsEmpty);
    }

    [Fact]
    public void Convert_EmptyPoint_IfSupported_Returns_Empty()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        var ntsEmptyPoint = gf.CreatePoint(); // empty point
        ntsEmptyPoint.SRID = 4326;

        IEdmPrimitiveTypeReference geographyEdmType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyPoint, isNullable: false);

        // Act
        ISpatial spatial = _converter.Convert(ntsEmptyPoint, geographyEdmType);

        // Assert
        Assert.True(spatial.IsEmpty);
    }
}
