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

internal interface ITruncatedAsyncEnumerable
{
    bool IsTruncated { get; }
}

internal sealed class TruncatedAsyncEnumerable<T> : IAsyncEnumerable<T>, ITruncatedAsyncEnumerable
{
    private readonly IAsyncEnumerable<T> _source;
    private readonly int _pageSize;

    public TruncatedAsyncEnumerable(IAsyncEnumerable<T> source, int pageSize)
    {
        _source = source;
        _pageSize = pageSize;
    }

    public bool IsTruncated { get; private set; }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        int count = 0;
        await foreach (T item in _source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (count == _pageSize)
            {
                IsTruncated = true;
                yield break;
            }

            count++;
            yield return item;
        }
    }
}
