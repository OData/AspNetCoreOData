//-----------------------------------------------------------------------------
// <copyright file="GeometryCollectionConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using ISpatial = Microsoft.Spatial.ISpatial;
using GeometryCollection = NetTopologySuite.Geometries.GeometryCollection;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal class GeometryCollectionConverter : ISpatialConverter
{
    public bool CanConvert(EdmPrimitiveTypeKind primitiveTypeKind, Geometry geometry)
    {
        return geometry is GeometryCollection
            && (primitiveTypeKind == EdmPrimitiveTypeKind.GeometryCollection
            || primitiveTypeKind == EdmPrimitiveTypeKind.GeographyCollection);
    }

    public ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType)
    {
        SpatialConverter converter = SpatialConverter.For(primitiveType, geometry.SRID);

        return converter.Convert((GeometryCollection)geometry);
    }
}
