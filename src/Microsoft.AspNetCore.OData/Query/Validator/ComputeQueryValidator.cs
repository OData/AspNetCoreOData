//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="ComputeQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class ComputeQueryValidator : IComputeQueryValidator
{
    /// <summary>
    /// Validates a <see cref="ComputeQueryOption" />.
    /// </summary>
    /// <param name="computeQueryOption">The $compute query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings)
    {
        if (computeQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(computeQueryOption));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        // so far, we don't have validation rules here for $compute
        // because 'DefaultQuerySetting' doesn't have configuration for $compute
        // we can only let ODL to parse and verify the compute clause,
        // however, developer can override this method add his own rules
        _ = computeQueryOption.ComputeClause;
    }

	/// <summary>
	/// Attempts to validate the <see cref="ComputeQueryOption" />.
	/// </summary>
	/// <param name="computeQueryOption">The $compute query.</param>
	/// <param name="validationSettings">The validation settings.</param>
	/// <param name="validationErrors">When this method returns, contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
	/// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
	public virtual bool TryValidate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        if (computeQueryOption == null || validationSettings == null)
        {
            // Use a single allocation for the error list only when needed
            // Pre-allocate with a reasonable default capacity.
            List<string> errors = new List<string>(2);

            if (computeQueryOption == null)
            {
                errors.Add(Error.ArgumentNull(nameof(computeQueryOption)).Message);
            }

            if (validationSettings == null)
            {
                errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
            }

            validationErrors = errors;
            return false;
        }

        try
        {
            // Let ODL parse and verify the compute clause
            _ = computeQueryOption.ComputeClause;
        }
        catch (ODataException ex)
        {
            validationErrors = new[] { ex.Message };
            return false;
        }

        validationErrors = Array.Empty<string>();
        return true;
    }
}
