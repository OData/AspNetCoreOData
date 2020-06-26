// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// An interface for configuring essential OData services.
    /// </summary>
    /// <summary>
    /// Allows fine grained configuration of essential OData services.
    /// </summary>
    internal class DefaultODataBuilder : IODataBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public DefaultODataBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the services collection.
        /// </summary>
        public IServiceCollection Services { get; }
    }
}
