// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    /// <summary>
    /// A factory for creating <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public static class ODataFormatterHelpers
    {
        private static IServiceProvider _serviceProvider = BuildServiceProvider();

        /// <summary>
        /// Gets an <see cref="ODataSerializerProvider"/>.
        /// </summary>
        /// <returns>An ODataSerializerProvider.</returns>
        public static ODataSerializerProvider GetSerializerProvider()
        {
            return _serviceProvider.GetRequiredService<ODataSerializerProvider>();
        }

        /// <summary>
        /// Gets an <see cref="ODataDeserializerProvider"/>.
        /// </summary>
        /// <returns>An ODataDeserializerProvider.</returns>
        public static ODataDeserializerProvider GetDeserializerProvider()
        {
            return _serviceProvider.GetRequiredService<ODataDeserializerProvider>();
        }

        public static ODataMessageWriter GetMockODataMessageWriter()
        {
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            return new ODataMessageWriter(requestMessage);
        }

        public static ODataMessageReader GetMockODataMessageReader()
        {
            MockODataRequestMessage requestMessage = new MockODataRequestMessage();
            return new ODataMessageReader(requestMessage);
        }

        private static IServiceProvider BuildServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

            services.AddSingleton<IODataTypeMappingProvider, ODataTypeMappingProvider>();

            // Deserializers.
            services.AddSingleton<ODataDeserializerProvider, DefaultODataDeserializerProvider>();

            services.AddSingleton<ODataResourceDeserializer>();
            services.AddSingleton<ODataEnumDeserializer>();
            services.AddSingleton<ODataPrimitiveDeserializer>();
            services.AddSingleton<ODataResourceSetDeserializer>();
            services.AddSingleton<ODataCollectionDeserializer>();
            services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
            services.AddSingleton<ODataActionPayloadDeserializer>();

            // Serializers.
            services.AddSingleton<ODataSerializerProvider, DefaultODataSerializerProvider>();

            services.AddSingleton<ODataEnumSerializer>();
            services.AddSingleton<ODataPrimitiveSerializer>();
            //   services.AddSingleton<ODataDeltaFeedSerializer>();
            services.AddSingleton<ODataResourceSetSerializer>();
            services.AddSingleton<ODataCollectionSerializer>();
            services.AddSingleton<ODataResourceSerializer>();
            services.AddSingleton<ODataServiceDocumentSerializer>();
            services.AddSingleton<ODataEntityReferenceLinkSerializer>();
            services.AddSingleton<ODataEntityReferenceLinksSerializer>();
            services.AddSingleton<ODataErrorSerializer>();
            services.AddSingleton<ODataMetadataSerializer>();
            services.AddSingleton<ODataRawValueSerializer>();

            return services.BuildServiceProvider();
        }
    }
}