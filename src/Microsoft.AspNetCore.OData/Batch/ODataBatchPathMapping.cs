// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Microsoft.AspNetCore.OData.Batch
{
    /// <summary>
    /// A class for storing batch route names and prefixes used to determine if a route is a
    /// batch route.
    /// </summary>
    internal class ODataBatchPathMapping
    {
        private Dictionary<TemplateMatcher, (string, ODataBatchHandler)> templateMappings = new Dictionary<TemplateMatcher, (string, ODataBatchHandler)>();

        /// <summary>
        /// Add a route name and template for batching.
        /// </summary>
        /// <param name="prefixName">The route prefix name.</param>
        /// <param name="routeTemplate">The route template.</param>
        /// <param name="handler">The batch handler.</param>
        public void AddRoute(string prefixName, string routeTemplate, ODataBatchHandler handler)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull(nameof(routeTemplate));
            }

            string newRouteTemplate = routeTemplate.StartsWith("/", StringComparison.Ordinal) ? routeTemplate.Substring(1) : routeTemplate;
            RouteTemplate parsedTemplate = TemplateParser.Parse(newRouteTemplate);
            TemplateMatcher matcher = new TemplateMatcher(parsedTemplate, new RouteValueDictionary());
            templateMappings[matcher] = (prefixName, handler);
        }

        /// <summary>
        /// Try and get the batch handler for a given path.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="prefixName">The route/prefix name if found or null.</param>
        /// <param name="handler">The batch handler.</param>
        /// <returns>true if a route name is found, otherwise false.</returns>
        public bool TryGetPrefixName(HttpContext context, out string prefixName, out ODataBatchHandler handler)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            prefixName = null;
            handler = null;
            string path = context.Request.Path;
            foreach (var item in templateMappings)
            {
                RouteValueDictionary routeData = new RouteValueDictionary();
                if (item.Key.TryMatch(path, routeData))
                {
                    prefixName = item.Value.Item1;
                    handler = item.Value.Item2;
                    if (routeData.Count > 0)
                    {
                        Merge(context.ODataFeature().BatchRouteData, routeData);
                    }

                    return true;
                }
            }

            return false;
        }

        private static void Merge(RouteValueDictionary batchRouteData, RouteValueDictionary routeData)
        {
            foreach (var item in routeData)
            {
                batchRouteData.Add(item.Key, item.Value);
            }
        }
    }
}
