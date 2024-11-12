//-----------------------------------------------------------------------------
// <copyright file="ODataServiceCollectionExtensions.cs" company=".NET Foundation">
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

namespace Microsoft.AspNetCore.OData.Abstracts;

internal static class ODataServiceCollectionExtensions
{
    /// <summary>
    /// Injects the default Web API OData services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The calling itself.</returns>
    public static IServiceCollection AddDefaultWebApiServices(this IServiceCollection services)
    {
        if (services == null)
        {
            throw Error.ArgumentNull(nameof(services));
        }

        services.AddSingleton<IETagHandler, DefaultODataETagHandler>();

        //builder.AddService<IODataPathHandler, DefaultODataPathHandler>(ServiceLifetime.Singleton);

        // ReaderSettings and WriterSettings are registered as prototype services.
        // There will be a copy (if it is accessed) of each prototype for each request.
#pragma warning disable CS0618 // ReadUntypedAsString is obsolete in ODL 8.
        services.AddScoped(sp => new ODataMessageReaderSettings
        {
            EnableMessageStreamDisposal = false,
            MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },

            // WebAPI should read untyped values as structural values by setting ReadUntypedAsString=false.
            // In ODL 8.x, ReadUntypedAsString option will be deleted.
            ReadUntypedAsString = false,

            // Enable read property name case-insensitive from payload.
            EnablePropertyNameCaseInsensitive = true,
            EnableReadingODataAnnotationWithoutPrefix = true
        });
#pragma warning restore CS0618 // Type or member is obsolete

        services.AddScoped(sp => new ODataMessageWriterSettings
        {
            EnableMessageStreamDisposal = false,
            MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
        });

        // QueryValidators.
        services.AddSingleton<ICountQueryValidator, CountQueryValidator>();

        services.AddSingleton<IFilterQueryValidator, FilterQueryValidator>();
        services.AddSingleton<IODataQueryValidator, ODataQueryValidator>();
        services.AddSingleton<IOrderByQueryValidator, OrderByQueryValidator>();
        services.AddSingleton<ISelectExpandQueryValidator, SelectExpandQueryValidator>();
        services.AddSingleton<ISkipQueryValidator, SkipQueryValidator>();
        services.AddSingleton<ISkipTokenQueryValidator, SkipTokenQueryValidator>();
        services.AddSingleton<ITopQueryValidator, TopQueryValidator>();
        services.AddSingleton<IComputeQueryValidator, ComputeQueryValidator>();

        services.AddSingleton<SkipTokenHandler, DefaultSkipTokenHandler>();

        // SerializerProvider and DeserializerProvider.
        services.AddSingleton<IODataSerializerProvider, ODataSerializerProvider>();
        services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

        // Deserializers.
        services.AddSingleton<ODataResourceDeserializer>();
        services.AddSingleton<ODataEnumDeserializer>();
        services.AddSingleton<ODataPrimitiveDeserializer>();
        services.AddSingleton<ODataResourceSetDeserializer>();
        services.AddSingleton<ODataCollectionDeserializer>();
        services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
        services.AddSingleton<ODataActionPayloadDeserializer>();
        services.AddSingleton<ODataDeltaResourceSetDeserializer>();

        // Serializers.
        services.AddSingleton<ODataEnumSerializer>();
        services.AddSingleton<ODataPrimitiveSerializer>();
        services.AddSingleton<ODataDeltaResourceSetSerializer>();
        services.AddSingleton<ODataResourceSetSerializer>();
        services.AddSingleton<ODataCollectionSerializer>();
        services.AddSingleton<ODataResourceSerializer>();
        services.AddSingleton<ODataServiceDocumentSerializer>();
        services.AddSingleton<ODataEntityReferenceLinkSerializer>();
        services.AddSingleton<ODataEntityReferenceLinksSerializer>();
        services.AddSingleton<ODataErrorSerializer>();
        services.AddSingleton<ODataMetadataSerializer>();
        services.AddSingleton<ODataRawValueSerializer>();

        // Binders.
        services.AddScoped<ODataQuerySettings>();

        services.AddSingleton<IFilterBinder, FilterBinder>();
        services.AddSingleton<IOrderByBinder, OrderByBinder>();
        services.AddSingleton<ISelectExpandBinder, SelectExpandBinder>();

        // HttpRequestScope.
        services.AddScoped<HttpRequestScope>();
        services.AddScoped(sp => sp.GetRequiredService<HttpRequestScope>().HttpRequest);
        return services;
    }
}
