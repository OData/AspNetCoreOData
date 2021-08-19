//-----------------------------------------------------------------------------
// <copyright file="ODataMvcCoreBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods to add OData services based on <see cref="IMvcCoreBuilder"/>.
    /// </summary>
    public static class ODataMvcCoreBuilderExtensions
    {
        /// <summary>
        /// Adds essential OData services to the specified <see cref="IMvcCoreBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to add services to.</param>
        /// <returns>A <see cref="IMvcCoreBuilder"/> that can be used to further configure the OData services.</returns>
        public static IMvcCoreBuilder AddOData(this IMvcCoreBuilder builder)
        {
            return builder.AddOData(opt => { });
        }

        /// <summary>
        /// Adds essential OData services to the specified <see cref="IMvcCoreBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to add services to.</param>
        /// <param name="setupAction">The OData options to configure the services with,
        /// including access to a service provider which you can resolve services from.</param>
        /// <returns>A <see cref="IMvcCoreBuilder"/> that can be used to further configure the OData services.</returns>
        public static IMvcCoreBuilder AddOData(this IMvcCoreBuilder builder, Action<ODataOptions> setupAction)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull(nameof(builder));
            }

            if (setupAction == null)
            {
                throw Error.ArgumentNull(nameof(setupAction));
            }

            builder.Services.AddODataCore();
            builder.Services.Configure(setupAction);
            return builder;
        }

        /// <summary>
        /// Adds essential OData services to the specified <see cref="IMvcCoreBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcCoreBuilder" /> to add services to.</param>
        /// <param name="setupAction">The OData options to configure the services with,
        /// including access to a service provider which you can resolve services from.</param>
        /// <returns>A <see cref="IMvcCoreBuilder"/> that can be used to further configure the OData services.</returns>
        public static IMvcCoreBuilder AddOData(this IMvcCoreBuilder builder, Action<ODataOptions, IServiceProvider> setupAction)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull(nameof(builder));
            }

            if (setupAction == null)
            {
                throw Error.ArgumentNull(nameof(setupAction));
            }

            builder.Services.AddODataCore();
            builder.Services.AddOptions<ODataOptions>().Configure(setupAction);

            return builder;
        }
    }
}
