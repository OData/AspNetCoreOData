// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing.Attributes
{
    /// <summary>
    /// Represents an attribute that can be placed on an action of an Controller to specify
    /// the OData URLs that the action handles.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ODataRouteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteAttribute"/> class.
        /// </summary>
        public ODataRouteAttribute()
            : this(template: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteAttribute"/> class.
        /// </summary>
        /// <param name="template">The OData URL path template that this action handles, it could be null.</param>
        public ODataRouteAttribute(string template)
            : this(template, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutePrefixAttribute"/> class.
        /// </summary>
        /// <param name="template">The OData URL path template that this action handles. It could be null, For example: "customers({key})".</param>
        /// <param name="prefix">The OData routing prefix setting. For example: "v{version}". It could be null which means this attribute be suitable for routes.</param>
        public ODataRouteAttribute(string template, string prefix)
        {
            PathTemplate = template ?? string.Empty;

            // It could be null which means this attribute can apply for all routes.
            RoutePrefix = prefix;
        }

        /// <summary>
        /// Gets the OData URL path template that this action handles.
        /// </summary>
        public string PathTemplate { get; }

        /// <summary>
        /// Gets or sets the OData route with which to associate the attribute.
        /// </summary>
        public string RoutePrefix { get; }
    }
}
