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

internal sealed class TruncatedAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source;
    private readonly int _pageSize;
    private readonly TruncationState _state;

    public TruncatedAsyncEnumerable(IAsyncEnumerable<T> source, int pageSize, TruncationState state)
    {
        _source = source;
        _pageSize = pageSize;
        _state = state;
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        int count = 0;
        await foreach (T item in _source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (count == _pageSize)
            {
                _state.IsTruncated = true;
                yield break;
            }

            yield return item;
            count++;
        }

        _state.IsTruncated = false;
    }
}

internal sealed class TruncationState
{
    public bool IsTruncated { get; set; }
}
