//-----------------------------------------------------------------------------
// <copyright file="SkipTokenQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    /// <param name="validationErrors">Contains a collection of <see cref="ODataException"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(SkipTokenQueryOption skipToken, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors)
    {
        List<ODataException> errors = new List<ODataException>();

        if (skipToken == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(skipToken)).Message));
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

        if (skipToken?.Context != null)
        {
            DefaultQueryConfigurations defaultConfigs = skipToken.Context.DefaultQueryConfigurations;
            if (!defaultConfigs.EnableSkipToken)
            {
                errors.Add(new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.SkipToken, nameof(AllowedQueryOptions))));
            }
        }

        // If there are any errors, return false
        validationErrors = errors;
        return errors.Count == 0;
    }
}
