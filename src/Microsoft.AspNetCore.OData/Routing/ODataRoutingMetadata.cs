// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Represents OData Endpoint metadata used during routing.
    /// </summary>
    public sealed class ODataRoutingMetadata : IODataRoutingMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingMetadata"/> class.
        /// </summary>
        /// <param name="prefix">The prefix string.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="template">The Routing path template.</param>
        public ODataRoutingMetadata(string prefix, IEdmModel model, ODataPathTemplate template)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingMetadata"/> class.
        /// </summary>
        /// <param name="prefix">The prefix string.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="template">The Routing path template.</param>
        public ODataRoutingMetadata(string prefix, IEdmModel model, string template)
        {
            Prefix = prefix;
            Model = model;
            StrTemplate = template;
        }

        /// <summary>
        /// Gets the prefix string.
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Gets the Edm model.
        /// </summary>
        public IEdmModel Model { get; }

        /// <summary>
        /// Gets the template string.
        /// </summary>
        public string StrTemplate { get; }

        /// <summary>
        /// Gets the OData path template
        /// </summary>
        public ODataPathTemplate Template { get; }

        // { { "$filter", "IntProp eq @p1" }, { "@p1", "@p2" }, { "@p2", "123" } });
        /// <summary>
        /// Generate the real <see cref="ODataPath"/> based on the template the route values.
        /// </summary>
        /// <param name="values">The route values.</param>
        /// <param name="queryString">The query string.</param>
        /// <returns>The built <see cref="ODataPath" />.</returns>
        public ODataPath GenerateODataPath(RouteValueDictionary values, QueryString queryString)
        {
            if (Template != null)
            {
                return Template.GenerateODataPath(Model, values, queryString);
            }

            return null;
        }
    }
}
