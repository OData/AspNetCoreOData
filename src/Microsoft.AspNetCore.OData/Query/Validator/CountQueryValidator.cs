//-----------------------------------------------------------------------------
// <copyright file="CountQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate a <see cref="CountQueryOption"/> 
    /// based on the <see cref="ODataValidationSettings"/>.
    /// </summary>
    public class CountQueryValidator : ICountQueryValidator
    {
        /// <summary>
        /// Validates a <see cref="CountQueryOption" />.
        /// </summary>
        /// <param name="countQueryOption">The $count query.</param>
        /// <param name="validationSettings">The validation settings.</param>
        public virtual void Validate(CountQueryOption countQueryOption, ODataValidationSettings validationSettings)
        {
            if (countQueryOption == null)
            {
                throw Error.ArgumentNull(nameof(countQueryOption));
            }

            if (validationSettings == null)
            {
                throw Error.ArgumentNull(nameof(validationSettings));
            }

            ODataPath path = countQueryOption.Context.Path;

            if (path != null && path.Count > 0)
            {
                IEdmProperty property = countQueryOption.Context.TargetProperty;
                IEdmStructuredType structuredType = countQueryOption.Context.TargetStructuredType;
                string name = countQueryOption.Context.TargetName;
                if (EdmHelpers.IsNotCountable(property, structuredType,
                    countQueryOption.Context.Model,
                    countQueryOption.Context.DefaultQuerySettings.EnableCount))
                {
                    if (property == null)
                    {
                        throw new InvalidOperationException(Error.Format(SRResources.NotCountableEntitySetUsedForCount, name));
                    }
                    else
                    {
                        throw new InvalidOperationException(Error.Format(SRResources.NotCountablePropertyUsedForCount, name));
                    }
                }
            }
        }

        internal static ICountQueryValidator GetCountQueryValidator(ODataQueryContext context)
        {
            return context?.RequestContainer?.GetRequiredService<ICountQueryValidator>()
                ?? new CountQueryValidator();
        }
    }
}
