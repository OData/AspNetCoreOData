// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatting.Deserialization;
using Microsoft.AspNetCore.OData.Formatting.Serialization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatting
{
    /// <summary>
    /// 
    /// </summary>
    public static class ODataFormattingServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddODataFormatter(this IServiceCollection services/*, Action<ODataRoutingOptions> setupAction*/)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            AddODataFormatterServices(services);
           // services.Configure(setupAction);
            return services;
        }

        static void AddODataFormatterServices(IServiceCollection services)
        {
            // Configure MvcCore to use formatters. The OData formatters do go into the global service
            // provider and get picked up by the AspNetCore MVC framework. However, they ignore non-OData
            // requests so they won't be used for non-OData formatting.
            services.AddMvcCore(options =>
            {
                // Add OData input formatters at index 0, which overrides the built-in json and xml formatters.
                // Add in reverse order at index 0 to preserve order from the factory in the final list.
                foreach (ODataInputFormatter inputFormatter in ODataInputFormatterFactory.Create().Reverse())
                {
                    options.InputFormatters.Insert(0, inputFormatter);
                }

                // Add OData output formatters at index 0, which overrides the built-in json and xml formatters.
                // Add in reverse order at index 0 to preserve order from the factory in the final list.
                foreach (ODataOutputFormatter outputFormatter in ODataOutputFormatterFactory.Create().Reverse())
                {
                    options.OutputFormatters.Insert(0, outputFormatter);
                }

                // Add the value provider.
                // options.ValueProviderFactories.Insert(0, new ODataValueProviderFactory());
            });

            // SerializerProvider and DeserializerProvider.
            services.AddSingleton<ODataSerializerProvider, DefaultODataSerializerProvider>();
            services.AddSingleton<ODataDeserializerProvider, DefaultODataDeserializerProvider>();

            // Serializers.
            services.AddSingleton<ODataEnumSerializer>();
         //   services.AddSingleton<ODataPrimitiveSerializer>();
         //   services.AddSingleton<ODataDeltaFeedSerializer>();
          //  services.AddSingleton<ODataResourceSetSerializer>();
            services.AddSingleton<ODataCollectionSerializer>();
        //   services.AddSingleton<ODataResourceSerializer>();
            services.AddSingleton<ODataServiceDocumentSerializer>();
            services.AddSingleton<ODataEntityReferenceLinkSerializer>();
            services.AddSingleton<ODataEntityReferenceLinksSerializer>();
            services.AddSingleton<ODataErrorSerializer>();
            services.AddSingleton<ODataMetadataSerializer>();
            services.AddSingleton<ODataRawValueSerializer>();

            services.AddSingleton(new ODataMessageReaderSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            services.AddSingleton(new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            services.AddSingleton<ODataMediaTypeResolver>();
            services.AddSingleton<ODataMessageInfo>();
        }
    }
}
