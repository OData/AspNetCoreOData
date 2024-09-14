//-----------------------------------------------------------------------------
// <copyright file="SelectExpandValidatorContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// The metadata context for $select and $expand validator.
/// </summary>
public class SelectExpandValidatorContext : QueryValidatorContext
{
    /// <summary>
    /// The top level $select and $expand query option.
    /// </summary>
    public SelectExpandQueryOption SelectExpand { get; set; }

    /// <summary>
    /// The remaining depth on property.
    /// It's weird logic in current implementation. Need to improve it later.
    /// </summary>
    public int? RemainingDepth { get; set; } = null;

    /// <summary>
    /// Clone the context.
    /// </summary>
    /// <returns>The cloned context.</returns>
    public SelectExpandValidatorContext Clone()
    {
        return new SelectExpandValidatorContext
        {
            SelectExpand = this.SelectExpand,
            Context = this.Context,
            ValidationSettings = this.ValidationSettings,
            Property = this.Property,
            StructuredType = this.StructuredType,
            RemainingDepth = this.RemainingDepth,
            CurrentDepth = this.CurrentDepth
        };
    }
}
