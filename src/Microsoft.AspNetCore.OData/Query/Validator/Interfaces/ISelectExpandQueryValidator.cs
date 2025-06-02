//-----------------------------------------------------------------------------
// <copyright file="ISelectExpandQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="SelectExpandQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface ISelectExpandQueryValidator
{
    /// <summary>
    /// Validates the <see cref="SelectExpandQueryOption"/>
    /// </summary>
    /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="SelectExpandQueryOption"/>.
    /// </summary>
    /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of <see cref="string"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors);
}
