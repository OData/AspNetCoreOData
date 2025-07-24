//-----------------------------------------------------------------------------
// <copyright file="ISpatialConverterRegistry.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using ISpatial = Microsoft.Spatial.ISpatial;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal interface ISpatialConverterRegistry
{
    public ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType);
}
