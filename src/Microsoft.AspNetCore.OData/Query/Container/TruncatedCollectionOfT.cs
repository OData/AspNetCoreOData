//-----------------------------------------------------------------------------
// <copyright file="TruncatedCollectionOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Query.Container
{
    /// <summary>
    /// Represents a class that truncates a collection to a given page size.
    /// </summary>
    /// <typeparam name="T">The collection element type.</typeparam>
    public class TruncatedCollection<T> : ITruncatedCollection, ICountOptionCollection, IQueryable<T>
    {
        private const int MinPageSize = 1;

        private bool? _isTruncated;
        private readonly IQueryable<T> _items;

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
        // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
        // the enumerable version just enumerates and is inefficient.
        public TruncatedCollection(IQueryable<T> source, int pageSize, bool parameterize): this(Take(source, pageSize, parameterize), pageSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
        /// </summary>
        /// <param name="source">The queryable collection to be truncated.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="totalCount">The total count.</param>
        public TruncatedCollection(IEnumerable<T> source, int pageSize, long? totalCount): this(pageSize > 0 ? source.Take(checked(pageSize + 1)).AsQueryable() : source.AsQueryable(), pageSize)
        {
	        TotalCount = totalCount;
        }

        private TruncatedCollection(IQueryable<T> source, int pageSize)
        {
	        _items = source;

	        if (pageSize < MinPageSize)
	        {
		        throw Error.ArgumentMustBeGreaterThanOrEqualTo("pageSize", pageSize, MinPageSize);
	        }

	        PageSize = pageSize;
        }

    


        private static IQueryable<T> Take(IQueryable<T> source, int pageSize, bool parameterize)
        {
            if (source == null)
            {
                throw Error.ArgumentNull("source");
            }

            return ExpressionHelpers.Take(source, checked(pageSize + 1), typeof(T), parameterize) as IQueryable<T>;
        }

        /// <inheritdoc />
        public int PageSize { get; }

        /// <inheritdoc />
        public bool IsTruncated => _isTruncated ?? throw new InvalidOperationException();

        /// <summary>
        /// Returns true if the underlying collection can be iterated asynchronously.
        /// </summary>
        public bool IsAsyncEnumerationPossible => _items is IAsyncEnumerable<T>;
        
        /// <summary>
        /// Returns an iterator which can be used to iterate the underlying collection asynchronously.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        IAsyncEnumerable<object> ITruncatedCollection.GetAsyncEnumerable()
        {
	        if (!(_items is IAsyncEnumerable<T> asyncEnumerable)) throw new InvalidOperationException();
	        return new AsyncEnumerableWrapper(asyncEnumerable, this);
	        
        }

        private class AsyncEnumerableWrapper : IAsyncEnumerable<object>
        {
	        private readonly IAsyncEnumerable<T> _Items;
	        private readonly TruncatedCollection<T> _Instance;

	        public AsyncEnumerableWrapper(IAsyncEnumerable<T> items, TruncatedCollection<T> instance)
	        {
		        _Items = items;
		        _Instance = instance;
	        }


	        public IAsyncEnumerator<object> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
	        {
		        return new AsyncTruncatedCollectionEnumerator(_Items, _Instance, cancellationToken); 
	        }
        }

        /// <inheritdoc />
        public long? TotalCount { get; }

        public IEnumerator<T> GetEnumerator()
        {
	        return new TruncatedCollectionEnumerator(_items, this);
        }

		/// <inheritdoc />
		public Type ElementType => _items.ElementType;
		/// <inheritdoc />
        public Expression Expression => _items.Expression;
		/// <inheritdoc />
        public IQueryProvider Provider => _items.Provider;

        private class TruncatedCollectionEnumerator : IEnumerator<T>
        {
	        private readonly IEnumerator<T> _items;
	        private readonly TruncatedCollection<T> _instance;
	        private int _remaining;

	        public TruncatedCollectionEnumerator(IEnumerable<T> items, TruncatedCollection<T> instance)
	        {
		        _items = items.GetEnumerator();
		        _remaining = instance.PageSize;
		        _instance = instance;
	        }

	        public bool MoveNext()
	        {
		        if (_remaining == 0)
		        {
			        _instance._isTruncated = _items.MoveNext();
			        return false;
		        }
		        _remaining--;
		        return _items.MoveNext();

	        }

	        public void Reset()
	        {
		        _remaining = _instance.PageSize;
		        _items.Reset();
	        }

	        public T Current => _items.Current;

	        object IEnumerator.Current => Current;

	        public void Dispose()
	        {
		        _items.Dispose();
	        }
        }
        
        private class AsyncTruncatedCollectionEnumerator : IAsyncEnumerator<object>
        {
	        private readonly IAsyncEnumerator<T> _items;
	        private readonly TruncatedCollection<T> _instance;
	        private int _remaining;
	        private bool _hasStatusBeenReported;

	        public AsyncTruncatedCollectionEnumerator(IAsyncEnumerable<T> items, TruncatedCollection<T> instance, CancellationToken cancellationToken)
	        {
		        _items = items.GetAsyncEnumerator(cancellationToken);
		        _remaining = instance.PageSize;
		        _instance = instance;
	        }

	        public ValueTask<bool> MoveNextAsync()
	        {
		        if (_remaining == 0)
		        {
			        return UpdateTruncatedListAsync();
		        }
		        _remaining--;
		        return _items.MoveNextAsync();
	        }

	        private async ValueTask<bool> UpdateTruncatedListAsync()
	        {
		        _instance._isTruncated = await _items.MoveNextAsync();
		        _hasStatusBeenReported = true;
		        return false;
	        }

	        public object Current => _items.Current;

	        public ValueTask DisposeAsync()
	        {
		        if (!_hasStatusBeenReported)
		        {
			        _instance._isTruncated = false;
		        }
		        return _items.DisposeAsync();
	        }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
	        return GetEnumerator();
        }
    }
}
