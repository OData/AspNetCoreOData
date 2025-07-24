//-----------------------------------------------------------------------------
// <copyright file="SpatialConverterRegistryTests.cs" company=".NET Foundation">
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
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using NtsPoint = NetTopologySuite.Geometries.Point;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Converters;

public class SpatialConverterRegistryTests
{
    [Fact]
    public void Convert_UsesFirstMatchingConverter_InRegistrationOrder()
    {
        // Arrange: two converters both claim they can convert; first should be used.
        var gf = NtsGeometryFactory.Default;
        NtsPoint point = gf.CreatePoint(new NtsCoordinate(1, 2));
        point.SRID = 0;

        var edmType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryPoint, isNullable: false);

        var first = new CountingConverter(canConvert: true, resultFactory: () => new DummySpatial(CoordinateSystem.DefaultGeometry));
        var second = new CountingConverter(canConvert: true, resultFactory: () => new DummySpatial(CoordinateSystem.DefaultGeometry));

        var registry = new SpatialConverterRegistry(new ISpatialConverter[] { first, second });

        // Act
        ISpatial result = registry.Convert(point, edmType);

        // Assert
        Assert.IsType<DummySpatial>(result);
        Assert.Equal(1, first.ConvertCalls);
        Assert.Equal(0, second.ConvertCalls);
        Assert.True(first.CanConvertCalls >= 1); // called at least once
    }

    [Fact]
    public void Convert_Throws_InvalidOperation_When_NoConverterMatches()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        NtsPoint point = gf.CreatePoint(new NtsCoordinate(1, 2));
        var edmType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryLineString, isNullable: false); // mismatch on purpose

        var never = new CountingConverter(canConvert: false, resultFactory: () => new DummySpatial(CoordinateSystem.DefaultGeometry));
        var registry = new SpatialConverterRegistry(new ISpatialConverter[] { never });

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => registry.Convert(point, edmType));
        Assert.Contains(typeof(NtsPoint).FullName!, ex.Message);
    }

    [Fact]
    public void Convert_Delegates_To_PointConverter_For_GeometryPoint()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        NtsPoint ntsPoint = gf.CreatePoint(new NtsCoordinate(12.34, 56.78));
        ntsPoint.SRID = 3857;

        IEdmPrimitiveTypeReference edm = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryPoint, isNullable: false);

        var registry = new SpatialConverterRegistry(new ISpatialConverter[]
        {
            new LineStringConverter(), // non-matching first
            new PointConverter()
        });

        // Act
        ISpatial spatial = registry.Convert(ntsPoint, edm);

        // Assert
        var gp = Assert.IsAssignableFrom<MsGeometryPoint>(spatial);
        Assert.Equal(12.34, gp.X, 6);
        Assert.Equal(56.78, gp.Y, 6);
        Assert.Equal(3857, gp.CoordinateSystem.EpsgId);
    }

    [Fact]
    public void Convert_Delegates_To_PointConverter_For_GeographyPoint()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        NtsPoint ntsPoint = gf.CreatePoint(new NtsCoordinate(-97.617134, 30.222296));
        ntsPoint.SRID = 4326;

        IEdmPrimitiveTypeReference edm = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeographyPoint, isNullable: false);

        var registry = new SpatialConverterRegistry(new ISpatialConverter[]
        {
            new LineStringConverter(), // non-matching first
            new PointConverter()
        });

        // Act
        ISpatial spatial = registry.Convert(ntsPoint, edm);

        // Assert
        var gp = Assert.IsAssignableFrom<MsGeographyPoint>(spatial);
        Assert.Equal(30.222296, gp.Latitude, 6);
        Assert.Equal(-97.617134, gp.Longitude, 6);
        Assert.Equal(4326, gp.CoordinateSystem.EpsgId);
    }

    [Fact]
    public void Convert_Chooses_Correct_Converter_Among_Many()
    {
        // Arrange
        var gf = NtsGeometryFactory.Default;
        NtsPoint point = gf.CreatePoint(new NtsCoordinate(0, 0));
        point.SRID = 0;

        var edmGeomPoint = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.GeometryPoint, isNullable: false);

        var converters = new ISpatialConverter[]
        {
            new LineStringConverter(),
            new PolygonConverter(),
            new MultiPointConverter(),
            new MultiLineStringConverter(),
            new MultiPolygonConverter(),
            new GeometryCollectionConverter(),
            new PointConverter() // correct one is last
        };

        var registry = new SpatialConverterRegistry(converters);

        // Act
        var result = registry.Convert(point, edmGeomPoint);

        // Assert
        Assert.IsAssignableFrom<MsGeometryPoint>(result);
    }

    private sealed class CountingConverter : ISpatialConverter
    {
        private readonly bool _canConvert;
        private readonly Func<ISpatial> _resultFactory;

        public int CanConvertCalls { get; private set; }
        public int ConvertCalls { get; private set; }

        public CountingConverter(bool canConvert, Func<ISpatial> resultFactory)
        {
            _canConvert = canConvert;
            _resultFactory = resultFactory;
        }

        public bool CanConvert(EdmPrimitiveTypeKind primitiveTypeKind, NtsGeometry geometry)
        {
            CanConvertCalls++;
            return _canConvert;
        }

        public ISpatial Convert(NtsGeometry geometry, IEdmPrimitiveTypeReference primitiveType)
        {
            ConvertCalls++;
            return _resultFactory();
        }
    }

    private sealed class DummySpatial : ISpatial
    {
        public DummySpatial(CoordinateSystem cs, bool isEmpty = false)
        {
            CoordinateSystem = cs;
            IsEmpty = isEmpty;
        }

        public CoordinateSystem CoordinateSystem { get; }
        public bool IsEmpty { get; }
    }
}
