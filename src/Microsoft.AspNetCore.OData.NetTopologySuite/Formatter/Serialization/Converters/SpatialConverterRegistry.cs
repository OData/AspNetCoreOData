//-----------------------------------------------------------------------------
// <copyright file="SpatialConverterRegistry.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Common;
using Microsoft.OData.Edm;
using Geometry = NetTopologySuite.Geometries.Geometry;
using ISpatial = Microsoft.Spatial.ISpatial;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Formatter.Serialization.Converters;

internal sealed class SpatialConverterRegistry : ISpatialConverterRegistry
{
    private readonly List<ISpatialConverter> converters;

    public SpatialConverterRegistry(IEnumerable<ISpatialConverter> converters)
    {
        this.converters = new List<ISpatialConverter>(converters);
    }

    public ISpatial Convert(Geometry geometry, IEdmPrimitiveTypeReference primitiveType)
    {
        for (int i = 0; i < this.converters.Count; i++)
        {
            if (this.converters[i].CanConvert(primitiveType.PrimitiveKind(), geometry))
            {
                return this.converters[i].Convert(geometry, primitiveType);
            }
        }

        throw Error.InvalidOperation(SRResources.CannotWriteType, typeof(ODataSpatialNetTopologySuiteSerializer), geometry.GetType().FullName);
    }
}
