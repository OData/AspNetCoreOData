//-----------------------------------------------------------------------------
// <copyright file="ODataSpatialNetTopologySuiteDeserializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Deserialization;

/// <summary>
/// Represents an <see cref="OData.Formatter.Deserialization.ODataDeserializer"/> that can read OData spatial types.
/// </summary>
internal class ODataSpatialNetTopologySuiteDeserializer : ODataSpatialDeserializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ODataSpatialNetTopologySuiteDeserializer"/> class.
    /// </summary>
    public ODataSpatialNetTopologySuiteDeserializer()
        : base()
    {
    }

    public override async Task<object> ReadAsync(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
    {
        if (messageReader == null)
        {
            throw new ArgumentNullException(nameof(messageReader));
        }

        if (readContext == null)
        {
            throw new ArgumentNullException(nameof(readContext));
        }

        if (readContext.Path?.LastSegment is PropertySegment propertySegment)
        {
            // Trust the model to provide the correct Edm type for the NTS spatial property.
            // The mapper only maps NTS types to Edm.Geometry* and we have no means at this point,
            // specifically, no access to GeographyAttribute decorating the property on the CLR type
            // to help us disambiguate between Edm.Geometry* and Edm.Geography* other than the model
            // that has the correct type information set during model creation.
            IEdmTypeReference edmType = propertySegment.Property.Type;

            ODataProperty property = await messageReader.ReadPropertyAsync(edmType).ConfigureAwait(false);
            return ReadInline(property, edmType, readContext);
        }

        return await base.ReadAsync(messageReader, type, readContext).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
    {
        if (item == null)
        {
            return null;
        }

        if (readContext == null)
        {
            throw Error.ArgumentNull(nameof(readContext));
        }

        ODataProperty property = item as ODataProperty;
        if (property != null)
        {
            item = property.Value;
        }

        if (!(item is ISpatial spatialValue))
        {
            // TODO: Use resource manager for error messages
            throw new ArgumentException($"The item must be of type ISpatial, but was '{item.GetType().FullName}'.", nameof(item));
        }

        return ConvertSpatialValue(spatialValue);
    }

    private object ConvertSpatialValue(ISpatial spatialValue)
    {
        switch (spatialValue)
        {
            case GeometryPoint:
                GeometryPoint geometryPoint = (GeometryPoint)spatialValue;
                
                return CreatePoint(
                    geometryPoint.X,
                    geometryPoint.Y,
                    geometryPoint.Z,
                    geometryPoint.M,
                    geometryPoint.CoordinateSystem?.EpsgId);
            case GeographyPoint:
                GeographyPoint geographyPoint = (GeographyPoint)spatialValue;

                return CreatePoint(
                    geographyPoint.Longitude,
                    geographyPoint.Latitude,
                    geographyPoint.Z,
                    geographyPoint.M,
                    geographyPoint.CoordinateSystem?.EpsgId);

            case GeometryLineString:
                GeometryLineString geometryLineString = (GeometryLineString)spatialValue;

                return CreateLineString(geometryLineString.Points, geometryLineString.CoordinateSystem?.EpsgId);
            case GeographyLineString:
                GeographyLineString geographyLineString = (GeographyLineString)spatialValue;

                return CreateLineString(geographyLineString.Points, geographyLineString.CoordinateSystem?.EpsgId);
            default:
                throw new NotSupportedException($"The type '{spatialValue.GetType().FullName}' is not supported.");
        }
    }

    private static Point CreatePoint(double x, double y, double? z, double? m, int? srid)
    {
        var coordinate = CreateCoordinate(x, y, z, m);
        // TODO: Is it better to use a GeometryFactory?
        var point = new Point(coordinate);
        if (!IsNullOrNaN(srid))
        {
            point.SRID = (int)srid;
        }

        return point;
    }

    private static LineString CreateLineString(ReadOnlyCollection<GeometryPoint> points, int? srid)
    {
        var coordinates = new Coordinate[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            GeometryPoint geometryPoint = points[i];
            coordinates[i] = CreateCoordinate(geometryPoint.X, geometryPoint.Y, geometryPoint.Z, geometryPoint.M);
        }

        var lineString = new LineString(coordinates);
        if (!IsNullOrNaN(srid))
        {
            lineString.SRID = (int)srid;
        }

        return lineString;
    }

    private static LineString CreateLineString(ReadOnlyCollection<GeographyPoint> points, int? srid)
    {
        Coordinate[] coordinates = new Coordinate[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            GeographyPoint geographyPoint = points[i];
            coordinates[i] = CreateCoordinate(geographyPoint.Longitude, geographyPoint.Latitude, geographyPoint.Z, geographyPoint.M);
        }

        LineString lineString = new LineString(coordinates);
        if (!IsNullOrNaN(srid))
        {
            lineString.SRID = (int)srid;
        }

        return lineString;
    }

    private static Coordinate CreateCoordinate(double x, double y, double? z, double? m)
    {
        bool hasZ = !IsNullOrNaN(z);
        bool hasM = !IsNullOrNaN(m);

        // Most common scenario: only X and Y
        if (!hasZ && !hasM)
        {
            return new Coordinate(x, y);
        }
        else if (hasZ && hasM)
        {
            return new CoordinateZM(x, y, z.Value, m.Value);
        }
        else if (hasZ)
        {
            return new CoordinateZ(x, y, z.Value);
        }
        else // hasM
        {
            return new CoordinateM(x, y, m.Value);
        }
    }

    private static bool IsNullOrNaN(double? value)
    {
        return !value.HasValue || double.IsNaN(value.Value);
    }
}
