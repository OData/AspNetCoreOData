//-----------------------------------------------------------------------------
// <copyright file="ODataFeature.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.UriParser.Aggregation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Abstracts;

/// <summary>
/// Contains the details of a given OData request. These properties should all be mutable.
/// None of these properties should ever be set to null.
/// </summary>
public class ODataFeature : IODataFeature
{
    internal const string ODataServiceVersionHeader = "OData-Version";
    internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

    private long? _totalCount;
    private bool _totalCountSet;

    /// <summary>
    /// Instantiates a new instance of the <see cref="ODataFeature"/> class.
    /// </summary>
    public ODataFeature()
    {
        _totalCountSet = false;
    }

    /// <summary>
    /// Gets or sets the OData path.
    /// </summary>
    public IEdmModel Model { get; set; }

    /// <summary>
    /// Gets or sets the OData path.
    /// </summary>
    public ODataPath Path { get; set; }

    /// <summary>
    /// Add a boolean value indicate whether it's endpoint routing or not.
    /// Maybe it's unnecessary later.
    /// </summary>
    public EndPoint Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the route prefix name.
    /// </summary>
    public string RoutePrefix { get; set; }

    /// <summary>
    /// Gets or sets the OData base address.
    /// </summary>
    public string BaseAddress { get; set; }

    /// <summary>
    /// Gets or sets the request scope.
    /// </summary>
    public IServiceScope RequestScope { get; set; }

    /// <summary>
    /// Gets or sets the request container.
    /// </summary>
    public IServiceProvider Services { get; set; }

    /// <summary>
    /// Gets or sets the batch route data.
    /// </summary>
    public RouteValueDictionary BatchRouteData { get; } = new RouteValueDictionary();

    /// <summary>
    /// Gets or sets the total count for the OData response.
    /// </summary>
    /// <value><c>null</c> if no count should be sent back to the client.</value>
    public long? TotalCount
    {
        get
        {
            if (_totalCountSet)
            {
                return _totalCount;
            }

            if (this.TotalCountFunc != null)
            {
                _totalCount = this.TotalCountFunc();
                _totalCountSet = true;
                return _totalCount;
            }

            return null;
        }
        set
        {
            _totalCount = value;
            _totalCountSet = value.HasValue;
        }
    }

    /// <summary>
    /// Gets or sets the total count function for the OData response.
    /// </summary>
    public Func<long> TotalCountFunc { get; set; }

    /// <summary>
    /// Gets or sets the parsed OData <see cref="ApplyClause"/> of the request.
    /// </summary>
    public ApplyClause ApplyClause { get; set; }

    /// <summary>
    /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
    /// </summary>
    public SelectExpandClause SelectExpandClause { get; set; }

    /// <summary>
    /// Gets or sets the next link for the OData response.
    /// </summary>
    public Uri NextLink { get; set; }

    /// <summary>
    /// Gets or sets the delta link for the OData response.
    /// </summary>
    public Uri DeltaLink { get; set; }

    /// <summary>
    /// Gets or sets the parsed <see cref="ODataQueryOptions"/> of the request.
    /// </summary>
    internal ODataQueryOptions QueryOptions { get; set; }

    /// <summary>
    /// Page size to be used by skiptoken implementation for the top-level resource for the request. 
    /// </summary>
    internal int PageSize { get; set; }

    /// <summary>
    /// Gets the body values from OData request.
    /// </summary>
    internal IDictionary<string, object> BodyValues { get; set; }

    /// <summary>
    /// Gets the data store used routing conventions to store any custom route data.
    /// </summary>
    /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
    public IDictionary<string, object> RoutingConventionsStore { get; } = new Dictionary<string, object>();
}
