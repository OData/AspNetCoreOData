//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryOptions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This defines a $apply OData query option for querying.
/// </summary>
public class ApplyQueryOption
{
    private ApplyClause _applyClause;
    private ODataQueryOptionParser _queryOptionParser;

    /// <summary>
    /// Initialize a new instance of <see cref="ApplyQueryOption"/> based on the raw $apply value and
    /// an EdmModel from <see cref="ODataQueryContext"/>.
    /// </summary>
    /// <param name="rawValue">The raw value for $filter query. It can be null or empty.</param>
    /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
    /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
    public ApplyQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
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
        Validator = context.GetApplyQueryValidator();
        _queryOptionParser = queryOptionParser;
        ResultClrType = Context.ElementClrType;
    }

    // Used when only the raw value is known.
    internal ApplyQueryOption(string rawValue)
    {
        if (string.IsNullOrEmpty(rawValue))
        {
            throw Error.ArgumentNullOrEmpty(nameof(rawValue));
        }

        RawValue = rawValue;
    }

    // This constructor is intended for unit testing only.
    internal ApplyQueryOption(string rawValue, ODataQueryContext context)
    {
        if (string.IsNullOrEmpty(rawValue))
        {
            throw Error.ArgumentNullOrEmpty(nameof(rawValue));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        RawValue = rawValue;
        Context = context;
        Validator = context.GetApplyQueryValidator();
        _queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$apply", rawValue } },
            context.RequestContainer);

        if (context.RequestContainer == null)
        {
            // By default, let's enable the property name case-insensitive
            _queryOptionParser.Resolver = ODataQueryContext.DefaultCaseInsensitiveResolver;
        }
    }

    /// <summary>
    ///  Gets the given <see cref="ODataQueryContext"/>.
    /// </summary>
    public ODataQueryContext Context { get; private set; }

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
    /// Gets or sets the $apply Query Validator.
    /// </summary>
    public IApplyQueryValidator Validator { get; set; }

    /// <summary>
    /// Validate the $apply query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
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
    public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
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
        IAssemblyResolver assembliesResolver = AssemblyResolverHelper.Default;
        if (Context.RequestContainer != null)
        {
            IAssemblyResolver injectedResolver = Context.RequestContainer.GetService<IAssemblyResolver>();
            if (injectedResolver != null)
            {
                assembliesResolver = injectedResolver;
            }
        }

        foreach (TransformationNode transformation in applyClause.Transformations)
        {
            if (transformation.Kind == TransformationNodeKind.Aggregate || transformation.Kind == TransformationNodeKind.GroupBy)
            {
                QueryBinderContext queryBinderContext = new QueryBinderContext(Context.Model, querySettings, ResultClrType);
                IAggregationBinder binder = Context.GetAggregationBinder();
                query = binder.ApplyBind(query, transformation, queryBinderContext, out Type resultClrType);
                this.ResultClrType = resultClrType;
            }
            else if (transformation.Kind == TransformationNodeKind.Compute)
            {
                ComputeTransformationNode computeTransformationNode = transformation as ComputeTransformationNode;

                IComputeBinder binder = Context.GetComputeBinder();
                QueryBinderContext binderContext = new QueryBinderContext(Context.Model, querySettings, ResultClrType);

                query = binder.ApplyBind(query, computeTransformationNode, binderContext, out Type resultClrType);
                this.ResultClrType = resultClrType;
            }
            else if (transformation.Kind == TransformationNodeKind.Filter)
            {
                FilterTransformationNode filterTransformation = transformation as FilterTransformationNode;

                IFilterBinder binder = Context.GetFilterBinder();
                QueryBinderContext binderContext = new QueryBinderContext(Context.Model, querySettings, ResultClrType);

                query = binder.ApplyBind(query, filterTransformation.FilterClause, binderContext);
            }
        }

        return query;
    }
}
