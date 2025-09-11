//-----------------------------------------------------------------------------
// <copyright file="ISpatialConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using NetTopologySuite.Geometries;
using ISpatial = Microsoft.Spatial.ISpatial;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal interface ISpatialConverter
{
    bool CanConvert(EdmPrimitiveTypeKind primitiveTypeKind, Geometry geometry);
    ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType);
}
