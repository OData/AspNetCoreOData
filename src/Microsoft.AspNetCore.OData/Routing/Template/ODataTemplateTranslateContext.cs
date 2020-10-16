// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
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
        /// Gets the parameter alias or the alias name itself.
        /// </summary>
        /// <param name="alias">The potential alias name.</param>
        /// <returns>The parameter alias name.</returns>
        public string GetParameterAliasOrSelf(string alias)
        {
            return GetParameterAliasOrSelf(alias, new HashSet<string>());
        }

        private string GetParameterAliasOrSelf(string alias, ISet<string> visited)
        {
            if (visited.Contains(alias))
            {
                // process "?@p1=@p2&@p2=@p1" infinite loop?
                throw new ODataException(Error.Format(SRResources.InfiniteParameterAlias, alias));
            }

            visited.Add(alias);

            if (alias == null)
            {
                return null;
            }

            // parameter alias starts with "@"
            if (!alias.StartsWith("@", StringComparison.Ordinal))
            {
                return alias;
            }

            if (!HttpContext.Request.Query.TryGetValue(alias, out StringValues values))
            {
                throw new ODataException(Error.Format(SRResources.MissingParameterAlias, alias));
            }

            alias = values;

            // Go to next level of parameter alias "?@p1=@p2&@p2=abc"
            return GetParameterAliasOrSelf(alias, visited);
        }
    }
}
