using System.Linq;
//using Microsoft.AspNetCore.Http;
using ODataQueryBuilder.Common;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.Extensions.Primitives;
using ODataQueryBuilder.Abstracts;
using Microsoft.OData.ModelBuilder;
using ODataQueryBuilder.Query.Expressions;

namespace ODataQueryBuilder.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    // TODO: Fix attribute compilation below:
    //[ODataQueryParameterBinding]
    public class ODataQueryOptionsFundamentals<TEntity> : ODataQueryOptionsFundamentals
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptionsFundamentals"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryFundamentalsContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryFundamentalsContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="request">The incoming request message</param>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public ODataQueryOptionsFundamentals(ODataQueryFundamentalsContext context, IEnumerable<KeyValuePair<string, StringValues>> requestQueryCollection)
            : base(context, requestQueryCollection)
        {
            if (QueryContext.ElementClrType == null)
            {
                throw Error.Argument("context", SRResources.ElementClrTypeNull, typeof(ODataQueryFundamentalsContext).Name);
            }

            if (context.ElementClrType != typeof(TEntity))
            {
                throw Error.Argument("context", SRResources.EntityTypeMismatch, context.ElementClrType.FullName, typeof(TEntity).FullName);
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfMatch header.
        /// </summary>
        public new ETag<TEntity> IfMatch(StringValues ifMatchValues, IETagHandler etagHandler)
        {
            return base.IfMatch(ifMatchValues, etagHandler) as ETag<TEntity>;
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from IfNoneMatch header.
        /// </summary>
        public new ETag<TEntity> IfNoneMatch(StringValues ifNoneMatchValues, IETagHandler etagHandler)
        {
            return base.IfNoneMatch(ifNoneMatchValues, etagHandler) as ETag<TEntity>;
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag>.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal override ETag GetETag(EntityTagHeaderValue entityTagHeaderValue, IETagHandler etagHandler)
        {
            return HttpRequestODataQueryExtensions.GetETag<TEntity>(entityTagHeaderValue, etagHandler, (IEdmModel) null, (IEdmNavigationSource) null);
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag>.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal ETag GetETag(EntityTagHeaderValue entityTagHeaderValue, IETagHandler etagHandler, IEdmModel model, IEdmNavigationSource source)
        {
            return HttpRequestODataQueryExtensions.GetETag<TEntity>(entityTagHeaderValue, etagHandler, model, source);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to, as well as a <see cref="bool"/> representing whether the results have been limited to a page size.</returns>
        public override (IQueryable, bool) ApplyTo(IQueryable query, ODataQuerySettings querySettings, AllowedQueryOptions ignoreQueryOptions)
        {
            ValidateQuery(query);
            return base.ApplyTo(query, querySettings, ignoreQueryOptions);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to, as well as a <see cref="bool"/> representing whether the results have been limited to a page size.</returns>
        public override (IQueryable, bool) ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            ValidateQuery(query);
            return base.ApplyTo(query, querySettings);
        }

        private static void ValidateQuery(IQueryable query)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (!TypeHelper.IsTypeAssignableFrom(typeof(TEntity), query.ElementType))
            {
                throw Error.Argument("query", SRResources.CannotApplyODataQueryOptionsOfT, typeof(ODataQueryOptionsFundamentals).Name, typeof(TEntity).FullName, typeof(IQueryable).Name, query.ElementType.FullName);
            }
        }
    }
}
