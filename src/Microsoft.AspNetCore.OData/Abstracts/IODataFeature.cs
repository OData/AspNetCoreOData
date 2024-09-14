//-----------------------------------------------------------------------------
// <copyright file="IODataFeature.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Abstracts;

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
    /// Gets/sets the route prefix name
    /// </summary>
    string RoutePrefix { get; set; }

    /// <summary>
    /// Gets/sets the endpoint selected
    /// </summary>
    EndPoint Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the OData base address.
    /// </summary>
    string BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the request scope.
    /// </summary>
    IServiceScope RequestScope { get; set; }

    /// <summary>
    /// Gets or sets the request container.
    /// </summary>
    IServiceProvider Services { get; set; }

    /// <summary>
    /// Gets or sets the batch route data.
    /// </summary>
    RouteValueDictionary BatchRouteData { get; }

    /// <summary>
    /// Gets or sets the total count for the OData response.
    /// </summary>
    /// <value><c>null</c> if no count should be sent back to the client.</value>
    long? TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total count function for the OData response.
    /// </summary>
    Func<long> TotalCountFunc { get; set; }

    /// <summary>
    /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
    /// </summary>
    ApplyClause ApplyClause { get; set; }

    /// <summary>
    /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
    /// </summary>
    SelectExpandClause SelectExpandClause { get; set; }

    /// <summary>
    /// Gets or sets the next link for the OData response.
    /// </summary>
    Uri NextLink { get; set; }

    /// <summary>
    /// Gets or sets the delta link for the OData response.
    /// </summary>
    Uri DeltaLink { get; set; }

    /// <summary>
    /// Gets the data store used by routing conventions to store any custom route data.
    /// </summary>
    /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
    IDictionary<string, object> RoutingConventionsStore { get; }
}
