//-----------------------------------------------------------------------------
// <copyright file="ODataTemplateTranslateContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
        /// </summary>
        /// <param name="context">The current HttpContext.</param>
        /// <param name="endpoint">The current endpoint to match.</param>
        /// <param name="routeValues">The current route values.</param>
        /// <param name="model">The current Edm model.</param>
        public ODataTemplateTranslateContext(HttpContext context, Endpoint endpoint, RouteValueDictionary routeValues, IEdmModel model)
        {
            HttpContext = context ?? throw Error.ArgumentNull(nameof(context));

            Endpoint = endpoint ?? throw Error.ArgumentNull(nameof(endpoint));

            RouteValues = routeValues ?? throw Error.ArgumentNull(nameof(routeValues));

            Model = model ?? throw Error.ArgumentNull(nameof(model));
        }

        /// <summary>
        /// Gets the current Endpoint <see cref="Endpoint"/>.
        /// </summary>
        /// <remarks>
        /// The internal setter is provided for unit test purposes only.
        /// </remarks>
        public Endpoint Endpoint { get; internal set; }

        /// <summary>
        /// Gets the current HttpContext <see cref="HttpContext"/>.
        /// </summary>
        /// <remarks>
        /// The internal setter is provided for unit test purposes only.
        /// </remarks>
        public HttpContext HttpContext { get; internal set; }

        /// <summary>
        /// Gets the route values <see cref="RouteValueDictionary"/>.
        /// </summary>
        /// <remarks>
        /// The internal setter is provided for unit test purposes only.
        /// </remarks>
        public RouteValueDictionary RouteValues { get; internal set; }

        /// <summary>
        /// Gets the Edm model <see cref="IEdmModel"/>.
        /// </summary>
        /// <remarks>
        /// The internal setter is provided for unit test purposes only.
        /// </remarks>
        public IEdmModel Model { get; internal set; }

        /// <summary>
        /// Gets the updated route values. This will include the updated route values.
        /// </summary>
        public RouteValueDictionary UpdatedValues { get; } = new RouteValueDictionary();

        /// <summary>
        /// Gets the generated path segments.
        /// </summary>
        public IList<ODataPathSegment> Segments { get; } = new List<ODataPathSegment>();

        /// <summary>
        /// Gets the parameter alias or the alias name itself.
        /// </summary>
        /// <param name="alias">The potential alias name.</param>
        /// <returns>The parameter alias name.</returns>
        public string GetParameterAliasOrSelf(string alias)
        {
            var set = new HashSet<string>();
            string value = GetParameterAliasOrSelf(alias, set);
            if (set.Count > 1)
            {
                // Since it returns from query, should unescape the string.
                return Uri.UnescapeDataString(value);
            }

            return value;
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

        internal bool IsPartOfRouteTemplate(string part)
        {
            string template = null;
            RouteEndpoint routeEndpoint = Endpoint as RouteEndpoint;
            if (routeEndpoint != null)
            {
                template = routeEndpoint.RoutePattern.RawText;
            }

            if (template != null)
            {
                return template.Contains(part, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
