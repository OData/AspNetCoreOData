//-----------------------------------------------------------------------------
// <copyright file="IApplyQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Provide the interface used to validate an <see cref="ApplyQueryOption"/>
/// based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public interface IApplyQueryValidator
{
    /// <summary>
    /// Validates the OData query.
    /// </summary>
    /// <param name="applyQueryOption">The $apply query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    void Validate(ApplyQueryOption applyQueryOption, ODataValidationSettings validationSettings);
}
