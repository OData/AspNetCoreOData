//-----------------------------------------------------------------------------
// <copyright file="ComputeQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="ComputeQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class ComputeQueryValidator : IComputeQueryValidator
{
    /// <summary>
    /// Validates a <see cref="ComputeQueryOption" />.
    /// </summary>
    /// <param name="computeQueryOption">The $compute query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(ComputeQueryOption computeQueryOption, ODataValidationSettings validationSettings)
    {
        if (computeQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(computeQueryOption));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        // Reject any property referenced by a $compute expression that the model marks as not
        // filterable or configures as not selectable, so those properties are enforced
        // consistently with $filter and $select. Developers can override this method to add
        // their own rules.
        ComputeClause computeClause = computeQueryOption.ComputeClause;
        if (computeClause != null)
        {
            foreach (ComputeExpression computeExpression in computeClause.ComputedItems)
            {
                QueryNodeRestrictionValidator.Validate(computeExpression.Expression, computeQueryOption.Context);
            }
        }
    }
}
