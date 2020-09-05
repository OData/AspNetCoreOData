// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.TryAddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();
            services.TryAddSingleton<IODataTypeMappingProvider, ODataTypeMappingProvider>();

            services.TryAddSingleton(sp =>
            {
                ODataOptions options = sp.GetRequiredService<IOptions<ODataOptions>>().Value;
                return options.BuildDefaultQuerySettings();
            });

            services.Configure(setupAction);

            IODataBuilder builder = new DefaultODataBuilder(services);
            builder.AddCoreOData();

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
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IODataControllerActionConvention, T>());

            return builder;
        }

        private static void AddCoreOData(this IODataBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            // Formatter
            builder.AddODataFormatter();

            // Routing related services
            builder.AddODataRouting();

            // Query
            builder.AddODataQuery();
        }
    }
}
