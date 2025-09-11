//-----------------------------------------------------------------------------
// <copyright file="EdmSpatialKindMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Edm;

/// <summary>
/// Maps NetTopologySuite geometry CLR types to their corresponding OData Edm geography primitive kinds.
/// </summary>
/// <remarks>
/// The mapping prefers the most specific known NTS shape (Point, LineString, Polygon, Multi*, GeometryCollection)
/// and falls back to <see cref="Geometry"/> → <see cref="EdmPrimitiveTypeKind.Geography"/>.
/// Returns <c>null</c> if the supplied type is <c>null</c> or not an NTS geometry type.
/// </remarks>
internal static class EdmSpatialKindMapper
{
    /// <summary>
    /// Gets the Edm geography primitive kind that corresponds to the given NetTopologySuite CLR type.
    /// </summary>
    /// <param name="relatedClrType">The CLR type to evaluate (e.g., <c>Point</c>, <c>LineString</c>).</param>
    /// <returns>
    /// The matching <see cref="EdmPrimitiveTypeKind"/> for geography shapes, or <c>null</c> if no mapping exists.
    /// </returns>
    public static EdmPrimitiveTypeKind? GetGeographyKindForClrType(Type relatedClrType)
    {
        if (relatedClrType == null)
        {
            return null;
        }

        // Check most specific types first, then fall back to base Geometry.
        if (typeof(Point).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyPoint;
        }
        if (typeof(LineString).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyLineString;
        }
        if (typeof(Polygon).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyPolygon;
        }
        if (typeof(MultiPoint).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyMultiPoint;
        }
        if (typeof(MultiLineString).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyMultiLineString;
        }
        if (typeof(MultiPolygon).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyMultiPolygon;
        }
        if (typeof(GeometryCollection).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.GeographyCollection;
        }
        if (typeof(Geometry).IsAssignableFrom(relatedClrType))
        {
            return EdmPrimitiveTypeKind.Geography;
        }

        return null;
    }
}
