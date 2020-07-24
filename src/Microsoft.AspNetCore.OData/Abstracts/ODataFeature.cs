// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        /// <summary>
        /// Instantiates a new instance of the <see cref="ODataFeature"/> class.
        /// </summary>
        public ODataFeature()
        {
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
        /// 
        /// </summary>
        public IDictionary<string, object> BodyValues { get; set; }
    }
}
