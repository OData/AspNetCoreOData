//-----------------------------------------------------------------------------
// <copyright file="IFilterQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="FilterQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface IFilterQueryValidator
{
    /// <summary>
    /// Validates the <see cref="FilterQueryOption" />.
    /// </summary>
    /// <param name="filterQueryOption">The $filter query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="FilterQueryOption" />.
    /// </summary>
    /// <param name="filterQueryOption">The $filter query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(FilterQueryOption filterQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors);
}
