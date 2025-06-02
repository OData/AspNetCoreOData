//-----------------------------------------------------------------------------
// <copyright file="CountQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="CountQueryOption"/> 
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class CountQueryValidator : ICountQueryValidator
{
    /// <summary>
    /// Validates a <see cref="CountQueryOption" />.
    /// </summary>
    /// <param name="countQueryOption">The $count query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings)
    {
        if (countQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(countQueryOption));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        ODataPath path = countQueryOption.Context.Path;

        if (path != null && path.Count > 0)
        {
            IEdmProperty property = countQueryOption.Context.TargetProperty;
            IEdmStructuredType structuredType = countQueryOption.Context.TargetStructuredType;
            string name = countQueryOption.Context.TargetName;
            if (EdmHelpers.IsNotCountable(property, structuredType,
                countQueryOption.Context.Model,
                countQueryOption.Context.DefaultQueryConfigurations.EnableCount))
            {
                if (property == null)
                {
                    throw new InvalidOperationException(Error.Format(SRResources.NotCountableEntitySetUsedForCount, name));
                }
                else
                {
                    throw new InvalidOperationException(Error.Format(SRResources.NotCountablePropertyUsedForCount, name));
                }
            }
        }
    }

    /// <summary>
    /// Attempts to validate the <see cref="CountQueryOption" />.
    /// </summary>
    /// <param name="countQueryOption"></param>
    /// <param name="validationSettings"></param>
    /// <param name="validationErrors">When this method returns, contains a collection of <see cref="string"/> instances describing any
    /// validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        if(countQueryOption == null || validationSettings == null)
        {
            // Preallocate with a reasonable default capacity.
            List<string> errors = new List<string>(2);

            if (countQueryOption == null)
            {
                errors.Add(Error.ArgumentNull(nameof(countQueryOption)).Message);
            }

            if (validationSettings == null)
            {
                errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
            }

            validationErrors = errors;
            return false;
        }

        ODataPath path = countQueryOption.Context.Path;

        if (path != null && path.Count > 0)
        {
            IEdmProperty property = countQueryOption.Context.TargetProperty;
            IEdmStructuredType structuredType = countQueryOption.Context.TargetStructuredType;
            string name = countQueryOption.Context.TargetName;
            if (EdmHelpers.IsNotCountable(property, structuredType,
                countQueryOption.Context.Model,
                countQueryOption.Context.DefaultQueryConfigurations.EnableCount))
            {
                string errorMessage = property == null
                    ? Error.Format(SRResources.NotCountableEntitySetUsedForCount, name)
                    : Error.Format(SRResources.NotCountablePropertyUsedForCount, name);

                validationErrors = new[] { errorMessage };
                return false;
            }
        }

        validationErrors = Array.Empty<string>();
        return true;
    }
}
