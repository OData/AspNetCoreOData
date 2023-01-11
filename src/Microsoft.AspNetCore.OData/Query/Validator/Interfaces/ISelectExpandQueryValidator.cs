//-----------------------------------------------------------------------------
// <copyright file="ISelectExpandQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="SelectExpandQueryOption"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface ISelectExpandQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="selectExpandQueryOption">The $select and $expand query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(SelectExpandQueryOption selectExpandQueryOption, ODataValidationSettings validationSettings);
    }
}
