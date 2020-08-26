// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;
using System;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// 
    /// </summary>
    public static class ODataQueryBuilderServiceExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IODataBuilder AddODataQuery(this IODataBuilder builder)
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
        public static IODataBuilder AddODataQuery(this IODataBuilder builder, Action<ODataQueryableOptions> setupAction)
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
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddODataQuery(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            return services.AddODataQuery(options => { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="setupAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddODataQuery(this IServiceCollection services, Action<DefaultQuerySettings> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            AddODataQueryServices(services, setupAction);

            // services.Configure(setupAction);
            return services;
        }

        static void AddODataQueryServices(IServiceCollection services, Action<DefaultQuerySettings> setupAction)
        {
            services.AddSingleton<ODataUriResolver>(
                sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

            //services.AddSingleton(sp =>
            //{
            //    DefaultQuerySettings settings = new DefaultQuerySettings();
            //    setupAction(settings);
            //    return settings;
            //});

            // QueryValidators.
            services.AddSingleton<CountQueryValidator>();
            services.AddSingleton<FilterQueryValidator>();
            services.AddSingleton<ODataQueryValidator>();
            services.AddSingleton<OrderByQueryValidator>();
            services.AddSingleton<SelectExpandQueryValidator>();
            services.AddSingleton<SkipQueryValidator>();
            services.AddSingleton<SkipTokenQueryValidator>();
            services.AddSingleton<TopQueryValidator>();

            services.AddScoped<ODataQuerySettings>();

            services.AddSingleton<SkipTokenHandler, DefaultSkipTokenHandler>();
            // services.AddTransient<FilterBinder>();
        }
    }
}
