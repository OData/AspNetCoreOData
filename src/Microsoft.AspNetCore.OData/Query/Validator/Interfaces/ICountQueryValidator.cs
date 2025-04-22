//-----------------------------------------------------------------------------
// <copyright file="ICountQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="CountQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface ICountQueryValidator
{
    /// <summary>
    /// Validates the <see cref="CountQueryOption" />.
    /// </summary>
    /// <param name="countQueryOption">The $count query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="CountQueryOption" />.
    /// </summary>
    /// <param name="countQueryOption"></param>
    /// <param name="validationSettings"></param>
    /// <param name="validationErrors">Contains a collection of <see cref="ODataException"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors);
}
