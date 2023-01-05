//-----------------------------------------------------------------------------
// <copyright file="SkipQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SkipQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SkipQueryValidator : ISkipQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SkipQueryOption" />.
        /// </summary>
        /// <param name="skipQueryOption">The $skip query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SkipQueryOption skipQueryOption, ODataValidationSettings validationSettings)
        {
            if (skipQueryOption == null)
            {
                throw Error.ArgumentNull(nameof(skipQueryOption));
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            if (skipQueryOption.Value > validationSettings.MaxSkip)
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxSkip, AllowedQueryOptions.Skip, skipQueryOption.Value));
            }
        }
    }
}
