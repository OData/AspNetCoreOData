// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// The context used to generate the <see cref="ODataPathSegment"/>.
    /// </summary>
    public class ODataTemplateTranslateContext
    {
        internal ODataTemplateTranslateContext(HttpContext context, 
             RouteValueDictionary routeValues, IEdmModel model)
        {
            HttpContext = context;
            RouteValues = routeValues;
            Model = model;
        }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the current HttpContext.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the route values.
        /// </summary>
        public RouteValueDictionary RouteValues { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public StringValues GetQueryString(string key)
        {
            HttpContext.Request.Query.TryGetValue(key, out StringValues values);
            return values;
        }
    }
}
