//-----------------------------------------------------------------------------
// <copyright file="MultiLineStringConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using ISpatial = Microsoft.Spatial.ISpatial;
using MultiLineString = NetTopologySuite.Geometries.MultiLineString;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal class MultiLineStringConverter : ISpatialConverter
{
    public bool CanConvert(EdmPrimitiveTypeKind primitiveTypeKind, Geometry geometry)
    {
        return geometry is MultiLineString
        && (primitiveTypeKind == EdmPrimitiveTypeKind.GeometryMultiLineString
        || primitiveTypeKind == EdmPrimitiveTypeKind.GeographyMultiLineString);
    }

    public ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType)
    {
        SpatialConverter converter = SpatialConverter.For(primitiveType, geometry.SRID);

        return converter.Convert((MultiLineString)geometry);
    }
}
