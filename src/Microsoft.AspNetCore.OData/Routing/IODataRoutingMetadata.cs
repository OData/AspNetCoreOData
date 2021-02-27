// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
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
        /// Gets the supported Http methods.
        /// </summary>
        ISet<string> HttpMethods { get; }

        /// <summary>
        /// Gets/sets the template display name.
        /// </summary>
        public string TemplateDisplayName { get; set; }

        /// <summary>
        /// Gets the OData path template
        /// </summary>
        ODataPathTemplate Template { get; }
    }
}
