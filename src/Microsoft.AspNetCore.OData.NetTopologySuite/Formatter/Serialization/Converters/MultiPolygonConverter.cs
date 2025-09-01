//-----------------------------------------------------------------------------
// <copyright file="MultiPolygonConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using ISpatial = Microsoft.Spatial.ISpatial;
using MultiPolygon = NetTopologySuite.Geometries.MultiPolygon;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal class MultiPolygonConverter : ISpatialConverter
{
    public bool CanConvert(EdmPrimitiveTypeKind primitiveTypeKind, Geometry geometry)
    {
        return geometry is MultiPolygon
        && (primitiveTypeKind == EdmPrimitiveTypeKind.GeometryMultiPolygon
        || primitiveTypeKind == EdmPrimitiveTypeKind.GeographyMultiPolygon);
    }

    public ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType)
    {
        SpatialConverter converter = SpatialConverter.For(primitiveType, geometry.SRID);

        return converter.Convert((MultiPolygon)geometry);
    }
}
