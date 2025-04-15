//-----------------------------------------------------------------------------
// <copyright file="ODataQueryOptionsOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This defines a composite OData query options that can be used to perform query composition.
/// Currently this only supports $filter, $orderby, $top, $skip.
/// </summary>
[ODataQueryParameterBinding]
public class ODataQueryOptions<TEntity> : ODataQueryOptions, IEndpointParameterMetadataProvider
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

    /// <summary>
    /// Binds the <see cref="HttpContext"/> and <see cref="ParameterInfo"/> to generate the <see cref="ODataQueryOptions{TEntity}"/>.
    /// </summary>
    /// <param name="context">The HttpContext.</param>
    /// <param name="parameter">The parameter info.</param>
    /// <returns>The built <see cref="ODataQueryOptions{TEntity}"/></returns>
    public static async ValueTask<ODataQueryOptions<TEntity>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        Type entityClrType = typeof(TEntity);
        IEdmModel model = context.GetOrCreateEdmModel(entityClrType, parameter);
        ODataPath path = context.GetOrCreateODataPath(entityClrType);
        context.ODataFeature().Services = context.GetOrCreateServiceProvider();

        ODataQueryContext entitySetContext = new ODataQueryContext(model, entityClrType, path);
        var result = new ODataQueryOptions<TEntity>(entitySetContext, context.Request);

        return await ValueTask.FromResult(result);
    }

    /// <summary>
    /// Populates metadata for the related <see cref="Endpoint"/> and <see cref="ParameterInfo"/>.
    /// </summary>
    /// <param name="parameter" >The parameter info.</param>
    /// <param name="builder">The endpoint builder that we can add metadata into.</param>
    public static void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
    {
        // Make sure we have the metadata added into the endpoint.
        // Shall we build the 'EdmModel' here? ==> Emm...No, because the 'convention' runs after this population.
        // If developer calls 'WithODataModel()', then any model created here will be replaced. So, no need/required to create model here.
        ODataEndpointConventionBuilderExtensions.ConfigureODataMetadata(builder, null);
   }

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
