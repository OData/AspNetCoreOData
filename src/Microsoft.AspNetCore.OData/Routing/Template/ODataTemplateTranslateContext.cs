// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataTemplateTranslateContext" /> class.
        /// For Unit test only.
        /// </summary>
        internal ODataTemplateTranslateContext()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataTemplateTranslateContext" /> class.
        /// For Unit test only.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        internal ODataTemplateTranslateContext(HttpContext context)
        {
            HttpContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataTemplateTranslateContext" /> class.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <param name="routeValues">The route values.</param>
        /// <param name="model">The Edm model.</param>
        internal ODataTemplateTranslateContext(HttpContext context, RouteValueDictionary routeValues, IEdmModel model)
        {
            HttpContext = context ?? throw new ArgumentNullException(nameof(context));

            RouteValues = routeValues ?? throw new ArgumentNullException(nameof(routeValues));

            Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>
        /// Gets the current HttpContext.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets the route values.
        /// </summary>
        public RouteValueDictionary RouteValues { get; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the query string using the key.
        /// </summary>
        /// <param name="key">The query string key.</param>
        /// <returns>Null or the string value of the query.</returns>
        public StringValues GetQueryString(string key)
        {
            HttpContext.Request.Query.TryGetValue(key, out StringValues values);
            return values;
        }
    }
}
