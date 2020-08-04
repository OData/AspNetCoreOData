// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Microsoft.AspNetCore.OData.Abstracts
{
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
        /// Gets or sets the route name.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets or sets the Url helper.
        /// </summary>
        public IUrlHelper UrlHelper { get; set; }

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
        /// Gets or sets the parsed OData <see cref="SelectExpandClause"/> of the request.
        /// </summary>
        public SelectExpandClause SelectExpandClause { get; set; }

        /// <summary>
        /// Gets or sets the parsed <see cref="ODataQueryOptions"/> of the request.
        /// </summary>
        internal ODataQueryOptions QueryOptions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> BodyValues { get; set; }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <value>Initially an empty <c>IDictionary&lt;string, object&gt;</c>.</value>
        public IDictionary<string, object> RoutingConventionsStore { get; set; } = new Dictionary<string, object>();
    }
}
