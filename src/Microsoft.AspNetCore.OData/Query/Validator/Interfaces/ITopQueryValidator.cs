//-----------------------------------------------------------------------------
// <copyright file="ITopQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="TopQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface ITopQueryValidator
{
    /// <summary>
    /// Validates a <see cref="TopQueryOption" />.
    /// </summary>
    /// <param name="topQueryOption">The $top query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="TopQueryOption" />.
    /// </summary>
    /// <param name="topQueryOption">The $top query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors);
}
