// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.ModelBuilder.Config;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="SkipTokenQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class SkipTokenQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="SkipTokenQueryOption" />.
        /// </summary>
        /// <param name="skipToken">The $skiptoken query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(SkipTokenQueryOption skipToken, ODataValidationSettings validationSettings)
        {
            if (skipToken == null)
            {
                throw Error.ArgumentNull(nameof(skipToken));
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            if (skipToken.Context != null)
            {
                DefaultQuerySettings defaultSetting = skipToken.Context.DefaultQuerySettings;
                if (!defaultSetting.EnableSkipToken)
                {
                    throw new ODataException(Error.Format(SRResources.NotAllowedQueryOption, AllowedQueryOptions.SkipToken, "AllowedQueryOptions"));
                }
            }
        }

        internal static SkipTokenQueryValidator GetSkipTokenQueryValidator(ODataQueryContext context)
        {
            return context?.RequestContainer?.GetRequiredService<SkipTokenQueryValidator>() ?? new SkipTokenQueryValidator();
        }
    }
}
