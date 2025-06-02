//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryOption.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query;

/// <summary>
/// This defines a $compute OData query option for querying.
/// The $compute system query option allows clients to define computed properties that can be used in a $select or within a $filter or $orderby expression.
/// Computed properties SHOULD be included as dynamic properties in the result and MUST be included if $select is specified with the computed property name, or star (*).
/// </summary>
public class ComputeQueryOption
{
    private ComputeClause _computeClause;
    private ODataQueryOptionParser _queryOptionParser;

    /// <summary>
    /// Initialize a new instance of <see cref="ComputeQueryOption"/> based on the raw $compute value and
    /// an EdmModel from <see cref="ODataQueryContext"/>.
    /// </summary>
    /// <param name="rawValue">The raw value for $compute query. It can be null or empty.</param>
    /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
    /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
    public ComputeQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
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
        Validator = context.GetComputeQueryValidator();
        _queryOptionParser = queryOptionParser;
        ResultClrType = Context.ElementClrType;
    }

    // This constructor is intended for unit testing only.
    internal ComputeQueryOption(string rawValue, ODataQueryContext context)
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

        Validator = context.GetComputeQueryValidator();
        _queryOptionParser = new ODataQueryOptionParser(
            context.Model,
            context.ElementType,
            context.NavigationSource,
            new Dictionary<string, string> { { "$compute", rawValue } },
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
    /// Gets the parsed <see cref="ComputeClause"/> for this query option.
    /// </summary>
    public ComputeClause ComputeClause
    {
        get
        {
            if (_computeClause == null)
            {
                _computeClause = _queryOptionParser.ParseCompute();
            }

            return _computeClause;
        }
    }

    /// <summary>
    ///  Gets the raw $compute value.
    /// </summary>
    public string RawValue { get; }

    /// <summary>
    /// Gets or sets the $compute Query Validator.
    /// </summary>
    public IComputeQueryValidator Validator { get; set; }

    /// <summary>
    /// Validate the $compute query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
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
    /// Attempts to validate the $compute query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
    /// </summary>
    /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
    /// <param name="validationErrors">When this method returns, contains a collection of <see cref="string"/> instances describing any
    /// validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public bool TryValidate(ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        List<string> errors = new List<string>();

        if (validationSettings == null)
        {
            errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
        }

        // If there are parameter errors, return early
        if (errors.Count != 0)
        {
            validationErrors = errors;
            return false;
        }

        validationErrors = errors;

        if (Validator != null)
        {
            Validator.TryValidate(this, validationSettings, out validationErrors);
        }

        return !validationErrors.Any();
    }
}
