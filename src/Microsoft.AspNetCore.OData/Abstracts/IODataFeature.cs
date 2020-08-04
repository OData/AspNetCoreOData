// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
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
        /// Gets or sets the OData model.
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

        /// <summary>
        /// Gets or sets the Url helper.
        /// </summary>
        IUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets or sets the total count for the OData response.
        /// </summary>
        /// <value><c>null</c> if no count should be sent back to the client.</value>
        long? TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        SelectExpandClause SelectExpandClause { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IDictionary<string, object> BodyValues { get; set; }
    }
}
