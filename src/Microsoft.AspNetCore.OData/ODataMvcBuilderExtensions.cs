// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides extension methods to add OData services based on <see cref="IMvcBuilder"/>.
    /// </summary>
    public static class ODataMvcBuilderExtensions
    {
        /// <summary>
        /// Adds essential OData services to the specified <see cref="IMvcBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to add services to.</param>
        /// <returns>A <see cref="IMvcBuilder"/> that can be used to further configure the OData services.</returns>
        public static IMvcBuilder AddOData(this IMvcBuilder builder)
        {
            return builder.AddOData(opt => { });
        }

        /// <summary>
        /// Adds essential OData services to the specified <see cref="IMvcBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to add services to.</param>
        /// <param name="setupAction">The OData options to configure the services with,
        /// including access to a service provider which you can resolve services from.</param>
        /// <returns>A <see cref="IMvcBuilder"/> that can be used to further configure the OData services.</returns>
        public static IMvcBuilder AddOData(this IMvcBuilder builder, Action<ODataOptions> setupAction)
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
        /// Adds essential OData services to the specified <see cref="IMvcBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder" /> to add services to.</param>
        /// <param name="setupAction">The OData options to configure the services with,
        /// including access to a service provider which you can resolve services from.</param>
        /// <returns>A <see cref="IMvcBuilder"/> that can be used to further configure the OData services.</returns>
        public static IMvcBuilder AddOData(this IMvcBuilder builder, Action<ODataOptions, IServiceProvider> setupAction)
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
