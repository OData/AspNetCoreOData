// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing.Attributes
{
    /// <summary>
    /// Represents an attribute that can be placed on an OData controller to specify
    /// the route template prefix that will be used for all actions of that controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ODataRoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutePrefixAttribute"/> class.
        /// </summary>
        /// <param name="template">The OData URL path template that this controller handles.For example: "customers({key})".</param>
        public ODataRoutePrefixAttribute(string template)
            : this (template, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutePrefixAttribute"/> class.
        /// </summary>
        /// <param name="template">The OData URL path template that this controller handles. For example: "customers({key})".</param>
        /// <param name="prefix">The OData routing prefix setting. For example: "v{version}". It could be null which means this attribute be suitable for routes.</param>
        public ODataRoutePrefixAttribute(string template, string prefix)
        {
            if (string.IsNullOrEmpty(template))
            {
                throw Error.ArgumentNullOrEmpty(nameof(template));
            }

            PathPrefixTemplate = template;

            // It could be null which means this attribute can apply for all routes.
            RoutePrefix = prefix;
        }

        /// <summary>
        /// Gets the OData URL path prefix template that this controller handles.
        /// </summary>
        public string PathPrefixTemplate { get; }

        /// <summary>
        /// Gets or sets the route prefix with which to associate the attribute.
        /// </summary>
        public string RoutePrefix { get; }
    }
}
