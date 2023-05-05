//-----------------------------------------------------------------------------
// <copyright file="ITruncatedCollection.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.OData.Query.Container
{
    /// <summary>
    /// Represents a collection that is truncated to a given page size.
    /// </summary>
    [SuppressMessage("Design", "CA1010:Collections should implement generic interface", Justification = "<Pending>")]
    public interface ITruncatedCollection : IEnumerable
    {
        /// <summary>
        /// Gets the page size the collection is truncated to.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets a value representing if the collection is truncated or not.
        /// </summary>
        bool IsTruncated { get; }

        bool IsAsyncEnumerationPossible { get; }
        IAsyncEnumerable<object> GetAsyncEnumerable();
    }
}
