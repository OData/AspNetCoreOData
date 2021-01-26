// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Routing.Constraints
{
    /// <summary>
    /// The OData routing constaint. It accepts the parameter in the constraint separated using ';'.
    /// </summary>
    internal abstract class ODataRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteConstraint" /> class.
        /// </summary>
        /// <param name="parameters">the key and parameter string, separated using ';'.</param>
        public ODataRouteConstraint(string parameters)
        {
            if (parameters == null)
            {
                Parameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                Parameters = new HashSet<string>(parameters.Split(";"), StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        protected ISet<string> Parameters { get; }

        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <param name="route">The router that this constraint belongs to.</param>
        /// <param name="routeKey">The name of the parameter that is being checked.</param>
        /// <param name="values">A dictionary that contains the parameters for the URL.</param>
        /// <param name="routeDirection">An object that indicates whether the constraint check is being performed when
        ///     an incoming request is being handled or when a URL is being generated.
        /// </param>
        /// <returns>true if the URL parameter contains a valid value; otherwise, false.</returns>
        public abstract bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection);
    }
}
