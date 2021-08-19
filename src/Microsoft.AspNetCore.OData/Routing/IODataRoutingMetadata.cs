//-----------------------------------------------------------------------------
// <copyright file="IODataRoutingMetadata.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Defines a contract use to specify the OData routing related metadata in <see cref="Endpoint.Metadata"/>.
    /// </summary>
    public interface IODataRoutingMetadata
    {
        /// <summary>
        /// Gets the prefix string.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        IEdmModel Model { get; }

        /// <summary>
        /// Gets the OData path template
        /// </summary>
        ODataPathTemplate Template { get; }
    }
}
