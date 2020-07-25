// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// The context used to generate the <see cref="ODataPathSegment"/>.
    /// </summary>
    public class ODataSegmentTemplateTranslateContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="request"></param>
        public ODataSegmentTemplateTranslateContext(IEdmModel model, HttpRequest request)
        {
            Model = model;
            Request = request;
        }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the current HttpContext.
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// Gets the route values.
        /// </summary>
        public RouteValueDictionary RouteValues => Request.RouteValues;

        /// <summary>
        /// Gets the previous navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }
    }
}
