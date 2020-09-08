// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Adds the OData query related services into the builder.
    /// </summary>
    internal static class ODataQueryBuilderServiceExtensions
    {
        /// <summary>
        /// Adds the OData query related services into the odata builder.
        /// </summary>
        /// <param name="builder">The OData builder.</param>
        /// <returns>The IODataBuilder itself.</returns>
        public static IODataBuilder AddODataQuery(this IODataBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            AddODataQueryServices(builder.Services);

            return builder;
        }

        static void AddODataQueryServices(IServiceCollection services)
        {
            /*
            // need this?
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
            */
            // services.AddTransient<FilterBinder>();
        }
    }
}
