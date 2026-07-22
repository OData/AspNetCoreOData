//-----------------------------------------------------------------------------
// <copyright file="SearchQueryOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This defines a $search OData query option for querying.
/// The $search system query option restricts the result to include only those items matching the specified search expression.
/// The definition of what it means to match is dependent upon the implementation.
/// </summary>
public class SearchQueryOption
{
    private SearchClause _searchClause;
    private ODataQueryOptionParser _queryOptionParser;

    /// <summary>
    /// Initialize a new instance of <see cref="SearchQueryOption"/> based on the raw $search value and
    /// an EdmModel from <see cref="ODataQueryContext"/>.
    /// </summary>
    /// <param name="rawValue">The raw value for $search query. It can be null or empty.</param>
    /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
    /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
    public SearchQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
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

        Context = context;
        RawValue = rawValue;
        _queryOptionParser = queryOptionParser;
        ResultClrType = Context.ElementClrType;
    }

    // This constructor is intended for unit testing only.
    internal SearchQueryOption(string rawValue, ODataQueryContext context)
    {
        if (string.IsNullOrEmpty(rawValue))
        {
            throw Error.ArgumentNullOrEmpty(nameof(rawValue));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        Context = context;
        RawValue = rawValue;

        _queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$search", rawValue } },
            context.RequestContainer);

        if (context.RequestContainer == null)
        {
            // By default, let's enable the property name case-insensitive
            _queryOptionParser.Resolver = ODataQueryContext.DefaultCaseInsensitiveResolver;
        }
    }

    /// <summary>
    /// Gets the given <see cref="ODataQueryContext"/>.
    /// </summary>
    public ODataQueryContext Context { get; }

    /// <summary>
    /// ClrType for result of transformations
    /// </summary>
    public Type ResultClrType { get; }

    /// <summary>
    /// Gets the parsed <see cref="SearchClause"/> for this query option.
    /// </summary>
    public SearchClause SearchClause
    {
        get
        {
            if (_searchClause == null)
            {
                _searchClause = _queryOptionParser.ParseSearch();
            }

            return _searchClause;
        }
    }

    /// <summary>
    ///  Gets the raw $search value.
    /// </summary>
    public string RawValue { get; }

    /// <summary>
    /// Apply the $search query to the given IQueryable.
    /// </summary>
    /// <remarks>
    /// The <see cref="ODataQuerySettings.HandleNullPropagation"/> property specifies how this method should handle null propagation.
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

        ISearchBinder binder = Context.GetSearchBinder();
        if (binder == null)
        {
            // If the developer doesn't provide the search binder, let's ignore the $search clause.
            return query;
        }

        if (Context.ElementClrType == null)
        {
            throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
        }

        QueryBinderContext binderContext = new QueryBinderContext(Context.Model, querySettings, Context.ElementClrType);

        return binder.ApplyBind(query, SearchClause, binderContext);
    }

    /// <summary>
    /// Validate the $search query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
    /// </summary>
    /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
    public void Validate(ODataValidationSettings validationSettings)
    {
        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        ISearchQueryValidator validator = Context.GetSearchQueryValidator();
        if (validator != null)
        {
            // If the developer doesn't provide the search validator, let's ignore the $search validation.
            validator.Validate(this, validationSettings);
        }
    }

	/// <summary>
	/// Attempts to validate the $search query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
	/// </summary>
	/// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
	/// <param name="validationErrors">When this method returns, contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
	/// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
	public bool TryValidate(ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        if (validationSettings == null)
        {
            validationErrors = new[] { Error.ArgumentNull(nameof(validationSettings)).Message };
            return false;
        }

        ISearchQueryValidator validator = Context.GetSearchQueryValidator();

        // If the developer doesn't provide the search validator, let's ignore the $search validation.
        if (validator != null && !validator.TryValidate(this, validationSettings, out validationErrors))
        {
            return false;
        }

        validationErrors = Array.Empty<string>();
        return true;
    }
}
