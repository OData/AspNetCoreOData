//-----------------------------------------------------------------------------
// <copyright file="ISearchQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provides the interface used to validate a <see cref="SearchQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface ISearchQueryValidator
{
    /// <summary>
    /// Validates the <see cref="SearchQueryOption" />.
    /// </summary>
    /// <param name="searchQueryOption">The $search query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(SearchQueryOption searchQueryOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="SearchQueryOption" />.
    /// </summary>
    /// <param name="searchQueryOption"></param>
    /// <param name="validationSettings"></param>
    /// <param name="validationErrors">Contains a collection of <see cref="ODataException"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(SearchQueryOption searchQueryOption, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors);
}
