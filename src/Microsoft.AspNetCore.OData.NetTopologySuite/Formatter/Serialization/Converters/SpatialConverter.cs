//-----------------------------------------------------------------------------
// <copyright file="SpatialConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using MsGeographyCollection = Microsoft.Spatial.GeographyCollection;
using MsGeographyFactory = Microsoft.Spatial.GeographyFactory;
using MsGeometryCollection = Microsoft.Spatial.GeometryCollection;
using MsGeometryFactory = Microsoft.Spatial.GeometryFactory;
using NtsCoordinate = NetTopologySuite.Geometries.Coordinate;
using NtsGeometry = NetTopologySuite.Geometries.Geometry;
using NtsGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using NtsLineString = NetTopologySuite.Geometries.LineString;
using NtsMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using NtsMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using NtsMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using NtsPoint = NetTopologySuite.Geometries.Point;
using NtsPolygon = NetTopologySuite.Geometries.Polygon;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal sealed class SpatialConverter
{
    private readonly bool isGeography; // true => Geography*, false => Geometry*
    private readonly CoordinateSystem coordinateSystem;
    private const int DefaultGeographySrid = 4326;
    private const int DefaultGeometrySrid = 0;

    #region Cached Delegates

    // ---------- LineString ----------
    private static readonly Action<GeometryFactory<GeometryLineString>, double, double, double?, double?> GeometryLineStringLineToAction =
        static (f, x, y, z, m) => f.LineTo(x, y, z, m);

    
    private static readonly Action<GeographyFactory<GeographyLineString>, double, double, double?, double?> GeographyLineStringLineToAction =
        static (f, lat, lon, z, m) => f.LineTo(lat, lon, z, m);

    // ---------- Polygon ----------

    private static readonly Action<GeometryFactory<GeometryPolygon>, double, double, double?, double?> GeometryPolygonRingAction =
        static (f, x, y, z, m) => f.Ring(x, y, z, m);

    private static readonly Action<GeographyFactory<GeographyPolygon>, double, double, double?, double?> GeographyPolygonRingAction =
        static (f, lat, lon, z, m) => f.Ring(lat, lon, z, m);

    private static readonly Action<GeometryFactory<GeometryPolygon>, double, double, double?, double?> GeometryPolygonLineToAction =
        static (f, x, y, z, m) => f.LineTo(x, y, z, m);

    private static readonly Action<GeographyFactory<GeographyPolygon>, double, double, double?, double?> GeographyPolygonLineToAction =
        static (f, lat, lon, z, m) => f.LineTo(lat, lon, z, m);

    // ---------- MultiPoint ----------
    private static readonly Action<GeometryFactory<GeometryMultiPoint>, double, double, double?, double?> GeometryMultiPointAction =
        static (f, x, y, z, m) => f.Point(x, y, z, m);

    private static readonly Action<GeographyFactory<GeographyMultiPoint>, double, double, double?, double?> GeographyMultiPointAction =
        static (f, lat, lon, z, m) => f.Point(lat, lon, z, m);

    // ---------- MultiLineString ----------
    private static readonly Action<GeometryFactory<GeometryMultiLineString>, double, double, double?, double?> GeometryMultiLineStringLineToAction =
        static (f, x, y, z, m) => f.LineTo(x, y, z, m);

    private static readonly Action<GeographyFactory<GeographyMultiLineString>, double, double, double?, double?> GeographyMultiLineStringLineToAction =
        static (f, lat, lon, z, m) => f.LineTo(lat, lon, z, m);

    // ---------- MultiPolygon ----------
    private static readonly Action<GeometryFactory<GeometryMultiPolygon>, double, double, double?, double?> GeometryMultiPolygonRingAction =
        static (f, x, y, z, m) => f.Ring(x, y, z, m);

    private static readonly Action<GeographyFactory<GeographyMultiPolygon>, double, double, double?, double?> GeographyMultiPolygonRingAction =
        static (f, lat, lon, z, m) => f.Ring(lat, lon, z, m);

    private static readonly Action<GeometryFactory<GeometryMultiPolygon>, double, double, double?, double?> GeometryMultiPolygonLineToAction =
        static (f, x, y, z, m) => f.LineTo(x, y, z, m);

    private static readonly Action<GeographyFactory<GeographyMultiPolygon>, double, double, double?, double?> GeographyMultiPolygonLineToAction =
        static (f, lat, lon, z, m) => f.LineTo(lat, lon, z, m);

    // ---------- GeometryCollection ----------
    private static readonly Action<GeometryFactory<MsGeometryCollection>, double, double, double?, double?> GeometryCollectionPointAction =
        static (f, x, y, z, m) => f.Point(x, y, z, m);

    private static readonly Action<GeographyFactory<MsGeographyCollection>, double, double, double?, double?> GeographyCollectionPointAction =
        static (f, lat, lon, z, m) => f.Point(lat, lon, z, m);

    private static readonly Func<GeometryFactory<MsGeometryCollection>, GeometryFactory<MsGeometryCollection>> GeometryCollectionLineStringFunc
        = static f => f.LineString();

    private static readonly Func<GeographyFactory<MsGeographyCollection>, GeographyFactory<MsGeographyCollection>> GeographyCollectionLineStringFunc
        = static f => f.LineString();

    private static readonly Action<GeometryFactory<MsGeometryCollection>, double, double, double?, double?> GeometryCollectionLineToAction =
        static (f, x, y, z, m) => f.LineTo(x, y, z, m);

    private static readonly Action<GeographyFactory<MsGeographyCollection>, double, double, double?, double?> GeographyCollectionLineToAction =
        static (f, lat, lon, z, m) => f.LineTo(lat, lon, z, m);

    private static readonly Func<GeometryFactory<MsGeometryCollection>, GeometryFactory<MsGeometryCollection>> GeometryCollectionPolygonAction
        = static f => f.Polygon();

    private static readonly Func<GeographyFactory<MsGeographyCollection>, GeographyFactory<MsGeographyCollection>> GeographyCollectionPolygonAction
        = static f => f.Polygon();

    private static readonly Action<GeometryFactory<MsGeometryCollection>, double, double, double?, double?> GeometryCollectionRingAction =
        static (f, x, y, z, m) => f.Ring(x, y, z, m);

    private static readonly Action<GeographyFactory<MsGeographyCollection>, double, double, double?, double?> GeographyCollectionRingAction =
        static (f, lat, lon, z, m) => f.Ring(lat, lon, z, m);

    #endregion Cached Delegates

    private SpatialConverter(bool geography, int srid)
    {
        this.isGeography = geography;
        // Use the static default coordinate systems for the common SRIDs
        if (geography)
        {
            this.coordinateSystem = srid == DefaultGeographySrid ?
                CoordinateSystem.DefaultGeography : CoordinateSystem.Geography(srid);
        }
        else
        {
            this.coordinateSystem = srid == DefaultGeometrySrid ?
                CoordinateSystem.DefaultGeometry : CoordinateSystem.Geometry(srid);
        }
    }

    public static SpatialConverter For(IEdmPrimitiveTypeReference primitiveType, int srid)
    {
        if (primitiveType == null)
        {
            throw Error.ArgumentNull(nameof(primitiveType));
        }

        bool isGeography = primitiveType.PrimitiveKind() switch
        {
            // Geography
            EdmPrimitiveTypeKind.GeographyPoint => true,
            EdmPrimitiveTypeKind.GeographyLineString => true,
            EdmPrimitiveTypeKind.GeographyPolygon => true,
            EdmPrimitiveTypeKind.GeographyMultiPoint => true,
            EdmPrimitiveTypeKind.GeographyMultiLineString => true,
            EdmPrimitiveTypeKind.GeographyMultiPolygon => true,
            EdmPrimitiveTypeKind.GeographyCollection => true,
            EdmPrimitiveTypeKind.Geography => true,
            // Geometry
            EdmPrimitiveTypeKind.GeometryPoint => false,
            EdmPrimitiveTypeKind.GeometryLineString => false,
            EdmPrimitiveTypeKind.GeometryPolygon => false,
            EdmPrimitiveTypeKind.GeometryMultiPoint => false,
            EdmPrimitiveTypeKind.GeometryMultiLineString => false,
            EdmPrimitiveTypeKind.GeometryMultiPolygon => false,
            EdmPrimitiveTypeKind.GeometryCollection => false,
            EdmPrimitiveTypeKind.Geometry => false,
            _ => throw Error.InvalidOperation(SRResources.SpatialConverter_UnsupportedEdmType, typeof(SpatialConverter).FullName, primitiveType.PrimitiveKind().ToString())
        };

        return new SpatialConverter(isGeography, srid);
    }

    public ISpatial Convert(NtsGeometry geometry)
    {
        if (geometry == null)
        {
            throw Error.ArgumentNull(nameof(geometry));
        }

        return geometry switch
        {
            NtsPoint point => Convert(point),
            NtsLineString lineString => Convert(lineString),
            NtsPolygon polygon => Convert(polygon),
            NtsMultiPoint multiPoint => Convert(multiPoint),
            NtsMultiLineString multiLineString => Convert(multiLineString),
            NtsMultiPolygon multiPolygon => Convert(multiPolygon),
            NtsGeometryCollection geometryCollection => Convert(geometryCollection),
            _ => throw Error.NotSupported(SRResources.SpatialConverter_UnsupportedGeometryType, typeof(SpatialConverter).FullName, geometry.GeometryType)
        };
    }

    private ISpatial Convert(NtsPoint point) =>
        this.isGeography
        ? BuildGeographyPoint(point)
        : BuildGeometryPoint(point);

    private ISpatial Convert(NtsLineString lineString) =>
        this.isGeography
        ? BuildGeographyLineString(lineString)
        : BuildGeometryLineString(lineString);

    private ISpatial Convert(NtsPolygon polygon) =>
        this.isGeography
        ? BuildGeographyPolygon(polygon)
        : BuildGeometryPolygon(polygon);

    private ISpatial Convert(NtsMultiPoint multiPoint) =>
        this.isGeography
        ? BuildGeographyMultiPoint(multiPoint)
        : BuildGeometryMultiPoint(multiPoint);

    private ISpatial Convert(NtsMultiLineString multiLineString) =>
        this.isGeography
        ? BuildGeographyMultiLineString(multiLineString)
        : BuildGeometryMultiLineString(multiLineString);

    private ISpatial Convert(NtsMultiPolygon multiPolygon) =>
        this.isGeography
        ? BuildGeographyMultiPolygon(multiPolygon)
        : BuildGeometryMultiPolygon(multiPolygon);

    private ISpatial Convert(NtsGeometryCollection geometryCollection) =>
        this.isGeography
        ? BuildGeographyCollection(geometryCollection)
        : BuildGeometryCollection(geometryCollection);

    private GeometryPoint BuildGeometryPoint(NtsPoint point)
    {
        Debug.Assert(point != null, $"{point} != null");

        if (point.Coordinate == null)
        {
            // POINT EMPTY
            return MsGeometryFactory.Point(coordinateSystem).Build();
        }

        new GeometryMapper().Map(point.Coordinate, out double x, out double y, out double? z, out double? m);

        return MsGeometryFactory.Point(coordinateSystem, x, y, z, m).Build();
    }

    private GeographyPoint BuildGeographyPoint(NtsPoint point)
    {
        Debug.Assert(point != null, $"{point} != null");

        if (point.Coordinate == null)
        {
            // POINT EMPTY
            return MsGeographyFactory.Point(coordinateSystem).Build();
        }

        new GeographyMapper().Map(point.Coordinate, out double lat, out double lon, out double? z, out double? m);

        return MsGeographyFactory.Point(coordinateSystem, lat, lon, z, m).Build();
    }

    private GeometryLineString BuildGeometryLineString(NtsLineString lineString)
    {
        Debug.Assert(lineString != null, $"{lineString} != null");

        GeometryFactory<GeometryLineString> factory = MsGeometryFactory.LineString(coordinateSystem);
        if (lineString.Coordinates == null || lineString.Coordinates.Length == 0)
        {
            // LINESTRING EMPTY
            return factory.Build();
        }

        ConstructFromCoordinates(
            factory,
            lineString.Coordinates,
            GeometryLineStringLineToAction,
            default(GeometryMapper));

        return factory.Build();
    }

    private GeographyLineString BuildGeographyLineString(NtsLineString lineString)
    {
        Debug.Assert(lineString != null, $"{lineString} != null");

        GeographyFactory<GeographyLineString> factory = MsGeographyFactory.LineString(coordinateSystem);
        if (lineString.Coordinates == null || lineString.Coordinates.Length == 0)
        {
            // LINESTRING EMPTY
            return factory.Build();
        }

        ConstructFromCoordinates(
            factory,
            lineString.Coordinates,
            GeographyLineStringLineToAction,
            default(GeographyMapper));

        return factory.Build();
    }

    private GeometryPolygon BuildGeometryPolygon(NtsPolygon polygon)
    {
        Debug.Assert(polygon != null, $"{polygon} != null");

        GeometryFactory<GeometryPolygon> factory = MsGeometryFactory.Polygon(coordinateSystem);
        if (polygon.Shell == null)
        {
            // POLYGON EMPTY
            return factory.Build();
        }

        ConstructFromPolygon(
            factory,
            polygon,
            GeometryPolygonRingAction,
            GeometryPolygonLineToAction,
            default(GeometryMapper));

        return factory.Build();
    }

    private GeographyPolygon BuildGeographyPolygon(NtsPolygon polygon)
    {
        Debug.Assert(polygon != null, $"{polygon} != null");

        GeographyFactory<GeographyPolygon> factory = MsGeographyFactory.Polygon(coordinateSystem);
        if (polygon.Shell == null)
        {
            // POLYGON EMPTY
            return factory.Build();
        }

        ConstructFromPolygon(
            factory,
            polygon,
            GeographyPolygonRingAction,
            GeographyPolygonLineToAction,
            default(GeographyMapper));

        return factory.Build();
    }

    private GeometryMultiPoint BuildGeometryMultiPoint(NtsMultiPoint multiPoint)
    {
        Debug.Assert(multiPoint != null, $"{multiPoint} != null");

        GeometryFactory<GeometryMultiPoint> factory = MsGeometryFactory.MultiPoint(coordinateSystem);
        if (multiPoint.Geometries == null || multiPoint.Geometries.Length == 0)
        {
            // MULTIPOINT EMPTY
            return factory.Build();
        }

        for (int i = 0; i < multiPoint.Geometries.Length; i++)
        {
            NtsPoint point = (NtsPoint)multiPoint.Geometries[i];

            ConstructFromCoordinate(
                factory,
                point.Coordinate,
                GeometryMultiPointAction,
                default(GeometryMapper));
        }

        return factory.Build();
    }

    private GeographyMultiPoint BuildGeographyMultiPoint(NtsMultiPoint multiPoint)
    {
        Debug.Assert(multiPoint != null, $"{multiPoint} != null");
        GeographyFactory<GeographyMultiPoint> factory = MsGeographyFactory.MultiPoint(coordinateSystem);
        if (multiPoint.Geometries == null || multiPoint.Geometries.Length == 0)
        {
            // MULTIPOINT EMPTY
            return factory.Build();
        }

        for (int i = 0; i < multiPoint.Geometries.Length; i++)
        {
            NtsPoint point = (NtsPoint)multiPoint.Geometries[i];

            ConstructFromCoordinate(
                factory,
                point.Coordinate,
                GeographyMultiPointAction,
                default(GeographyMapper));
        }

        return factory.Build();
    }

    private GeometryMultiLineString BuildGeometryMultiLineString(NtsMultiLineString multiLineString)
    {
        Debug.Assert(multiLineString != null, $"{multiLineString} != null");

        GeometryFactory<GeometryMultiLineString> factory = MsGeometryFactory.MultiLineString(coordinateSystem);
        if (multiLineString.Geometries == null || multiLineString.Geometries.Length == 0)
        {
            // MULTILINESTRING EMPTY
            return factory.Build();
        }

        for (int i = 0; i < multiLineString.Geometries.Length; i++)
        {
            NtsLineString lineString = (NtsLineString)multiLineString.Geometries[i];

            ConstructFromCoordinates(
                factory.LineString(),
                lineString.Coordinates,
                GeometryMultiLineStringLineToAction,
                default(GeometryMapper));
        }

        return factory.Build();
    }

    private GeographyMultiLineString BuildGeographyMultiLineString(NtsMultiLineString multiLineString)
    {
        Debug.Assert(multiLineString != null, $"{multiLineString} != null");

        GeographyFactory<GeographyMultiLineString> factory = MsGeographyFactory.MultiLineString(coordinateSystem);
        if (multiLineString.Geometries == null || multiLineString.Geometries.Length == 0)
        {
            // MULTILINESTRING EMPTY
            return factory.Build();
        }

        for (int i = 0; i < multiLineString.Geometries.Length; i++)
        {
            NtsLineString lineString = (NtsLineString)multiLineString.Geometries[i];

            ConstructFromCoordinates(
                factory.LineString(),
                lineString.Coordinates,
                GeographyMultiLineStringLineToAction,
                default(GeographyMapper));
        }

        return factory.Build();
    }

    private GeometryMultiPolygon BuildGeometryMultiPolygon(NtsMultiPolygon multiPolygon)
    {
        Debug.Assert(multiPolygon != null, $"{multiPolygon} != null");

        GeometryFactory<GeometryMultiPolygon> factory = MsGeometryFactory.MultiPolygon(coordinateSystem);
        if (multiPolygon.Geometries == null || multiPolygon.Geometries.Length == 0)
        {
            // MULTIPOLYGON EMPTY
            return factory.Build();
        }

        for (int i = 0; i < multiPolygon.Geometries.Length; i++)
        {
            NtsPolygon polygon = (NtsPolygon)multiPolygon.Geometries[i];

            ConstructFromPolygon(
                factory.Polygon(),
                polygon,
                GeometryMultiPolygonRingAction,
                GeometryMultiPolygonLineToAction,
                default(GeometryMapper));
        }

        return factory.Build();
    }

    private GeographyMultiPolygon BuildGeographyMultiPolygon(NtsMultiPolygon multiPolygon)
    {
        Debug.Assert(multiPolygon != null, $"{multiPolygon} != null");

        GeographyFactory<GeographyMultiPolygon> factory = MsGeographyFactory.MultiPolygon(coordinateSystem);
        if (multiPolygon.Geometries == null || multiPolygon.Geometries.Length == 0)
        {
            // MULTIPOLYGON EMPTY
            return factory.Build();
        }

        for (int i = 0; i < multiPolygon.Geometries.Length; i++)
        {
            NtsPolygon polygon = (NtsPolygon)multiPolygon.Geometries[i];

            ConstructFromPolygon(
                factory.Polygon(),
                polygon,
                GeographyMultiPolygonRingAction,
                GeographyMultiPolygonLineToAction,
                default(GeographyMapper));
        }

        return factory.Build();
    }

    private MsGeometryCollection BuildGeometryCollection(NtsGeometryCollection geometryCollection)
    {
        Debug.Assert(geometryCollection != null, $"{geometryCollection} != null");

        GeometryFactory<MsGeometryCollection> factory = MsGeometryFactory.Collection(coordinateSystem);
        if (geometryCollection.Coordinates == null || geometryCollection.Geometries.Length == 0)
        {
            // GEOMETRYCOLLECTION EMPTY
            return factory.Build();
        }

        ConstructFromGeometryCollection(factory, geometryCollection, default(GeometryMapper));

        return factory.Build();
    }

    private MsGeographyCollection BuildGeographyCollection(NtsGeometryCollection geometryCollection)
    {
        Debug.Assert(geometryCollection != null, $"{geometryCollection} != null");

        GeographyFactory<MsGeographyCollection> factory = MsGeographyFactory.Collection(coordinateSystem);
        if (geometryCollection.Coordinates == null || geometryCollection.Geometries.Length == 0)
        {
            // GEOMETRYCOLLECTION EMPTY
            return factory.Build();
        }

        ConstructFromGeometryCollection(factory, geometryCollection, default(GeographyMapper));

        return factory.Build();
    }

    private static void ConstructFromCoordinate<TFactory, TMapper>(
        TFactory factory,
        NtsCoordinate coordinate,
        Action<TFactory, double, double, double?, double?> startPoint,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        mapper.Map(coordinate, out double latOrX, out double lonOrY, out double? elevation, out double? measure);
        startPoint(factory, latOrX, lonOrY, elevation, measure);
    }

    private static void ConstructFromCoordinates<TFactory, TMapper>(
        TFactory factory,
        NtsCoordinate[] coordinates,
        Action<TFactory, double, double, double?, double?> lineTo,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        Debug.Assert(factory != null, $"{factory} != null");
        Debug.Assert(coordinates != null, $"{coordinates} != null");
        Debug.Assert(lineTo != null, $"{lineTo} != null");

        if (coordinates == null || coordinates.Length == 0)
        {
            return;
        }

        for (int i = 0; i < coordinates.Length; i++)
        {
            mapper.Map(coordinates[i], out double latOrX, out double lonOrY, out double? elevation, out double? measure);
            lineTo(factory, latOrX, lonOrY, elevation, measure);
        }
    }

    private static void ConstructFromCoordinates<TFactory, TMapper>(
        TFactory factory,
        NtsCoordinate[] coordinates,
        Action<TFactory, double, double, double?, double?> geoStart,
        Action<TFactory, double, double, double?, double?> lineTo,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        Debug.Assert(factory != null, $"{factory} != null");
        Debug.Assert(coordinates != null, $"{coordinates} != null");
        Debug.Assert(geoStart != null, $"{geoStart} != null");
        Debug.Assert(lineTo != null, $"{lineTo} != null");

        if (coordinates == null || coordinates.Length == 0)
        {
            return;
        }

        mapper.Map(coordinates[0], out double latOrX, out double lonOrY, out double? elevation, out double? measure);
        geoStart(factory, latOrX, lonOrY, elevation, measure);

        for (int i = 1; i < coordinates.Length; i++)
        {
            mapper.Map(coordinates[i], out latOrX, out lonOrY, out elevation, out measure);
            lineTo(factory, latOrX, lonOrY, elevation, measure);
        }
    }

    private static void ConstructFromPolygon<TFactory, TMapper>(
        TFactory factory,
        NtsPolygon polygon,
        Action<TFactory, double, double, double?, double?> ringStart,
        Action<TFactory, double, double, double?, double?> lineTo,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        Debug.Assert(factory != null, $"{factory} != null");
        Debug.Assert(polygon != null, $"{polygon} != null");
        Debug.Assert(ringStart != null, $"{ringStart} != null");
        Debug.Assert(lineTo != null, $"{lineTo} != null");

        // TODO: Check polygon not null

        // Outer ring
        ConstructFromCoordinates(factory, polygon.Shell?.Coordinates, ringStart, lineTo, mapper);

        // Holes
        for (int i = 0; i < polygon.Holes.Length; i++)
        {
            ConstructFromCoordinates(factory, polygon.Holes[i].Coordinates, ringStart, lineTo, mapper);
        }
    }

    private void ConstructFromGeometryCollection<TMapper>(
        GeometryFactory<MsGeometryCollection> collectionFactory,
        NtsGeometryCollection geometryCollection,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        Debug.Assert(collectionFactory != null, $"{collectionFactory} != null");
        Debug.Assert(geometryCollection != null, $"{geometryCollection} != null");

        ConstructFromGeometryCollectionCore(
            geometryCollection,
            collectionFactory: collectionFactory,
            // Shared
            pointStart: GeometryCollectionPointAction,
            lineTo: GeometryCollectionLineToAction,
            // LineString
            lineStringFactory: collectionFactory.LineString,
            // Polygon
            polygonFactory: collectionFactory.Polygon,
            ringStart: GeometryCollectionRingAction,
            // MultiPoint
            multiPointFactory: collectionFactory.MultiPoint,
            // MultiLineString
            multiLineStringFactory: collectionFactory.MultiLineString,
            multiLineStringItemFactory: GeometryCollectionLineStringFunc,
            // MultiPolygon
            multiPolygonFactory: collectionFactory.MultiPolygon,
            multiPolygonItemFactory: GeometryCollectionPolygonAction,
            // Nested GeometryCollection
            nestedGeometryCollectionFactory: nestedGeometryCollection => ConstructFromGeometryCollection(
                collectionFactory.Collection(),
                nestedGeometryCollection,
                mapper),
            mapper);
    }

    private void ConstructFromGeometryCollection<TMapper>(
        GeographyFactory<MsGeographyCollection> collectionFactory,
        NtsGeometryCollection geometryCollection,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        Debug.Assert(geometryCollection != null, $"{geometryCollection} != null");
        Debug.Assert(collectionFactory != null, $"{collectionFactory} != null");

        ConstructFromGeometryCollectionCore(
            geometryCollection,
            collectionFactory: collectionFactory,
            // Shared
            pointStart: GeographyCollectionPointAction,
            lineTo: GeographyCollectionLineToAction,
            // LineString
            lineStringFactory: collectionFactory.LineString,
            // Polygon
            polygonFactory: collectionFactory.Polygon,
            ringStart: GeographyCollectionRingAction,
            // MultiPoint
            multiPointFactory: collectionFactory.MultiPoint,
            // MultiLineString
            multiLineStringFactory: collectionFactory.MultiLineString,
            multiLineStringItemFactory: GeographyCollectionLineStringFunc,
            // MultiPolygon
            multiPolygonFactory: collectionFactory.MultiPolygon,
            multiPolygonItemFactory: GeographyCollectionPolygonAction,
            // Nested GeometryCollection
            nestedGeometryCollectionFactory: nestedGeometryCollection => ConstructFromGeometryCollection(
                collectionFactory.Collection(),
                nestedGeometryCollection,
                mapper),
            mapper);
    }

    private void ConstructFromGeometryCollectionCore<TFactory, TMapper>(
        NtsGeometryCollection geometryCollection,
        TFactory collectionFactory,
        // Shared
        Action<TFactory, double, double, double?, double?> pointStart,
        Action<TFactory, double, double, double?, double?> lineTo,
        // LineString
        Func<TFactory> lineStringFactory,
        // Polygon
        Func<TFactory> polygonFactory,
        Action<TFactory, double, double, double?, double?> ringStart,
        // MultiPoint
        Func<TFactory> multiPointFactory,
        // MultiLineString
        Func<TFactory> multiLineStringFactory,
        Func<TFactory, TFactory> multiLineStringItemFactory,
        // MultiPolygon
        Func<TFactory> multiPolygonFactory,
        Func<TFactory, TFactory> multiPolygonItemFactory,
        // Nested GeometryCollection
        Action<NtsGeometryCollection> nestedGeometryCollectionFactory,
        TMapper mapper)
        where TMapper : struct, ICoordinateMapper
    {
        Debug.Assert(collectionFactory != null, $"{collectionFactory} != null");
        Debug.Assert(pointStart != null, $"{pointStart} != null");
        Debug.Assert(lineTo != null, $"{lineTo} != null");
        Debug.Assert(lineStringFactory != null, $"{lineStringFactory} != null");
        Debug.Assert(polygonFactory != null, $"{polygonFactory} != null");
        Debug.Assert(ringStart != null, $"{ringStart} != null");
        Debug.Assert(multiPointFactory != null, $"{multiPointFactory} != null");
        Debug.Assert(multiLineStringFactory != null, $"{multiLineStringFactory} != null");
        Debug.Assert(multiLineStringItemFactory != null, $"{multiLineStringItemFactory} != null");
        Debug.Assert(multiPolygonFactory != null, $"{multiPolygonFactory} != null");
        Debug.Assert(multiPolygonItemFactory != null, $"{multiPolygonItemFactory} != null");
        Debug.Assert(nestedGeometryCollectionFactory != null, $"{nestedGeometryCollectionFactory} != null");
        // TODO: Because of recursion, perform an actual null check for geometryCollection

        for (int i = 0; i < geometryCollection.Geometries.Length; i++)
        {
            NtsGeometry geometry = geometryCollection.Geometries[i];

            switch (geometry)
            {
                case NtsPoint point:
                    {
                        ConstructFromCoordinate(collectionFactory, point.Coordinate, pointStart, mapper);

                        break;
                    }
                case NtsLineString lineString:
                    {
                        TFactory factory = lineStringFactory();
                        ConstructFromCoordinates(factory, lineString.Coordinates, lineTo, mapper);

                        break;
                    }
                case NtsPolygon polygon:
                    {
                        TFactory factory = polygonFactory();
                        ConstructFromPolygon(factory, polygon, ringStart, lineTo, mapper);

                        break;
                    }
                case NtsMultiPoint multiPoint:
                    {
                        TFactory factory = multiPointFactory();
                        for (int j = 0; j < multiPoint.Geometries.Length; j++)
                        {
                            NtsPoint point = (NtsPoint)multiPoint.Geometries[j];
                            ConstructFromCoordinate(factory, point.Coordinate, pointStart, mapper);
                        }

                        break;
                    }
                case NtsMultiLineString multiLineString:
                    {
                        TFactory factory = multiLineStringFactory();
                        for (int j = 0; j < multiLineString.Geometries.Length; j++)
                        {
                            NtsLineString lineString = (NtsLineString)multiLineString.Geometries[j];
                            TFactory itemFactory = multiLineStringItemFactory(factory);
                            ConstructFromCoordinates(itemFactory, lineString.Coordinates, lineTo, mapper);
                        }

                        break;
                    }
                case NtsMultiPolygon multiPolygon:
                    {
                        TFactory factory = multiPolygonFactory();
                        for (int j = 0; j < multiPolygon.Geometries.Length; j++)
                        {
                            NtsPolygon polygon = (NtsPolygon)multiPolygon.Geometries[j];
                            TFactory itemFactory = multiPolygonItemFactory(factory);
                            ConstructFromPolygon(itemFactory, polygon, ringStart, lineTo, mapper);
                        }

                        break;
                    }
                case NtsGeometryCollection nestedGeometryCollection:
                    nestedGeometryCollectionFactory(nestedGeometryCollection);

                    break;
                default:
                    throw Error.NotSupported(
                        SRResources.SpatialConverter_UnsupportedGeometryType,
                        typeof(SpatialConverter).FullName,
                        geometry.GeometryType);
            }
        }
    }

    // Map NTS Coordinate to (latOrX,lonOrY,z,m) with correct orientation for Geometry/Geography.
    // For Geometry: (latOrX,lonOrY) = (X,Y), for Geography: (latOrX,lonOrY) = (Y,X)

    // NOTE: We are using the mapper pattern to prevent allocation of delegate instances at call sites,
    // support inlining, and enable better optimization by JIT.
    private interface ICoordinateMapper
    {
        void Map(NtsCoordinate coordinate, out double latOrX, out double lonOrY, out double? z, out double? m);
    }

    private readonly struct GeometryMapper : ICoordinateMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Map(NtsCoordinate coordinate, out double x, out double y, out double? z, out double? m)
        {
            z = double.IsNaN(coordinate.Z) ? null : coordinate.Z;
            m = double.IsNaN(coordinate.M) ? null : coordinate.M;
            x = coordinate.X; y = coordinate.Y;
        }
    }

    private readonly struct GeographyMapper : ICoordinateMapper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Map(NtsCoordinate coordinate, out double lat, out double lon, out double? z, out double? m)
        {
            z = double.IsNaN(coordinate.Z) ? null : coordinate.Z;
            m = double.IsNaN(coordinate.M) ? null : coordinate.M;
            lat = coordinate.Y; lon = coordinate.X; // lat, lon
        }
    }
}
