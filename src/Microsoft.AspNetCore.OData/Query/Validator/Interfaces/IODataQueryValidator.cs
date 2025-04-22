//-----------------------------------------------------------------------------
// <copyright file="IODataQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="ODataQueryOptions"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface IODataQueryValidator
{
    /// <summary>
    /// Validates the <see cref="ODataQueryOptions" />.
    /// </summary>
    /// <param name="options">The OData query options to validate.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(ODataQueryOptions options, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="ODataQueryOptions" />.
    /// </summary>
    /// <param name="options">The OData query options to validate.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">The collection of validation errors.</param>
    /// <returns>True if the validation succeeded; otherwise, false.</returns>
    bool TryValidate(ODataQueryOptions options, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors);
}
