// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// An interface for managing per-route (prefix) service containers.
    /// </summary>
    public interface IPerRouteContainer
    {
        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// Gets the services dictionary.
        /// </summary>
        IDictionary<string, IServiceProvider> Services { get; }

        /// <summary>
        /// Get the root service provider for a given route (prefix) name.
        /// </summary>
        /// <param name="routeName">The route name (the route prefix name).</param>
        /// <returns>The root service provider for the route (prefix) name.</returns>
        IServiceProvider GetServiceProvider(string routeName);
    }
}
