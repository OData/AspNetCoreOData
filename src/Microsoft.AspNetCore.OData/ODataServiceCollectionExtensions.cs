// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods to add odata services.
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
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddCoreOData();
            // services.AddSingleton<ODataOptions>();

            return new DefaultODataBuilder(services);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IODataBuilder AddOData(this IServiceCollection services, Action<ODataOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddCoreOData();
            // services.AddSingleton<ODataOptions>();

            services.Configure(setupAction);

            return new DefaultODataBuilder(services);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddOData(this IServiceCollection services, Action<ODataOptionsBuilder> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddCoreOData();
            // services.AddSingleton<ODataOptions>();

            services.Configure(setupAction);

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddConvention<T>(this IServiceCollection services)
            where T : class, IODataControllerActionConvention
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, T>());

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IODataBuilder AddODataRouting(this IODataBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            //AddODataRoutingServices(builder.Services);
            //builder.Services.Configure(setupAction);
            return builder;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IODataBuilder AddODataRouting(this IODataBuilder builder, Action<ODataRoutingOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddODataRoutingServices(builder.Services);
            builder.Services.Configure(setupAction);
            return builder;
        }


        private static void AddCoreOData(this IServiceCollection services)
        {
            services.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

            services.AddSingleton<IODataTypeMappingProvider, ODataTypeMappingProvider>();

            // services.AddSingleton(typeof(ODataClrTypeCache));
        }

        static void AddODataRoutingServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RouteOptions>, ODataRoutingConstraintConfiguration>());

            //services.TryAddEnumerable(
            //    ServiceDescriptor.Transient<IConfigureOptions<ODataRoutingOptions>, ODataRoutingOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelProvider>());

            // for debug only
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelDebugProvider>());

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

            services.AddSingleton<IODataTemplateTranslator, DefaultODataTemplateTranslator>();
            services.AddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();
        }
    }
}
