//-----------------------------------------------------------------------------
// <copyright file="IComputeQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate a <see cref="ComputeQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface IComputeQueryValidator
{
    /// <summary>
    /// Validates the <see cref="ComputeQueryOption" />.
    /// </summary>
    /// <param name="computeQueryOption">The $compute query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings);

    /// <summary>
    /// Attempts to validate the <see cref="ComputeQueryOption" />.
    /// </summary>
    /// <param name="computeQueryOption">The $compute query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of <see cref="ODataException"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings, out IEnumerable<ODataException> validationErrors);
}
