//-----------------------------------------------------------------------------
// <copyright file="TopQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="TopQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class TopQueryValidator : ITopQueryValidator
{
    /// <summary>
    /// Validates a <see cref="TopQueryOption" />.
    /// </summary>
    /// <param name="topQueryOption">The $top query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings)
    {
        if (topQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(topQueryOption));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        if (topQueryOption.Value > validationSettings.MaxTop)
        {
            throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxTop,
                AllowedQueryOptions.Top, topQueryOption.Value));
        }

        int maxTop;
        IEdmProperty property = topQueryOption.Context.TargetProperty;
        IEdmStructuredType structuredType = topQueryOption.Context.TargetStructuredType;

        if (EdmHelpers.IsTopLimitExceeded(
            property,
            structuredType,
            topQueryOption.Context.Model,
            topQueryOption.Value, topQueryOption.Context.DefaultQueryConfigurations,
            out maxTop))
        {
            throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop,
                AllowedQueryOptions.Top, topQueryOption.Value));
        }
    }

    /// <summary>
    /// Attempts to validate the <see cref="TopQueryOption" />.
    /// </summary>
    /// <param name="topQueryOption">The $top query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        if (topQueryOption == null || validationSettings == null)
        {
            // Pre-allocate with a reasonable default capacity.
            List<string> errors = new List<string>(2);

            if (topQueryOption == null)
            {
                errors.Add(Error.ArgumentNull(nameof(topQueryOption)).Message);
            }

            if (validationSettings == null)
            {
                errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
            }

            validationErrors = errors;
            return false;
        }

        if (topQueryOption.Value > validationSettings.MaxTop)
        {
            validationErrors = new[] { Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxTop, AllowedQueryOptions.Top, topQueryOption.Value) };
            return false;
        }

        int maxTop;
        IEdmProperty property = topQueryOption.Context.TargetProperty;
        IEdmStructuredType structuredType = topQueryOption.Context.TargetStructuredType;

        if (EdmHelpers.IsTopLimitExceeded(
            property,
            structuredType,
            topQueryOption.Context.Model,
            topQueryOption.Value, topQueryOption.Context.DefaultQueryConfigurations,
            out maxTop))
        {
            validationErrors = new[] { Error.Format(SRResources.SkipTopLimitExceeded, maxTop, AllowedQueryOptions.Top, topQueryOption.Value) };
            return false;
        }

        // If there are any errors, return false
        validationErrors = Array.Empty<string>();
        return true;
    }
}
