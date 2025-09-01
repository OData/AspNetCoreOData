//-----------------------------------------------------------------------------
// <copyright file="ODataSpatialNetTopologySuiteSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Spatial;
using NetTopologySuite.Geometries;
using Geometry = NetTopologySuite.Geometries.Geometry;
using GeometryFactory = Microsoft.Spatial.GeometryFactory;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization;

/// <summary>
/// Represents an <see cref="OData.Formatter.Serialization.ODataSerializer"/> for serializing spatial types.
/// </summary>
internal class ODataSpatialNetTopologySuiteSerializer : ODataSpatialSerializer
{
    private readonly ISpatialConverterRegistry spatialConverterRegistry;

    /// <summary>
    /// Initializes a new instance of <see cref="ODataSpatialNetTopologySuiteSerializer"/>.
    /// </summary>
    public ODataSpatialNetTopologySuiteSerializer(ISpatialConverterRegistry spatialConverterRegistry)
        : base()
    {
        Error.ArgumentNull(nameof(spatialConverterRegistry));

        this.spatialConverterRegistry = spatialConverterRegistry;
    }

    /// <summary>
    /// Creates an <see cref="ODataPrimitiveValue"/> for the object represented by <paramref name="graph"/>.
    /// </summary>
    /// <param name="graph">The primitive value.</param>
    /// <param name="primitiveType">The EDM primitive type of the value.</param>
    /// <param name="writeContext">The serializer write context.</param>
    /// <returns>The created <see cref="ODataPrimitiveValue"/>.</returns>
    public override ODataPrimitiveValue CreateODataPrimitiveValue(
        object graph,
        IEdmPrimitiveTypeReference primitiveType,
        ODataSerializerContext writeContext)
    {
        return CreatePrimitive(graph, primitiveType, writeContext);
    }

    private ODataPrimitiveValue CreatePrimitive(
        object value,
        IEdmPrimitiveTypeReference primitiveType,
        ODataSerializerContext writeContext)
    {
        if (value == null)
        {
            return null;
        }

        if (!primitiveType.IsSpatial())
        {
            throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataSpatialNetTopologySuiteSerializer), primitiveType.FullName());
        }

        if (!(value is Geometry))
        {
            throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataSpatialNetTopologySuiteSerializer), value.GetType().FullName);
        }

        ISpatial spatialValue = this.spatialConverterRegistry.Convert((Geometry)value, primitiveType);
        ODataPrimitiveValue primitive = new ODataPrimitiveValue(spatialValue);

        if (writeContext != null)
        {
            // TODO: Verify expected behavior for spatial values based on metadata level.
            AddTypeNameAnnotationAsNeeded(primitive, primitiveType, writeContext.MetadataLevel);
        }

        return primitive;
    }

    private static GeometryPoint ConvertToGeometryPoint(Point point)
    {
        bool isZNullOrNaN = IsNullOrNaN(point.Z);
        bool isMNullOrNaN = IsNullOrNaN(point.M);

        if (isZNullOrNaN && isMNullOrNaN)
        {
            // Only X and Y
            return GeometryPoint.Create(point.Y, point.X);
        }
        else if (!isZNullOrNaN && isMNullOrNaN)
        {
            // X, Y, Z
            return GeometryPoint.Create(point.Y, point.X, point.Z);
        }
        else
        {
            // X, Y, Z, M
            return GeometryPoint.Create(point.Y, point.X, point.Z, point.M);
        }
    }

    private static GeometryLineString ConvertToGeometryLineString(LineString lineString)
    {
        CoordinateSystem coordinateSystem = CoordinateSystem.Geometry(lineString.SRID);
        Coordinate[] coordinates = lineString.Coordinates;
        GeometryFactory<GeometryLineString> geometryLineStringFactory;

        Coordinate coordinate = coordinates[0];
        if (IsNullOrNaN(coordinate.Z) && IsNullOrNaN(coordinate.M)) // Most common case
        {
            geometryLineStringFactory = GeometryFactory.LineString(coordinateSystem, coordinate.X, coordinate.Y);
        }
        else // X, Y, Z, M
        {
            geometryLineStringFactory = GeometryFactory.LineString(coordinateSystem, coordinate.X, coordinate.Y, coordinate.Z, coordinate.M);
        }

        for (int i = 1; i < coordinates.Length; i++)
        {
            coordinate = coordinates[i];
            if (IsNullOrNaN(coordinate.Z) && IsNullOrNaN(coordinate.M))
            {
                geometryLineStringFactory.LineTo(coordinate.X, coordinate.Y);
            }
            else // X, Y, Z, M
            {
                geometryLineStringFactory.LineTo(coordinate.X, coordinate.Y, coordinate.Z, coordinate.M);
            }
        }

        return geometryLineStringFactory.Build();
    }

    private static GeographyPoint ConvertToGeographyPoint(Point point)
    {
        bool isZNullOrNaN = IsNullOrNaN(point.Z);
        bool isMNullOrNaN = IsNullOrNaN(point.M);

        if (isZNullOrNaN && isMNullOrNaN)
        {
            // Only X and Y
            return GeographyPoint.Create(point.Y, point.X);
        }
        else if (!isZNullOrNaN && isMNullOrNaN)
        {
            // X, Y, Z
            return GeographyPoint.Create(point.Y, point.X, point.Z);
        }
        else
        {
            // X, Y, Z, M
            return GeographyPoint.Create(point.Y, point.X, point.Z, point.M);
        }
    }

    private static GeographyLineString ConvertToGeographyLineString(LineString lineString)
    {
        CoordinateSystem coordinateSystem = CoordinateSystem.Geography(lineString.SRID);
        Coordinate[] coordinates = lineString.Coordinates;
        GeographyFactory<GeographyLineString> geographyLineStringFactory;

        Coordinate coordinate = coordinates[0];
        if (IsNullOrNaN(coordinate.Z) && IsNullOrNaN(coordinate.M)) // Most common case
        {
            geographyLineStringFactory = GeographyFactory.LineString(coordinateSystem, coordinate.X, coordinate.Y);
        }
        else // X, Y, Z, M
        {
            geographyLineStringFactory = GeographyFactory.LineString(coordinateSystem, coordinate.X, coordinate.Y, coordinate.Z, coordinate.M);
        }

        for (int i = 1; i < coordinates.Length; i++)
        {
            coordinate = coordinates[i];
            if (IsNullOrNaN(coordinate.Z) && IsNullOrNaN(coordinate.M))
            {
                geographyLineStringFactory.LineTo(coordinate.X, coordinate.Y);
            }
            else // X, Y, Z, M
            {
                geographyLineStringFactory.LineTo(coordinate.X, coordinate.Y, coordinate.Z, coordinate.M);
            }
        }

        return geographyLineStringFactory.Build();
    }

    private static bool IsNullOrNaN(double? value)
    {
        return !value.HasValue || double.IsNaN(value.Value);
    }
}
