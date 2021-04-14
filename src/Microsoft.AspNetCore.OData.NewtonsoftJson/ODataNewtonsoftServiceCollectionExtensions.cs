// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.NewtonsoftJson
{
    /// <summary>
    /// Extension methods for adding OData Json converter to Newtonsoft.Json to <see cref="IODataBuilder"/>.
    /// </summary>
    public static class ODataNewtonsoftServiceCollectionExtensions
    {
        /// <summary>
        /// Configures Newtonsoft.Json using OData Json converter.
        /// </summary>
        /// <param name="builder">The OData builder.</param>
        /// <returns>The <see cref="IODataBuilder"/>.</returns>
        public static IODataBuilder AddNewtonsoftJson(this IODataBuilder builder)
        {
            return builder.AddNewtonsoftJson(opt => { });
        }

        /// <summary>
        /// Configures Newtonsoft.Json using OData Json converter.
        /// </summary>
        /// <param name="builder">The OData builder.</param>
        /// <param name="setupAction">Callback to configure <see cref="MvcNewtonsoftJsonOptions"/>.</param>
        /// <returns>The <see cref="IODataBuilder"/>.</returns>
        public static IODataBuilder AddNewtonsoftJson(this IODataBuilder builder, Action<MvcNewtonsoftJsonOptions> setupAction)
        {
            return builder.AddNewtonsoftJson(setupAction, null);
        }

        /// <summary>
        /// Configures Newtonsoft.Json using OData Json converter.
        /// </summary>
        /// <param name="builder">The OData builder.</param>
        /// <param name="setupAction">Callback to configure <see cref="MvcNewtonsoftJsonOptions"/>.</param>
        /// <param name="mapperProvider">The property mapper provider.</param>
        /// <returns>The <see cref="IODataBuilder"/>.</returns>
        public static IODataBuilder AddNewtonsoftJson(this IODataBuilder builder, Action<MvcNewtonsoftJsonOptions> setupAction,
            Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction is null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            Action<MvcNewtonsoftJsonOptions> odataSetupAction = opt =>
            {
                setupAction(opt);

                if (mapperProvider is null)
                {
                    opt.SerializerSettings.Converters.Add(new JSelectExpandWrapperConverter());
                }
                else
                {
                    opt.SerializerSettings.Converters.Add(new JSelectExpandWrapperConverter(mapperProvider));
                }

                opt.SerializerSettings.Converters.Add(new JDynamicTypeWrapperConverter());
                opt.SerializerSettings.Converters.Add(new JPageResultValueConverter());
                opt.SerializerSettings.Converters.Add(new JSingleResultValueConverter());
            };

            builder.Services.AddControllers().AddNewtonsoftJson(odataSetupAction);
            return builder;
        }
    }
}
