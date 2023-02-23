//-----------------------------------------------------------------------------
// <copyright file="IODataQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Provide the interface used to validate a <see cref="ODataQueryOptions"/>
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public interface IODataQueryValidator
    {
        /// <summary>
        /// Validates the OData query.
        /// </summary>
        /// <param name="options">The OData query options to validate.</param>
        /// <param name="validationSettings">The validation settings.</param>
        void Validate(ODataQueryOptions options, ODataValidationSettings validationSettings);
    }
}
