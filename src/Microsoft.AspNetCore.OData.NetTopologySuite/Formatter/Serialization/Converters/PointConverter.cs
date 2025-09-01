//-----------------------------------------------------------------------------
// <copyright file="PointConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using ISpatial = Microsoft.Spatial.ISpatial;
using Point = NetTopologySuite.Geometries.Point;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal class PointConverter : ISpatialConverter
{
    public bool CanConvert(EdmPrimitiveTypeKind primitiveTypeKind, Geometry geometry)
    {
        return geometry is Point
            && (primitiveTypeKind == EdmPrimitiveTypeKind.GeometryPoint
            || primitiveTypeKind == EdmPrimitiveTypeKind.GeographyPoint);
    }

    public ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType)
    {
        SpatialConverter converter = SpatialConverter.For(primitiveType, geometry.SRID);

        return converter.Convert((Point)geometry);
    }
}
