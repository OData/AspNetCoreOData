//-----------------------------------------------------------------------------
// <copyright file="ISkipQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="SkipQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface ISkipQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SkipQueryOption" />.
        /// </summary>
        /// <param name="skipQueryOption">The $skip query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings);
    }
}
