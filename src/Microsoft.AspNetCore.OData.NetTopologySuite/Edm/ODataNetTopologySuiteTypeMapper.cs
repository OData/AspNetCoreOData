//-----------------------------------------------------------------------------
// <copyright file="ODataNetTopologySuiteTypeMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using GeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using LineString = NetTopologySuite.Geometries.LineString;
using MultiLineString = NetTopologySuite.Geometries.MultiLineString;
using MultiPoint = NetTopologySuite.Geometries.MultiPoint;
using MultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = NetTopologySuite.Geometries.Polygon;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Edm;

/// <summary>
/// A type mapper that registers mappings between NetTopologySuite (NTS) geometry CLR types
/// and OData Edm spatial primitive kinds.
/// </summary>
/// <remarks>
/// - Maps NTS types to Edm.Geometry* kinds (e.g., <see cref="Point"/> → <see cref="EdmPrimitiveTypeKind.GeometryPoint"/>).
/// - This mapper complements the default mapper by adding non-standard mappings for NTS reference types.
/// - This mapper is attached to an <see cref="IEdmModel"/> as a direct-value annotation to scope it per model/route.
/// </remarks>
public class ODataNetTopologySuiteTypeMapper : DefaultODataTypeMapper
{
    /// <summary>
    /// Singleton instance of <see cref="ODataNetTopologySuiteTypeMapper"/>.
    /// </summary>
    internal static readonly ODataNetTopologySuiteTypeMapper Instance;

    static ODataNetTopologySuiteTypeMapper()
    {
        Instance = new ODataNetTopologySuiteTypeMapper();
    }

    /// <summary>
    /// Registers the NetTopologySuite geometry CLR types as Edm spatial primitive kinds.
    /// </summary>
    /// <remarks>
    /// Mappings are registered as non-standard to avoid overriding the default primitive mappings and
    /// to allow providers/conventions to influence the final Edm kind (e.g., switch to geography).
    /// </remarks>
    protected override void RegisterSpatialMappings()
    {
        BuildReferenceTypeMapping<Point>(EdmPrimitiveTypeKind.GeometryPoint, isStandard: false);
        BuildReferenceTypeMapping<LineString>(EdmPrimitiveTypeKind.GeometryLineString, isStandard: false);
        BuildReferenceTypeMapping<Polygon>(EdmPrimitiveTypeKind.GeometryPolygon, isStandard: false);
        BuildReferenceTypeMapping<MultiPoint>(EdmPrimitiveTypeKind.GeometryMultiPoint, isStandard: false);
        BuildReferenceTypeMapping<MultiLineString>(EdmPrimitiveTypeKind.GeometryMultiLineString, isStandard: false);
        BuildReferenceTypeMapping<MultiPolygon>(EdmPrimitiveTypeKind.GeometryMultiPolygon, isStandard: false);
        BuildReferenceTypeMapping<GeometryCollection>(EdmPrimitiveTypeKind.GeometryCollection, isStandard: false);
        BuildReferenceTypeMapping<Geometry>(EdmPrimitiveTypeKind.Geometry, isStandard: false);
    }
}
