//-----------------------------------------------------------------------------
// <copyright file="ContainerBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    internal static class ContainerBuilderExtensions
    {
        /// <summary>
        /// Injects the default Web API OData services.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <returns>The calling itself.</returns>
        public static IContainerBuilder AddDefaultWebApiServices(this IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull(nameof(builder));
            }

            builder.AddService<IETagHandler, DefaultODataETagHandler>(ServiceLifetime.Singleton);

            //builder.AddService<IODataPathHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);

            // ReaderSettings and WriterSettings are registered as prototype services.
            // There will be a copy (if it is accessed) of each prototype for each request.
            builder.AddServicePrototype(new ODataMessageReaderSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },

                // WebAPI should read untyped values as structural values by setting ReadUntypedAsString=false.
                // In ODL 8.x, ReadUntypedAsString option will be deleted.
                ReadUntypedAsString = false
            });

            builder.AddServicePrototype(new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
            });

            // QueryValidators.
            builder.AddService<CountQueryValidator>(ServiceLifetime.Singleton);

            // FilterQueryValidator should be scoped, otherwise some instance field (for example:_currentNodeCount) should be a problem.
            builder.AddService<FilterQueryValidator>(ServiceLifetime.Scoped);

            builder.AddService<ODataQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<OrderByQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SelectExpandQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SkipQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<SkipTokenQueryValidator>(ServiceLifetime.Singleton);
            builder.AddService<TopQueryValidator>(ServiceLifetime.Singleton);

            builder.AddService<SkipTokenHandler, DefaultSkipTokenHandler>(ServiceLifetime.Singleton);

            // SerializerProvider and DeserializerProvider.
            builder.AddService<IODataSerializerProvider, ODataSerializerProvider>(ServiceLifetime.Singleton);
            builder.AddService<IODataDeserializerProvider, ODataDeserializerProvider>(ServiceLifetime.Singleton);

            // Deserializers.
            builder.AddService<ODataResourceDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEnumDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataActionPayloadDeserializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeltaResourceSetDeserializer>(ServiceLifetime.Singleton);

            // Serializers.
            builder.AddService<ODataEnumSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataPrimitiveSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataDeltaResourceSetSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSetSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataCollectionSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataResourceSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataServiceDocumentSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinkSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataEntityReferenceLinksSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataErrorSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataMetadataSerializer>(ServiceLifetime.Singleton);
            builder.AddService<ODataRawValueSerializer>(ServiceLifetime.Singleton);

            // Binders.
            builder.AddService<ODataQuerySettings>(ServiceLifetime.Scoped);
            builder.AddService<IFilterBinder, FilterBinder>(ServiceLifetime.Scoped);
            builder.AddService<IOrderByBinder, OrderByBinder>(ServiceLifetime.Scoped);
            builder.AddService<ISelectExpandBinder, SelectExpandBinder>(ServiceLifetime.Singleton);

            // HttpRequestScope.
            builder.AddService<HttpRequestScope>(ServiceLifetime.Scoped);
            builder.AddService(ServiceLifetime.Scoped, sp => sp.GetRequiredService<HttpRequestScope>().HttpRequest);
            return builder;
        }
    }
}
