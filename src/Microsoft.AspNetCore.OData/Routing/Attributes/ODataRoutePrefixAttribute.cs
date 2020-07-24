// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Routing.Attributes
{
    /// <summary>
    /// Represents an attribute that can be placed on an OData controller to specify
    /// the prefix that will be used for all actions of that controller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ODataRoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutePrefixAttribute"/> class.
        /// </summary>
        /// <param name="routePrefix">The OData URL path template that this action handles.</param>
        public ODataRoutePrefixAttribute(string routePrefix)
        {
            RoutePrefix = routePrefix ?? throw new ArgumentNullException(nameof(routePrefix));
        }

        /// <summary>
        /// Gets the OData URL path template that this action handles.
        /// </summary>
        public string RoutePrefix { get; }

        /// <summary>
        /// Gets or sets the model name with which to associate the attribute.
        /// </summary>
        public string ModelName { get; set; }
    }
}
