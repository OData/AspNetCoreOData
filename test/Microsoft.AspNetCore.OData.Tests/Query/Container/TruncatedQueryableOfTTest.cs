//-----------------------------------------------------------------------------
// <copyright file="TruncatedQueryableOfTTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query.Container;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Container;

public class TruncatedQueryableTest
{
    [Fact]
    public void Create_DoesNotEnumerate_AndSyncEnumerationUsesOneLookaheadItem()
    {
        // Arrange
        int itemsRead = 0;

        IEnumerable<int> GetItems()
        {
            for (int i = 1; i <= 4; i++)
            {
                itemsRead++;
                yield return i;
            }
        }

        // Act
        IQueryable query = TruncatedQueryable.Create(GetItems().AsQueryable(), pageSize: 2);

        // Assert
        Assert.Equal(0, itemsRead);
        Assert.Equal(new[] { 1, 2 }, ((IEnumerable<int>)query).ToArray());
        Assert.Equal(3, itemsRead);
        Assert.True(Assert.IsAssignableFrom<ITruncatedCollection>(query).IsTruncated);
    }

    [Fact]
    public async Task Create_PreservesAsyncEnumeration_AndUsesOneLookaheadItem()
    {
        // Arrange
        var source = new AsyncQueryable<int>(new[] { 1, 2, 3, 4 }.AsQueryable());

        // Act
        IQueryable query = TruncatedQueryable.Create(source, pageSize: 2);
        var results = new List<int>();
        await foreach (int item in Assert.IsAssignableFrom<IAsyncEnumerable<int>>(query))
        {
            results.Add(item);
        }

        // Assert
        Assert.Equal(new[] { 1, 2 }, results);
        Assert.Equal(3, source.ItemsRead);
        Assert.True(Assert.IsAssignableFrom<ITruncatedCollection>(query).IsTruncated);
    }

    private sealed class AsyncQueryable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly IQueryable<T> _source;

        public AsyncQueryable(IQueryable<T> source)
        {
            _source = source;
        }

        public int ItemsRead { get; private set; }

        public Type ElementType => typeof(T);

        public System.Linq.Expressions.Expression Expression => _source.Expression;

        public IQueryProvider Provider => _source.Provider;

        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            foreach (T item in _source)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ItemsRead++;
                yield return item;
                await Task.Yield();
            }
        }
    }
}
