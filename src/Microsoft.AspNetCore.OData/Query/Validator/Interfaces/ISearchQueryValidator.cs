//-----------------------------------------------------------------------------
// <copyright file="ISearchQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provides the interface used to validate a <see cref="SearchQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface ISearchQueryValidator
{
    /// <summary>
    /// Validates the OData $search query.
    /// </summary>
    /// <param name="searchQueryOption">The $search query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(SearchQueryOption searchQueryOption, ODataValidationSettings validationSettings);
}
