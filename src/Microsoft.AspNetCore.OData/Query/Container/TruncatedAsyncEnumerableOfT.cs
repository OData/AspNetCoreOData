//-----------------------------------------------------------------------------
// <copyright file="TruncatedAsyncEnumerableOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Query.Container;

public class TruncatedAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source;
    private readonly int _pageSize;
    private readonly TruncationState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedAsyncEnumerable{T}"/> class, which provides an
    /// asynchronous enumerable that limits the number of items returned per page and tracks truncation state.
    /// </summary>
    /// <param name="source">The source asynchronous enumerable to be paginated and truncated.</param>
    /// <param name="pageSize">The maximum number of items to include in each page. Must be greater than zero.</param>
    /// <param name="state">The truncation state object used to track whether the enumeration was truncated.</param>
    public TruncatedAsyncEnumerable(IAsyncEnumerable<T> source, int pageSize, TruncationState state)
    {
        _source = source;
        _pageSize = pageSize;
        _state = state;
    }

    /// <summary>
    /// Returns an asynchronous enumerator that iterates through the items in the source collection,  up to a specified page size.
    /// </summary>
    /// <remarks>The enumerator yields items from the source collection until the specified page size is reached.  
    /// If the number of items exceeds the page size, the enumeration is truncated, and the state is updated to true. Otherwise, the state is updated to false.</remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. If the token is canceled, the enumeration is stopped.</param>
    /// <returns>An asynchronous enumerator that yields items from the source collection, up to the specified page size.</returns>
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        int count = 0;
        await foreach (var item in _source.WithCancellation(cancellationToken))
        {
            if (count < _pageSize)
            {
                yield return item;
                count++;
            }
            else
            {
                // More items exist than pageSize, so mark as truncated and stop yielding.
                _state.IsTruncated = true;
                yield break;
            }
        }

        // If we didn't hit the limit, not truncated.
        _state.IsTruncated = false;
    }
}


/// <summary>
/// Used to track the truncation state of an async enumerable.
/// </summary>
public class TruncationState
{
    public bool IsTruncated { get; set; }
}
