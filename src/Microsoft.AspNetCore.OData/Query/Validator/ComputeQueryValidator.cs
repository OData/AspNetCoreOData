//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
    /// <param name="validationErrors">When this method returns, contains a collection of <see cref="ODataException"/> instances describing any
    /// validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors)
    {
        List<ODataException> errors = new List<ODataException>();

        if (computeQueryOption == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(computeQueryOption)).Message));
        }

        if (validationSettings == null)
        {
            errors.Add(new ODataException(Error.ArgumentNull(nameof(validationSettings)).Message));
        }

        // If there are parameter errors, return early
        if (errors.Count > 0)
        {
            validationErrors = errors;
            return false;
        }

        try
        {
            // so far, we don't have validation rules here for $compute
            // because 'DefaultQuerySetting' doesn't have configuration for $compute
            // we can only let ODL to parse and verify the compute clause,
            // however, developer can override this method add his own rules
            _ = computeQueryOption.ComputeClause;
        }
        catch (ODataException ex)
        {
            errors.Add(ex);
        }

        validationErrors = errors;
        return errors.Count == 0;
    }
}
