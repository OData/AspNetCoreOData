//-----------------------------------------------------------------------------
// <copyright file="TopQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    /// <param name="validationErrors">Contains a collection of <see cref="ODataException"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors)
    {
        List<ODataException> errors = new List<ODataException>();

        if (topQueryOption == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(topQueryOption)).Message));
        }

        if (validationSettings == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(validationSettings)).Message));
        }

        // If there are parameter errors, return early
        if (errors.Count != 0)
        {
            validationErrors = errors;
            return false;
        }

        if (topQueryOption != null && validationSettings != null && topQueryOption.Value > validationSettings.MaxTop)
        {
            errors.Add(new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxTop, AllowedQueryOptions.Top, topQueryOption.Value)));
        }

        // If there are parameter errors, return early
        if (errors.Count != 0)
        {
            validationErrors = errors;
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
            errors.Add(new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop, AllowedQueryOptions.Top, topQueryOption.Value)));
        }

        // If there are any errors, return false
        validationErrors = errors;
        return errors.Count == 0;
    }
}
