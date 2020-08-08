// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.OData.Query
{
    public class ODataQueryableOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether property can apply $filter.
        /// </summary>
        public bool EnableFilter { get; set; }

        public ODataQueryableOptions Filter(bool isFilterable)
        {
            EnableFilter = isFilterable;
            return this;
        }

        public ODataQueryableOptions Expand(bool isExpandable)
        {
            return this;
        }
    }

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
        public static IServiceCollection AddODataQuery(this IServiceCollection services/*, Action<ODataRoutingOptions> setupAction*/)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            AddODataQueryServices(services);
            // services.Configure(setupAction);
            return services;
        }

        static void AddODataQueryServices(IServiceCollection services)
        {
            services.AddSingleton<DefaultQuerySettings>();

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
        }
    }
}
