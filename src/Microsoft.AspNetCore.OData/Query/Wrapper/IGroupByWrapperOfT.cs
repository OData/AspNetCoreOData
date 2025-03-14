//-----------------------------------------------------------------------------
// <copyright file="IGroupByWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    /// <summary>
    /// Represents the result of a $apply query operation.
    /// </summary>
    public interface IGroupByWrapper<T>
    {
        /// <summary>Gets or sets the property container that contains the grouping properties.</summary>
        T GroupByContainer { get; set; }

        /// <summary>Gets or sets the property container that contains the aggregation properties.</summary>
        T Container { get; set; }
    }
}
