//-----------------------------------------------------------------------------
// <copyright file="SkipQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="SkipQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class SkipQueryValidator : ISkipQueryValidator
{
    /// <summary>
    /// Validates a <see cref="SkipQueryOption" />.
    /// </summary>
    /// <param name="skipQueryOption">The $skip query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings)
    {
        if (skipQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(skipQueryOption));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        if (skipQueryOption.Value > validationSettings.MaxSkip)
        {
            throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxSkip, AllowedQueryOptions.Skip, skipQueryOption.Value));
        }
    }

    /// <summary>
    /// Attempts to validate the <see cref="SkipQueryOption" />.
    /// </summary>
    /// <param name="skipQueryOption">The $skip query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of <see cref="ODataException"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors)
    {
        List<ODataException> errors = new List<ODataException>();

        if (skipQueryOption == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(skipQueryOption)).Message));
        }

        if (validationSettings == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(validationSettings)).Message));
        }

        if (skipQueryOption.Value > validationSettings.MaxSkip)
        {
            errors.Add(new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxSkip, AllowedQueryOptions.Skip, skipQueryOption.Value)));
        }

        // If there are any errors, return false
        validationErrors = errors;
        return errors.Count == 0;
    }
}
