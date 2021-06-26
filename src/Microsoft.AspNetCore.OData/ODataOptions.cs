// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Contains the details of a given OData request. These properties should all be mutable.
    /// </summary>
    /// <remarks>Caution: The properties in this class should not be <see langword="null"/>.</remarks>
    public class ODataOptions
    {
        #region Settings

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// By default, it supports key as segment only if the key is single key.
        /// </summary>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; set; } = ODataUrlKeyDelimiter.Slash;

        /// <summary>
        /// Gets or Sets a value indicating if batch requests should continue on error.
        /// By default, it's false.
        /// </summary>
        public bool EnableContinueOnErrorHeader { get; set; }

        /// <summary>
        /// Gets or Sets a value indicating if attribute routing is enabled or not.
        /// By default, it's enabled.
        /// </summary>
        public bool EnableAttributeRouting { get; set; } = true;

        /// <summary>
        /// Gets or sets a function to build an <see cref="IContainerBuilder"/>.
        /// Please call it before the "AddModel".
        /// </summary>
        public Func<IContainerBuilder> BuilderFactory { get; set; }

        /// <summary>
        /// Gets or sets a TimeZoneInfo for the <see cref="DateTime"/> serialization and deserialization.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// Gets the routing conventions.
        /// </summary>
        public IList<IODataControllerActionConvention> Conventions { get; } = new List<IODataControllerActionConvention>();

        /// <summary>
        /// Configure the route options.
        /// </summary>
        public ODataRouteOptions RouteOptions { get; } = new ODataRouteOptions();

        #endregion

        #region RouteComponents

        /// <summary>
        /// Contains the OData <see cref="IEdmModel"/> instances and Dependency Injection containers for specific routes.
        /// </summary>
        /// <remarks>Do not modify this collection yourself. Instead, use the .AddModel() methods for registering model instances."/></remarks>
        public IDictionary<string, (IEdmModel EdmModel, IServiceProvider ServiceProvider)> RouteComponents { get; } = new Dictionary<string, (IEdmModel, IServiceProvider)>();

        /// <summary>
        /// Adds an <see cref="IEdmModel"/> to the default route.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> to add to the default route.</param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions AddModel(IEdmModel model)
        {
            return AddModel(string.Empty, model, configureAction: null);
        }

        /// <summary>
        /// Adds an <see cref="IEdmModel"/>, as well as the given <see cref="ODataBatchHandler"/>, to the default route.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> to add to the default route.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/> instance to add to the default route.</param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions AddModel(IEdmModel model, ODataBatchHandler batchHandler)
        {
            return AddModel(string.Empty, model, builder => builder.AddService(ServiceLifetime.Singleton, sp => batchHandler));
        }

        /// <summary>
        /// Adds an <see cref="IEdmModel"/> to the specified route.
        /// </summary>
        /// <param name="prefix">The model related prefix. It could be null which means there's no prefix when access this model.</param>
        /// <param name="model">The <see cref="IEdmModel"/> to add to the specified route.</param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        /// <remarks></remarks>
        public ODataOptions AddModel(string prefix, IEdmModel model)
        {
            return AddModel(prefix, model, configureAction: null);
        }

        /// <summary>
        /// Adds an <see cref="IEdmModel"/>, as well as the given <see cref="ODataBatchHandler"/>, to the specified route.
        /// </summary>
        /// <param name="prefix">The model related prefix. It could be null which means there's no prefix when access this model.</param>
        /// <param name="model">The <see cref="IEdmModel"/> to add to the specified route.</param>
        /// <param name="batchHandler">The <see cref="ODataBatchHandler"/> instance to add to the specified route.</param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions AddModel(string prefix, IEdmModel model, ODataBatchHandler batchHandler)
        {
            return AddModel(prefix, model, builder => builder.AddService(ServiceLifetime.Singleton, sp => batchHandler));
        }

        /// <summary>
        /// Adds OData model using the service configuration.
        /// </summary>
        /// <param name="prefix">The model related prefix.</param>
        /// <param name="model">The <see cref="IEdmModel"/> to add to the specified route.</param>
        /// <param name="configureAction">
        /// A <see cref="Action{IContainerBuilder}"/> lambda method that allows you to add services directly to the DI container just prior to finalization.
        /// </param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions AddModel(string prefix, IEdmModel model, Action<IContainerBuilder> configureAction)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (RouteComponents.ContainsKey(prefix))
            {
                throw Error.InvalidOperation(SRResources.ModelPrefixAlreadyUsed, prefix);
            }

            // Consider to use Lazy<IServiceProvider> ?
            IServiceProvider serviceProvider = BuildRouteContainer(model, configureAction);
            RouteComponents[prefix] = (model, serviceProvider);
            return this;
        }

        /// <summary>
        /// Get the root service provider for a given route (prefix) name.
        /// </summary>
        /// <param name="prefix">The route name (the route prefix name).</param>
        /// <returns>The root service provider for the route (prefix) name.</returns>
        public IServiceProvider GetODataServiceProvider(string prefix)
        {
            if (prefix != null && RouteComponents.ContainsKey(prefix))
            {
                return RouteComponents[prefix].ServiceProvider;
            }

            return null;
        }

        #endregion

        #region Global Query settings

        /// <summary>
        /// Enables all OData query features in one command.
        /// </summary>
        /// <param name="maxTopValue">The maximum value of $top that a client can request.</param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions EnableODataQuery(int? maxTopValue)
        {
            QuerySettings.EnableExpand = true;
            QuerySettings.EnableSelect = true;
            QuerySettings.EnableFilter = true;
            QuerySettings.EnableOrderBy = true;
            QuerySettings.EnableCount = true;
            QuerySettings.EnableSkipToken = true;
            SetMaxTop(maxTopValue);
            return this;
        }

        /// <summary>
        /// Enable $expand query options.
        /// </summary>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions Expand()
        {
            QuerySettings.EnableExpand = true;
            return this;
        }

        /// <summary>
        /// Enable $select query options.
        /// </summary>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions Select()
        {
            QuerySettings.EnableSelect = true;
            return this;
        }

        /// <summary>
        /// Enable $filter query options.
        /// </summary>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions Filter()
        {
            QuerySettings.EnableFilter = true;
            return this;
        }

        /// <summary>
        /// Enable $orderby query options.
        /// </summary>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions OrderBy()
        {
            QuerySettings.EnableOrderBy = true;
            return this;
        }

        /// <summary>
        /// Enable $count query options.
        /// </summary>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions Count()
        {
            QuerySettings.EnableCount = true;
            return this;
        }

        /// <summary>
        /// Enable $skiptop query option.
        /// </summary>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions SkipToken()
        {
            QuerySettings.EnableSkipToken = true;
            return this;
        }

        /// <summary>
        /// Sets the maximum value of $top that a client can request.
        /// </summary>
        /// <param name="maxTopValue">The maximum value of $top that a client can request.</param>
        /// <returns>The current <see cref="ODataOptions"/> instance to enable fluent configuration.</returns>
        public ODataOptions SetMaxTop(int? maxTopValue)
        {
            if (maxTopValue.HasValue && maxTopValue.Value < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxTopValue), maxTopValue, 0);
            }

            QuerySettings.MaxTop = maxTopValue;
            return this;
        }

        /// <summary>
        /// Gets or sets whether or not the OData system query options should be prefixed with '$'.
        /// </summary>
        public bool EnableNoDollarQueryOptions { get; set; } = true;

        /// <summary>
        /// Gets the query setting.
        /// </summary>
        public DefaultQuerySettings QuerySettings { get; } = new DefaultQuerySettings();

        #endregion

        /// <summary>
        /// Build the container.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="setupAction">The setup config.</param>
        /// <returns>The built service provider.</returns>
        private IServiceProvider BuildRouteContainer(IEdmModel model, Action<IContainerBuilder> setupAction)
        {
            Contract.Assert(model != null);

            IContainerBuilder odataContainerBuilder = null;
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

            // Inject the core odata services.
            odataContainerBuilder.AddDefaultODataServices();

            // Inject the default query setting from this options.
            odataContainerBuilder.AddService(ServiceLifetime.Singleton, sp => this.QuerySettings);

            // Inject the default Web API OData services.
            odataContainerBuilder.AddDefaultWebApiServices();

            // Set Uri resolver to by default enabling unqualified functions/actions and case insensitive match.
            odataContainerBuilder.AddService(ServiceLifetime.Singleton,
                typeof(ODataUriResolver),
                sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

            // Inject the Edm model.
            // From Current ODL implment, such injection only be used in reader and writer if the input
            // model is null.
            odataContainerBuilder.AddService(ServiceLifetime.Singleton, sp => model);

            // Inject the customized services.
            setupAction?.Invoke(odataContainerBuilder);

            return odataContainerBuilder.BuildContainer();
        }
    }
}
