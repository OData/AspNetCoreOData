using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using ODataQueryBuilder.Abstracts;
using ODataQueryBuilder.Query.Expressions;
//using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace ODataQueryBuilder.Query
{
    /// <summary>
    /// This defines a $apply OData query option for querying.
    /// </summary>
    public class ApplyQueryOption
    {
        private ApplyClause _applyClause;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="ApplyQueryOption"/> based on the raw $apply value and
        /// an EdmModel from <see cref="ODataQueryFundamentalsContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryFundamentalsContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public ApplyQueryOption(string rawValue, ODataQueryFundamentalsContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty(nameof(rawValue));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull(nameof(queryOptionParser));
            }

            RawValue = rawValue;
            Context = context;
            // TODO: Implement and add validator
            //Validator = new FilterQueryValidator();
            _queryOptionParser = queryOptionParser;
            ResultClrType = Context.ElementClrType;
        }

        // for unit test only
        internal ApplyQueryOption(string rawValue, ODataQueryFundamentalsContext context)
        {
            RawValue = rawValue;
            Context = context;
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryFundamentalsContext"/>.
        /// </summary>
        public ODataQueryFundamentalsContext Context { get; private set; }

        /// <summary>
        /// ClrType for result of transformations
        /// </summary>
        public Type ResultClrType { get; private set; }

        /// <summary>
        /// Gets the parsed <see cref="ApplyClause"/> for this query option.
        /// </summary>
        public ApplyClause ApplyClause
        {
            get
            {
                if (_applyClause == null)
                {
                    _applyClause = _queryOptionParser.ParseApply();
                }

                return _applyClause;
            }
        }

        /// <summary>
        ///  Gets the raw $apply value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Apply the apply query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, IAssemblyResolver assembliesResolver, IFilterBinder filterBinder)
        {
            if (query == null)
            {
                throw Error.ArgumentNull(nameof(query));
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull(nameof(querySettings));
            }

            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            // Linq to SQL not supported for $apply
            if (query.Provider.GetType().Namespace == HandleNullPropagationOptionHelper.Linq2SqlQueryProviderNamespace)
            {
                throw Error.NotSupported(SRResources.ApplyQueryOptionNotSupportedForLinq2SQL);
            }

            ApplyClause applyClause = ApplyClause;
            Contract.Assert(applyClause != null);

            // The IWebApiAssembliesResolver service is internal and can only be injected by WebApi.
            // This code path may be used in cases when the service container is not available
            // and the service container is available but may not contain an instance of IWebApiAssembliesResolver.
            /*IAssemblyResolver assembliesResolver = AssemblyResolverHelper.Default;
            if (Context.RequestContainer != null)
            {
                IAssemblyResolver injectedResolver = Context.RequestContainer.GetService<IAssemblyResolver>();
                if (injectedResolver != null)
                {
                    assembliesResolver = injectedResolver;
                }
            }*/

            foreach (var transformation in applyClause.Transformations)
            {
                if (transformation.Kind == TransformationNodeKind.Aggregate || transformation.Kind == TransformationNodeKind.GroupBy)
                {
                    var binder = new AggregationBinder(querySettings, assembliesResolver, ResultClrType, Context.Model, transformation);
                    query = binder.Bind(query);
                    this.ResultClrType = binder.ResultClrType;
                }
                else if (transformation.Kind == TransformationNodeKind.Compute)
                {
                    var binder = new ComputeBinder(querySettings, assembliesResolver, ResultClrType, Context.Model, (ComputeTransformationNode)transformation);
                    query = binder.Bind(query);
                    this.ResultClrType = binder.ResultClrType;
                }
                else if (transformation.Kind == TransformationNodeKind.Filter)
                {
                    var filterTransformation = transformation as FilterTransformationNode;

                    //IFilterBinder binder = Context.GetFilterBinder();
                    QueryBinderContext binderContext = new QueryBinderContext(Context.Model, querySettings, ResultClrType);

                    query = filterBinder.ApplyBind(query, filterTransformation.FilterClause, binderContext);
                }
            }

            return query;
        }
    }
}
