// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.Extensions.DependencyInjection;

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

        private static void AddCoreOData(this IServiceCollection services)
        {
            services.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

            services.AddSingleton<IODataTypeMappingProvider, ODataTypeMappingProvider>();

            // services.AddSingleton(typeof(ODataClrTypeCache));
        }
    }
}
