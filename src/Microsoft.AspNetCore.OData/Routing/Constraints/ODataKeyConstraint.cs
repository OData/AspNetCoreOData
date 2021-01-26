// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Routing.Constraints
{
    /// <summary>
    /// Customers({odataKey:odataKeys(firstName;LastName)})
    /// </summary>
    internal class ODataKeyConstraint : ODataRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataKeyConstraint" /> class.
        /// </summary>
        /// <param name="keys">the key string, separated using ';'.</param>
        public ODataKeyConstraint(string keys)
            : base(keys)
        {
        }

        /// <inheritdoc />
        public override bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (routeDirection != RouteDirection.IncomingRequest)
            {
                return false;
            }

            // the incoming request looks like:
            // ~/odata/Customers('abc')
            // the route value dictionary contains: key='abc'
            // KeyValuePairParser is used to split "'abc'"
            // 1)  string.Empty -> "'abc'"
            // the constraint looks like:  {key:odataKeys(customerId)}
            // It's simply to compare the paremters between in the constraint and in the real request.
            if (values.TryGetValue(routeKey, out var value) && value != null)
            {
                string parameterValueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (parameterValueString == null)
                {
                    return false;
                }

                if (KeyValuePairParser.TryParse(parameterValueString, out IDictionary<string, string> parsedKeys))
                {
                    // If it's single key, directly use the key value without the key
                    if (parsedKeys.Count == 1 && parsedKeys.ContainsKey(string.Empty) &&
                        Parameters.Count == 1)
                    {
                        values[Parameters.ElementAt(0)] = parsedKeys[string.Empty];
                        return true;
                    }

                    // id=42
                    if (Parameters.SetEquals(parsedKeys.Keys))
                    {
                        foreach (var item in parsedKeys)
                        {
                            values[item.Key] = item.Value;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public static string ConstraintName = "odatakeys";

        /// <summary>
        /// Format the key constraint pattern.
        /// </summary>
        /// <param name="keys">The keys string.</param>
        /// <returns>The built key constraint pattern.</returns>
        public static string FormatConstraint(IEnumerable<string> keys)
        {
            // =>  ({key:odatakeys(p1;p2;...)})
            if (keys == null)
            {
                return $"{{key:{ConstraintName}()}}";
            }

            string combinedParams = string.Join(";", keys);
            return $"{{key:{ConstraintName}({combinedParams})}}";
        }
    }
}
