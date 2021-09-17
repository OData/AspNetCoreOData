//-----------------------------------------------------------------------------
// <copyright file="SelectExpandBinderContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Wrapper for properties used by the <see cref="ISelectExpandBinder"/>.
    /// </summary>
    public class SelectExpandBinderContext
    {
        /// <summary>
        /// Gets or sets the <see cref="ODataQuerySettings"/> that contains all the query application related settings.
        /// </summary>
        public ODataQuerySettings QuerySettings { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SelectExpandQueryOption"/> which contains the $select and $expand query options.
        /// </summary>
        public SelectExpandQueryOption SelectExpandQuery { get; set; }
    }
}
