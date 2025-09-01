//-----------------------------------------------------------------------------
// <copyright file="ODataSpatialSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization;

// TODO: Add to declared API when finalized
/// <summary>
/// Represents an <see cref="ODataSerializer"/> for serializing spatial types.
/// </summary>
public class ODataSpatialSerializer : ODataPrimitiveSerializer
{
    /// <summary>
    /// Initializes a new instance of <see cref="ODataSpatialSerializer"/>.
    /// </summary>
    public ODataSpatialSerializer()
        : base()
    {
    }
}
