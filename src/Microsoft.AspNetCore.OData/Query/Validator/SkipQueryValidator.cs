﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SkipQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SkipQueryValidator
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

        internal static SkipQueryValidator GetSkipQueryValidator(ODataQueryContext context)
        {
            if (context == null || context.RequestContainer == null)
            {
                return new SkipQueryValidator();
            }

            return context.RequestContainer.GetRequiredService<SkipQueryValidator>();
        }
    }
}
