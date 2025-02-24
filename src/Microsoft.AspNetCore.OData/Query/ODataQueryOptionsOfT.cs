//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionsOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    [ODataQueryParameterBinding]
    public class ODataQueryOptions<TEntity> : ODataQueryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="request">The incoming request message</param>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public ODataQueryOptions(ODataQueryContext context, HttpRequest request)
            : base(context, request)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.Argument("context", SRResources.ElementClrTypeNull, typeof(ODataQueryContext).Name);
            }

            if (context.ElementClrType != typeof(TEntity))
            {
                throw Error.Argument("context", SRResources.EntityTypeMismatch, context.ElementClrType.FullName, typeof(TEntity).FullName);
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfMatch header.
        /// </summary>
        public new ETag<TEntity> IfMatch
        {
            get
            {
                return base.IfMatch as ETag<TEntity>;
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfNoneMatch header.
        /// </summary>
        public new ETag<TEntity> IfNoneMatch
        {
            get
            {
                return base.IfNoneMatch as ETag<TEntity>;
            }
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag>.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal override ETag GetETag(EntityTagHeaderValue etagHeaderValue)
        {
            return Request.GetETag<TEntity>(etagHeaderValue);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query)
        {
            ValidateQuery(query);
            return base.ApplyTo(query);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            ValidateQuery(query);
            return base.ApplyTo(query, querySettings);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Bind the <see cref="HttpContext"/> and <see cref="ParameterInfo"/> to generate the <see cref="ODataQueryOptions{TEntity}"/>.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        /// <param name="parameter">The parameter info.</param>
        /// <returns>The built <see cref="ODataQueryOptions{TEntity}"/></returns>
        public static ValueTask<ODataQueryOptions<TEntity>> BindAsync(HttpContext context, ParameterInfo parameter)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (parameter == null)
            {
                throw Error.ArgumentNull(nameof(parameter));
            }

            Type entityClrType = typeof(TEntity);
            IEdmModel model = GetEdmModel(context);
            ODataQueryContext entitySetContext = new ODataQueryContext(model, entityClrType, context.ODataFeature().Path);
            var result = new ODataQueryOptions<TEntity>(entitySetContext, context.Request);

            return ValueTask.FromResult(result);
        }

        private static IEdmModel GetEdmModel(HttpContext context)
        {
            Endpoint endpoint = context.GetEndpoint();

            // 1. If customer calls "WithOData({prefix})", let's use it to fetch the model associated when calling 'AddOData(...)'
            ODataPrefixMetadata prefixMetadata = endpoint.Metadata.GetMetadata<ODataPrefixMetadata>();
            if (prefixMetadata != null)
            {
                ODataOptions options = context.RequestServices.GetService<IOptions<ODataOptions>>()?.Value;
                if (options != null)
                {
                    if (options.RouteComponents.TryGetValue(prefixMetadata.Prefix, out var routeComponents))
                    {
                        var odataFeature = context.ODataFeature();
                        odataFeature.RoutePrefix = prefixMetadata.Prefix;
                        odataFeature.Model = routeComponents.EdmModel;
                        odataFeature.Services = routeComponents.ServiceProvider;
                        return routeComponents.EdmModel;
                    }
                }

                throw new InvalidOperationException($"The '{prefixMetadata.Prefix}' when calling WithOData() is not registered in AddOData().");
            }

            // 2. Let's retrieve the model from the cache
            IODataEndpointModelMapper endpointModelMapper = context.RequestServices.GetService<IODataEndpointModelMapper>();
            IEdmModel model;
            if (endpointModelMapper != null && endpointModelMapper.Maps.TryGetValue(endpoint, out model))
            {
                return model;
            }

            // 3. Let's build the model on the fly
            Type entityClrType = typeof(TEntity);
            IAssemblyResolver resolver = context.RequestServices.GetService<IAssemblyResolver>();
            ODataConventionModelBuilder builder = resolver != null ?
                new ODataConventionModelBuilder(resolver, true) :
                new ODataConventionModelBuilder();

            EntityTypeConfiguration entityTypeConfiguration = builder.AddEntityType(entityClrType);
            builder.AddEntitySet(entityClrType.Name, entityTypeConfiguration);

            var modelConfigs = endpoint.Metadata.OfType<IODataModelConfiguration>();
            foreach (var config in modelConfigs)
            {
                config.Apply(context, builder);
            }

            model = builder.GetEdmModel();

            // Add the model into the cache
            if (endpointModelMapper != null)
            {
                endpointModelMapper.Maps[endpoint] = model;
            }

            return model;
        }
#endif

        private static void ValidateQuery(IQueryable query)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (!TypeHelper.IsTypeAssignableFrom(typeof(TEntity), query.ElementType))
            {
                throw Error.Argument("query", SRResources.CannotApplyODataQueryOptionsOfT, typeof(ODataQueryOptions).Name, typeof(TEntity).FullName, typeof(IQueryable).Name, query.ElementType.FullName);
            }
        }
    }
}
