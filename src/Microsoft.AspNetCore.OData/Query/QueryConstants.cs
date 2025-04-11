//-----------------------------------------------------------------------------
// <copyright file="QueryConstants.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Constant values used in aggregation operation.
    /// </summary>
    internal static class QueryConstants
    {
        /// <summary>Name for <see cref="IGroupByWrapper{TContainer, TWrapper}.Container"/> property.</summary>
        public const string GroupByWrapperContainerProperty = "Container";

        /// <summary>Name for <see cref="IGroupByWrapper{TContainer, TWrapper}.GroupByContainer"/> property.</summary>
        public const string GroupByWrapperGroupByContainerProperty = "GroupByContainer";

        /// <summary>Name for <see cref="IAggregationPropertyContainer{TWrapper, TContainer}.Name"/> property.</summary>
        public const string AggregationPropertyContainerNameProperty = "Name";

        /// <summary>Name for <see cref="IAggregationPropertyContainer{TWrapper, TContainer}.Value"/> property.</summary>
        public const string AggregationPropertyContainerValueProperty = "Value";

        /// <summary>Name for <see cref="IAggregationPropertyContainer{TWrapper, TContainer}.NestedValue"/> property.</summary>
        public const string AggregationPropertyContainerNestedValueProperty = "NestedValue";

        /// <summary>Name for <see cref="IAggregationPropertyContainer{TWrapper, TContainer}.Next"/> property.</summary>
        public const string AggregationPropertyContainerNextProperty = "Next";

        /// <summary>Name for <see cref="IFlatteningWrapper{T}.Source"/> property.</summary>
        public const string FlatteningWrapperSourceProperty = "Source";

        /// <summary>Name for <see cref="IComputeWrapper{T}.Instance"/> property.</summary>
        public const string ComputeWrapperInstanceProperty = "Instance";

        /// <summary>Name for <see cref="IComputeWrapper{T}.Model"/> property.</summary>
        public const string ComputeWrapperModelProperty = "Model";
    }
}
