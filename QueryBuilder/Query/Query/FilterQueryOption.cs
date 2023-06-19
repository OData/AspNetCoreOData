using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using QueryBuilder.Query.Expressions;
using QueryBuilder.Query.Validator;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace QueryBuilder.Query
{
    /// <summary>
    /// This defines a $filter OData query option for querying.
    /// </summary>
    public class FilterQueryOption
    {
        private FilterClause _filterClause;
        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initialize a new instance of <see cref="FilterQueryOption"/> based on the raw $filter value and
        /// an EdmModel from <see cref="ODataQueryContext2"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext2"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public FilterQueryOption(string rawValue, ODataQueryContext2 context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            Validator = context.Validators.GetFilterQueryValidator();
            _queryOptionParser = queryOptionParser;
        }

        internal FilterQueryOption(ODataQueryContext2 context, FilterClause filterClause)
        {
            _filterClause = filterClause;
            Context = context;
            Validator = context.Validators.GetFilterQueryValidator();
        }

        // This constructor is intended for unit testing only.
        internal FilterQueryOption(string rawValue, ODataQueryContext2 context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            Context = context;
            RawValue = rawValue;
            Validator = context.Validators.GetFilterQueryValidator();
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$filter", rawValue } }/*,
                context.RequestContainer*/);
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext2"/>.
        /// </summary>
        public ODataQueryContext2 Context { get; private set; }

        /// <summary>
        /// Gets or sets the Filter Query Validator
        /// </summary>
        public IFilterQueryValidator Validator { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ComputeQueryOption"/>.
        /// </summary>
        public ComputeQueryOption Compute { get; set; }

        /// <summary>
        /// Gets the parsed <see cref="FilterClause"/> for this query option.
        /// </summary>
        public FilterClause FilterClause
        {
            get
            {
                if (_filterClause == null)
                {
                    _filterClause = _queryOptionParser.ParseFilter();
                    SingleValueNode filterExpression = _filterClause.Expression.Accept(
                        new ParameterAliasNodeTranslator(_queryOptionParser.ParameterAliasNodes)) as SingleValueNode;
                    filterExpression = filterExpression ?? new ConstantNode(null);
                    _filterClause = new FilterClause(filterExpression, _filterClause.RangeVariable);
                }

                return _filterClause;
            }
            internal set { _filterClause = value; }
        }

        /// <summary>
        ///  Gets the raw $filter value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Apply the filter query to the given IQueryable.
        /// </summary>
        /// <remarks>
        /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies
        /// how this method should handle null propagation.
        /// </remarks>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new <see cref="IQueryable"/> after the filter query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }
            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }

            FilterClause filterClause = FilterClause;
            Contract.Assert(filterClause != null);

            QueryBinderContext binderContext = new QueryBinderContext(Context.Model, querySettings, Context.ElementClrType);

            if (Compute != null)
            {
                binderContext.AddComputedProperties(Compute.ComputeClause.ComputedItems);
            }

            IFilterBinder binder = Context.Binders.GetFilterBinder();
            return binder.ApplyBind(query, filterClause, binderContext);
        }

        /// <summary>
        /// Validate the filter query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        public void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }
    }
}
