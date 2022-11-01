//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="ComputeQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class ComputeQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="ComputeQueryOption" />.
        /// </summary>
        /// <param name="computeQueryOption">The $compute query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings)
        {
            // so far, we don't have validation rules here for $compute
            // however, customer can use this to inject the validator to add his own rules
        }

        internal static ComputeQueryValidator GetComputeQueryValidator(ODataQueryContext context)
        {
            return context?.RequestContainer?.GetService<ComputeQueryValidator>() ?? new ComputeQueryValidator();
        }
    }
}
