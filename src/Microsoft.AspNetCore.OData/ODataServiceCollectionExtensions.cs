// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods to add OData services.
    /// </summary>
    public static class ODataServiceCollectionExtensions
    {
        /// <summary>
        /// Adds essential OData services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>An <see cref="IODataBuilder"/> that can be used to further configure the OData services.</returns>
        public static IODataBuilder AddOData(this IServiceCollection services)
        {
            return services.AddOData(opt => { });
        }

        /// <summary>
        /// Adds services required for OData requests.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setupAction">The OData options to configure the services with.</param>
        /// <returns>The <see cref="IODataBuilder"/> so that additional calls can be chained.</returns>
        public static IODataBuilder AddOData(this IServiceCollection services, Action<ODataOptions> setupAction)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            if (setupAction == null)
            {
                throw Error.ArgumentNull(nameof(setupAction));
            }

            services.TryAddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();
            services.TryAddSingleton<IODataTypeMappingProvider, ODataTypeMappingProvider>();

            // Configure MvcCore to use formatters. The OData formatters do go into the global service
            // provider and get picked up by the AspNetCore MVC framework. However, they ignore non-OData
            // requests so they won't be used for non-OData formatting.
            services.AddControllers(options =>
            {
                // Add OData input formatters at index 0, which overrides the built-in json and xml formatters.
                // Add in reverse order at index 0 to preserve order from the factory in the final list.
                foreach (ODataInputFormatter inputFormatter in ODataInputFormatterFactory.Create().Reverse())
                {
                    options.InputFormatters.Insert(0, inputFormatter);
                }

                // Add OData output formatters at index 0, which overrides the built-in json and xml formatters.
                // Add in reverse order at index 0 to preserve order from the factory in the final list.
                foreach (ODataOutputFormatter outputFormatter in ODataOutputFormatterFactory.Create().Reverse())
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }

                // Add the value provider.
                // options.ValueProviderFactories.Insert(0, new ODataValueProviderFactory());
            });

            services.Configure(setupAction);

            services.AddODataRouting();

            IODataBuilder builder = new DefaultODataBuilder(services);
            return builder;
        }

        /// <summary>
        /// Adds OData routing convention.
        /// </summary>
        /// <typeparam name="T">The routing convention type.</typeparam>
        /// <param name="builder">The OData service builder.</param>
        /// <returns>The <see cref="IODataBuilder"/> so that additional calls can be chained.</returns>
        public static IODataBuilder AddConvention<T>(this IODataBuilder builder)
            where T : class, IODataControllerActionConvention
        {
            if (builder == null)
            {
                throw Error.ArgumentNull(nameof(builder));
            }

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, T>());

            return builder;
        }

        /// <summary>
        /// Adds the core odata routing services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The IODataBuilder itself.</returns>
        private static void AddODataRouting(this IServiceCollection services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelProvider>());

            // for debug only
            //services.TryAddEnumerable(
            //    ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelDebugProvider>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ODataRoutingMatcherPolicy>());

            // OData Routing conventions
            // ~/$metadata
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, MetadataRoutingConvention>());

            // ~/EntitySet
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, EntitySetRoutingConvention>());

            // ~/EntitySet/{key}
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, EntityRoutingConvention>());

            // ~/Singleton
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, SingletonRoutingConvention>());

            // ~/EntitySet|Singleton/.../NS.Function
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, FunctionRoutingConvention>());

            // ~/EntitySet|Singleton/.../NS.Action
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, ActionRoutingConvention>());

            // ~/OperationImport
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, OperationImportRoutingConvention>());

            // ~/EntitySet{key}|Singleton/{Property}
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, PropertyRoutingConvention>());

            // ~/EntitySet{key}|Singleton/{NavigationProperty}
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, NavigationRoutingConvention>());

            // ~/EntitySet{key}|Singleton/{NavigationProperty}/$ref
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, RefRoutingConvention>());

            // Attribute routing
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, AttributeRoutingConvention>());

            services.TryAddSingleton<IODataTemplateTranslator, DefaultODataTemplateTranslator>();
            services.TryAddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();
        }
    }
}
