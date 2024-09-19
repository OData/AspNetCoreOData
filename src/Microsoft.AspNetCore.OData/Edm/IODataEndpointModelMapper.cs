//-----------------------------------------------------------------------------
// <copyright file="IODataEndpointModelMapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// A cache for <see cref="Endpoint"/> and <see cref="IEdmModel"/> mapping.
    /// It's typically used for minimal API scenario.
    /// </summary>
    public interface IODataEndpointModelMapper
    {
        /// <summary>
        /// Gets the map between <see cref="Endpoint"/> and <see cref="IEdmModel"/>
        /// </summary>
        ConcurrentDictionary<Endpoint, IEdmModel> Maps { get; }
    }
}
