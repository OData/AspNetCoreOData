//-----------------------------------------------------------------------------
// <copyright file="IGroupByWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Container;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    /// <summary>
    /// Represents the result of a $apply query operation.
    /// </summary>
    /// <typeparam name="TContainer">The type of the aggregation property container associated with this group.</typeparam>
    /// <typeparam name="TWrapper">The type of the group-by wrapper itself, enforcing recursive typing.</typeparam>
    public interface IGroupByWrapper<TContainer, TWrapper>
        where TContainer : IAggregationPropertyContainer<TWrapper, TContainer>
        where TWrapper : IGroupByWrapper<TContainer, TWrapper>
    {
        /// <summary>Gets or sets the property container that contains the grouping properties.</summary>
        TContainer GroupByContainer { get; set; }

        /// <summary>Gets or sets the property container that contains the aggregation properties.</summary>
        TContainer Container { get; set; }
    }
}
