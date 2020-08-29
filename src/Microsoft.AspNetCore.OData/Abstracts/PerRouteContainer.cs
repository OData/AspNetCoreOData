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
        /// <param name="options"></param>
        public PerRouteContainer(IOptions<ODataOptions> options)
        {
            _options = options;
            Initialize();
        }

        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>
        /// </summary>
        public Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, IServiceProvider> Services => _perPrefixContainers;

        /// <summary>
        /// Create a root container for a given route name.
        /// </summary>
        /// <param name="routeName">The route name.</param>
        /// <param name="configureAction">The configuration actions to apply to the container.</param>
        /// <returns>An instance of <see cref="IServiceProvider"/> to manage services for a route.</returns>
        public virtual IServiceProvider CreateServiceProvider(string routeName, Action<IContainerBuilder> configureAction)
        {
            IContainerBuilder builder = CreateContainerBuilderWithCoreServices();

            configureAction?.Invoke(builder);

            IServiceProvider serviceProvider = builder.BuildContainer();
            if (serviceProvider == null)
            {
                throw Error.InvalidOperation(SRResources.NullContainer);
            }

            _perPrefixContainers.AddOrUpdate(routeName, serviceProvider, (k, v) => serviceProvider);

            return serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routeName"></param>
        /// <returns></returns>
        public virtual IServiceProvider GetServiceProvider(string routeName)
        {
            IServiceProvider rootContainer;
            if (_perPrefixContainers.TryGetValue(routeName, out rootContainer))
            {
                return rootContainer;
            }

            return null;
        }

        /// <summary>
        /// Create a container builder with the default OData services.
        /// </summary>
        /// <returns>An instance of <see cref="IContainerBuilder"/> to manage services.</returns>
        protected IContainerBuilder CreateContainerBuilderWithCoreServices()
        {
            IContainerBuilder builder;
            if (this.BuilderFactory != null)
            {
                builder = this.BuilderFactory();
                if (builder == null)
                {
                    throw Error.InvalidOperation(SRResources.NullContainerBuilder);
                }
            }
            else
            {
                builder = new DefaultContainerBuilder();
            }

            builder.AddDefaultODataServices();

            // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
            builder.AddService(
                ServiceLifetime.Singleton,
                typeof(ODataUriResolver),
                sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

            return builder;
        }

        internal void Initialize()
        {
            _perPrefixContainers = new ConcurrentDictionary<string, IServiceProvider>();

            foreach (var config in _options.Value.Models)
            {
                IEdmModel model = config.Value.Item1;
                var serviceBuilder = config.Value.Item2;

                IContainerBuilder odataContainerBuilder = null;
                //if (_serviceProvider != null)
                //{
                //    odataContainerBuilder = _serviceProvider.GetService<IContainerBuilder>();
                //}

                //if (odataContainerBuilder == null)
                //{
                //    odataContainerBuilder = new DefaultContainerBuilder();
                //}

                if (this.BuilderFactory != null)
                {
                    odataContainerBuilder = this.BuilderFactory();
                    if (odataContainerBuilder == null)
                    {
                        throw Error.InvalidOperation(SRResources.NullContainerBuilder);
                    }
                }
                else
                {
                    odataContainerBuilder = new DefaultContainerBuilder();
                }

                odataContainerBuilder.AddDefaultODataServices();

                serviceBuilder?.Invoke(odataContainerBuilder);

                // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
                odataContainerBuilder.AddService(ServiceLifetime.Singleton,
                    typeof(ODataUriResolver),
                    sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

                odataContainerBuilder.AddService(ServiceLifetime.Singleton, sp => model);

                _perPrefixContainers[config.Key] = odataContainerBuilder.BuildContainer();
            }
        }
    }
}
