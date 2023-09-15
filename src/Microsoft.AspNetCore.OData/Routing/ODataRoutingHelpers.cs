//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    ///  OData routing helper methods.
    /// </summary>
    internal static class ODataRoutingHelpers
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="routeTemplate">TODO</param>
        /// <param name="routePrefixes">TODO</param>
        /// <param name="sanitizedRouteTemplate">TODO</param>
        /// <returns></returns>
        public static string FindRoutePrefix(string routeTemplate, IEnumerable<string> routePrefixes, out string sanitizedRouteTemplate)
        {
            if (routeTemplate.StartsWith('/'))
            {
                routeTemplate = routeTemplate.Substring(1);
            }
            else if (routeTemplate.StartsWith("~/", StringComparison.Ordinal))
            {
                routeTemplate = routeTemplate.Substring(2);
            }

            // The route template could take the following forms:
            // #1) nonemptyprefix/Customers/{key} - matches the "nonemptyprefix" prefix route
            // #2) Customers({key}) - matches the empty prefix route
            //     - #1 matches the "nonemptyprefix" prefix route
            //     - #2 matches the empty prefix route
            // Since #1 and #2 can be considered to contain the empty prefix,
            // we compare the non-empty route prefixes first.
            // If no match, then we compare empty route prefix.
            string emptyPrefix = null;

            foreach (var prefix in routePrefixes)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    emptyPrefix = prefix;

                    continue;
                }
                else if (routeTemplate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    // "nonemptyprefix/Customers/{key}" scenario - remove the "nonemptyprefix" route prefix
                    sanitizedRouteTemplate = routeTemplate.Substring(prefix.Length);

                    // Route template could be "nonemptyprefix".
                    // This check is necessary since the sanitized route template would be an empty string in such a scenario.
                    if (sanitizedRouteTemplate.StartsWith("/", StringComparison.Ordinal))
                    {
                        sanitizedRouteTemplate = sanitizedRouteTemplate.Substring(1);
                    }

                    return prefix;
                }
            }

            // we are here because no non-empty prefix matches.
            if (emptyPrefix != null)
            {
                // So, if we have empty prefix route, it could match all OData route template.
                sanitizedRouteTemplate = routeTemplate;

                return emptyPrefix;
            }

            sanitizedRouteTemplate = null;

            return null;
        }
    }
}
