//-----------------------------------------------------------------------------
// <copyright file="OrderByValidatorContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// The metadata context for $orderby validator.
/// </summary>
public class OrderByValidatorContext : QueryValidatorContext
{
    private int _orderByNodeCount = 0;

    /// <summary>
    /// The top level $orderby query option.
    /// </summary>
    public OrderByQueryOption OrderBy { get; set; }

    /// <summary>
    /// Gets current orderby node count.
    /// </summary>
    public int OrderByNodeCount => _orderByNodeCount;

    /// <summary>
    /// Increment orderby node count.
    /// </summary>
    /// <exception cref="ODataException">Throw OData exception.</exception>
    public void IncrementNodeCount()
    {
        ++_orderByNodeCount;

        if (_orderByNodeCount > ValidationSettings.MaxOrderByNodeCount)
        {
            throw new ODataException(Error.Format(SRResources.OrderByNodeCountExceeded, ValidationSettings.MaxOrderByNodeCount));
        }
    }
}
