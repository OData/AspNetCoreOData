// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData
{

    /// <summary>
    /// Provides extension methods to add OData services to <see cref="IServiceCollection">IServiceCollections</see>.
    /// </summary>
    internal static class IServiceCollectionExtensions
    {

        #region Public Methods

        /// <summary>
        /// Adds the core OData services required for OData requests.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to inject the services into.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance to enable fluent configuration.</returns>
        public static IServiceCollection AddODataCore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            //
            // Options
            //
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ODataOptions>, ODataOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, ODataMvcOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<JsonOptions>, ODataJsonOptionsSetup>());

            //
            // Parser & Resolver & Provider
            //
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IODataQueryRequestParser, DefaultODataQueryRequestParser>());

            services.TryAddSingleton<IAssemblyResolver, DefaultAssemblyResolver>();

            //
            // Routing
            //
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApplicationModelProvider, ODataRoutingApplicationModelProvider>());

            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ODataRoutingMatcherPolicy>());

            services.TryAddSingleton<IODataTemplateTranslator, DefaultODataTemplateTranslator>();

            services.TryAddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to inject the services into.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance to enable fluent configuration.</returns>
        public static IServiceCollection AddODataDefaultServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            //RWM: This is a hack to get around the Microsoft.OData.Core IContainerBuilder limitation.
            var builder = new DefaultContainerBuilder();
            builder.AddDefaultODataServices();
            services.AddRange(builder.services.Where(c => c.Lifetime == Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton).ToList());
            return services;
        }

        /// <summary>
        /// Injects the default Web API OData services into the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to inject the services into.</param>
        /// <returns>The current <see cref="IServiceCollection"/> instance to enable fluent configuration.</returns>
        /// <remarks>This should be registered on the ServiceProvider manipulated by the <see cref="ODataMvcBuilderExtensions"/>.</remarks>
        public static IServiceCollection AddODataWebApiServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            services.AddSingleton<IETagHandler, DefaultODataETagHandler>();

            //builder.AddService<IODataPathHandler, DefaultODataPathHandler>();

            // QueryValidators
            services.AddSingleton<CountQueryValidator>();
            services.AddSingleton<ODataQueryValidator>();
            services.AddSingleton<OrderByQueryValidator>();
            services.AddSingleton<SelectExpandQueryValidator>();
            services.AddSingleton<SkipQueryValidator>();
            services.AddSingleton<SkipTokenQueryValidator>();
            services.AddSingleton<TopQueryValidator>();

            services.AddSingleton<SkipTokenHandler, DefaultSkipTokenHandler>();

            // SerializerProvider and DeserializerProvider
            services.AddSingleton<IODataSerializerProvider, ODataSerializerProvider>();
            services.AddSingleton<IODataDeserializerProvider, ODataDeserializerProvider>();

            // Deserializers
            services.AddSingleton<ODataResourceDeserializer>();
            services.AddSingleton<ODataEnumDeserializer>();
            services.AddSingleton<ODataPrimitiveDeserializer>();
            services.AddSingleton<ODataResourceSetDeserializer>();
            services.AddSingleton<ODataCollectionDeserializer>();
            services.AddSingleton<ODataEntityReferenceLinkDeserializer>();
            services.AddSingleton<ODataActionPayloadDeserializer>();
            services.AddSingleton<ODataDeltaResourceSetDeserializer>();

            // Serializers
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

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to inject the services into.</param>
        /// <param name="odataVersion"></param>
        /// <returns>The current <see cref="IServiceCollection"/> instance to enable fluent configuration.</returns>
        /// <remarks>This should be registered on the ServiceProvider manipulated by the <see cref="ODataMvcBuilderExtensions"/>.</remarks>
        public static IServiceCollection AddScopedODataServices(this IServiceCollection services, ODataVersion odataVersion = ODataVersion.V4)
        {
            if (services == null)
            {
                throw Error.ArgumentNull(nameof(services));
            }

            // ReaderSettings and WriterSettings are registered as prototype services.
            // There will be a copy (if it is accessed) of each prototype for each request.
            services.TryAddScoped(sp => new ODataMessageReaderSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = long.MaxValue },
            });

            services.TryAddScoped(sp => new ODataMessageWriterSettings
            {
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = long.MaxValue },
            });

            services.AddScoped<ODataMessageInfo>();
            services.AddScoped<ODataUriParserSettings>();
            services.AddScoped<UriPathParser>();
            services.AddScoped(sp => new ODataSimplifiedOptions(odataVersion));

            // FilterQueryValidator should be scoped, otherwise some instance field (for example:_currentNodeCount) should be a problem.
            services.AddScoped<FilterQueryValidator>();

            // Binders
            services.TryAddScoped<ODataQuerySettings>();
            services.AddTransient<FilterBinder>();

            // HttpRequestScope
            services.AddScoped<HttpRequestScope>();
            services.AddScoped(sp => sp.GetRequiredService<HttpRequestScope>().HttpRequest);
            return services;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instance">The <see cref="IServiceCollection"/> instance to inject the services into.</param>
        /// <param name="collectionToAdd"></param>
        /// <returns></returns>
        private static IServiceCollection AddRange(this IServiceCollection instance, List<ServiceDescriptor> collectionToAdd)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            foreach (var service in collectionToAdd)
            {
                instance.Add(service);
            }

            return instance;
        }

        #endregion

    }

}
