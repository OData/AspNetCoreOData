//-----------------------------------------------------------------------------
// <copyright file="TopQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="TopQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class TopQueryValidator : ITopQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="TopQueryOption" />.
        /// </summary>
        /// <param name="topQueryOption">The $top query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(TopQueryOption topQueryOption, ODataValidationSettings validationSettings)
        {
            if (topQueryOption == null)
            {
                throw Error.ArgumentNull(nameof(topQueryOption));
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            if (topQueryOption.Value > validationSettings.MaxTop)
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, validationSettings.MaxTop,
                    AllowedQueryOptions.Top, topQueryOption.Value));
            }

            int maxTop;
            IEdmProperty property = topQueryOption.Context.TargetProperty;
            IEdmStructuredType structuredType = topQueryOption.Context.TargetStructuredType;

            if (EdmHelpers.IsTopLimitExceeded(
                property,
                structuredType,
                topQueryOption.Context.Model,
                topQueryOption.Value, topQueryOption.Context.DefaultQuerySettings,
                out maxTop))
            {
                throw new ODataException(Error.Format(SRResources.SkipTopLimitExceeded, maxTop,
                    AllowedQueryOptions.Top, topQueryOption.Value));
            }
        }
    }
}
