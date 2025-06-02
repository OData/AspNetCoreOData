//-----------------------------------------------------------------------------
// <copyright file="IOrderByQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="OrderByQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface IOrderByQueryValidator
{
    /// <summary>
    /// Validates a <see cref="OrderByQueryOption" />.
    /// </summary>
    /// <param name="orderByOption">The $orderby query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(OrderByQueryOption orderByOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="OrderByQueryOption" />.
    /// </summary>
    /// <param name="orderByOption">The $orderby query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(OrderByQueryOption orderByOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors);
}
