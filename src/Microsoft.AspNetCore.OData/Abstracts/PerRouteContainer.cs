// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// A class for managing per-route service containers.
    /// </summary>
    public class PerRouteContainer : IPerRouteContainer
    {
        private ConcurrentDictionary<string, IServiceProvider> _perPrefixContainers;
        private readonly IOptions<ODataOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerRouteContainer"/> class.
        /// </summary>
        /// <param name="options">The global OData options.</param>
        public PerRouteContainer(IOptions<ODataOptions> options)
        {
            _options = options;
            Initialize();
        }

        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        /// <remarks>
        /// Consider to move this into service collection.
        /// </remarks>
        public Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// Gets the services dictionary.
        /// </summary>
        public virtual IDictionary<string, IServiceProvider> Services => _perPrefixContainers;

        /// <summary>
        /// Get the root service provider for a given route (prefix) name.
        /// </summary>
        /// <param name="routeName">The route name (the route prefix name).</param>
        /// <returns>The root service provider for the route (prefix) name.</returns>
        public virtual IServiceProvider GetServiceProvider(string routeName)
        {
            return _perPrefixContainers.GetValueOrDefault(routeName);
        }

        /// <summary>
        /// Initalize the per-route container.
        /// </summary>
        private void Initialize()
        {
            //_perPrefixContainers = new ConcurrentDictionary<string, IServiceProvider>();

            //foreach (var config in _options.Value.Models)
            //{
            //    IEdmModel model = config.Value.Item1;
            //    Action<IContainerBuilder> serviceBuilder = config.Value.Item2;

            //    IContainerBuilder odataContainerBuilder = null;
            //    if (this.BuilderFactory != null)
            //    {
            //        odataContainerBuilder = this.BuilderFactory();
            //        if (odataContainerBuilder == null)
            //        {
            //            throw Error.InvalidOperation(SRResources.NullContainerBuilder);
            //        }
            //    }
            //    else
            //    {
            //        odataContainerBuilder = new DefaultContainerBuilder();
            //    }

            //    odataContainerBuilder.AddDefaultODataServices();

            //    serviceBuilder?.Invoke(odataContainerBuilder);

            //    // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
            //    odataContainerBuilder.AddService(ServiceLifetime.Singleton,
            //        typeof(ODataUriResolver),
            //        sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

            //    odataContainerBuilder.AddService(ServiceLifetime.Singleton, sp => model);

            //    _perPrefixContainers[config.Key] = odataContainerBuilder.BuildContainer();
            //}
        }
    }
}
