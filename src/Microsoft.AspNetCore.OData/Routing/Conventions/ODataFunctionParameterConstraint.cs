// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// NS.MyFunction({odataParameter:odataParameters(p1;p2;p3)})
    /// MyFunction({odataParameter:odataParameters(p1;p2;p3)})
    /// </summary>
    internal class ODataFunctionParameterConstraint : IRouteConstraint
    {
        private const string RouteKey = "odataParameter";

        private IEdmModel _model;
        private IEdmFunction _function;

        private ISet<string> _parameters;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameters"></param>
        public ODataFunctionParameterConstraint(string parameters)
        {
            if (parameters == null)
            {
                _parameters = new HashSet<string>();
            }
            else
            {
                _parameters = new HashSet<string>(parameters.Split(";"));
            }
        }


        public static string FormatConstraint(IDictionary<string, string> parameters)
        {
            // =>  ({odataParameter:odataParameters(p1;p2)})
            string combinedParams = string.Join(";", parameters.Select(p => p.Key));
            return $"({{myparameter:odataParameters({combinedParams})}})";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="route"></param>
        /// <param name="routeKey"></param>
        /// <param name="values"></param>
        /// <param name="routeDirection"></param>
        /// <returns></returns>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                // Get Route key from values?
                if (values.TryGetValue(routeKey, out var value) && value != null)
                {
                    // Why values doesn't contain the "routeKey" 's value???
                    //
                    return true;
                }
            }

            return false;
        }
    }
}
