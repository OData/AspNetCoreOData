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
    /// NS.MyFunction({parameter:odataparams(p1;p2;p3)})
    /// MyFunction({parameter:odataparams(p1;p2;p3)})
    /// </summary>
    internal class ODataParameterConstraint : ODataRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataParameterConstraint" /> class.
        /// </summary>
        /// <param name="parameters">the key and parameter string, separated using ';'.</param>
        public ODataParameterConstraint(string parameters)
            : base(parameters)
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
            // ~/odata/Customers/Namespace.GetPrice(orgId='abc', depId=123)
            // the route value dictionary contains: parameter=orgId='abc', depId=123
            // KeyValuePairParser is used to split "orgId='abc', depId=123"
            // 1)  orgId -> "'abc'"
            // 2)  depId -> "123"
            // the constraint looks like:  {parameter:odataparams(orgId;depId)}
            // Algorithm: It's simply to compare the paremters between in the constraint and in the real request.
            if (values.TryGetValue(routeKey, out var value) && value != null)
            {
                string parameterValueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (parameterValueString == null)
                {
                    return false;
                }

                if (KeyValuePairParser.TryParse(parameterValueString, out IDictionary<string, string> parsedParameters))
                {
                    if (Parameters.SetEquals(parsedParameters.Keys))
                    {
                        foreach (var item in parsedParameters)
                        {
                            values[item.Key] = item.Value;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public static string ConstraintName = "odataparams";

        public static string FormatConstraint(IEnumerable<string> parameters)
        {
            // =>  ({parameter:odataParameters(p1;p2)})
            if (parameters == null)
            {
                return $"{{parameter:{ConstraintName}()}}";
            }

            string combinedParams = string.Join(";", parameters);
            return $"{{parameter:{ConstraintName}({combinedParams})}}";
        }
    }
}
