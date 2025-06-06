//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="SkipTokenQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class SkipTokenQueryValidator : ISkipTokenQueryValidator
{
    /// <summary>
    /// Validates a <see cref="SkipTokenQueryOption" />.
    /// </summary>
    /// <param name="skipToken">The $skiptoken query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(SkipTokenQueryOption skipToken, ODataValidationSettings validationSettings)
    {
        if (skipToken == null)
        {
            throw Error.ArgumentNull(nameof(skipToken));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        if (skipToken.Context != null)
        {
            DefaultQueryConfigurations defaultConfigs = skipToken.Context.DefaultQueryConfigurations;
            if (!defaultConfigs.EnableSkipToken)
            {
                throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.SkipToken, "AllowedQueryOptions"));
            }
        }
    }

    /// <summary>
    /// Attempts to validate the <see cref="SkipTokenQueryOption" />.
    /// </summary>
    /// <param name="skipToken">The $skiptoken query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(SkipTokenQueryOption skipToken, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        if(skipToken == null || validationSettings == null)
        {
            // Pre-allocate with a reasonable default capacity.
            List<string> errors = new List<string>(2);

            if (skipToken == null)
            {
                errors.Add(Error.ArgumentNull(nameof(skipToken)).Message);
            }

            if (validationSettings == null)
            {
                errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
            }

            validationErrors = errors;
            return false;
        }

        if (skipToken?.Context != null)
        {
            DefaultQueryConfigurations defaultConfigs = skipToken.Context.DefaultQueryConfigurations;
            if (!defaultConfigs.EnableSkipToken)
            {
                validationErrors = new[] { Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.SkipToken, nameof(AllowedQueryOptions)) };
                return false;
            }
        }

        // If there are any errors, return false
        validationErrors = Array.Empty<string>();
        return true;
    }
}
