// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// Provide the interface for the details of a given OData request.
    /// </summary>
    public interface IODataFeature
    {
        /// <summary>
        /// Gets or sets the OData path.
        /// </summary>
        IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the OData path.
        /// </summary>
        ODataPath Path { get; set; }

        /// <summary>
        /// Gets/sets the route name
        /// </summary>
        string RouteName { get; set; }

        /// <summary>
        /// Gets/sets the endpoint selected
        /// </summary>
        EndPoint Endpoint { get; set; }
    }
}
