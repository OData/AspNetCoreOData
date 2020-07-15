// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// 
    /// </summary>
    public static class ODataRoutingServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IMvcBuilder AddODataRouting(this IMvcBuilder builder, Action<ODataRoutingOptions> setupAction)
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
        /// <param name="model"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddODataRouting(this IServiceCollection services, Action<ODataRoutingOptions> setupAction)
        {
            AddODataRoutingServices(services);
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

        static void AddODataRoutingServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ODataRoutingOptions>, ODataRoutingOptionsSetup>());

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

            // ~/EntitySet|Singleton/.../NS.Operation
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, OperationRoutingConvention>());

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
        }
    }
}
