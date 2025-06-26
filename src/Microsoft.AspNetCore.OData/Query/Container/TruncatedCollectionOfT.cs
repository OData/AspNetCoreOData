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
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Query.Container;

/// <summary>
/// Represents a class that truncates a collection to a given page size.
/// </summary>
/// <typeparam name="T">The collection element type.</typeparam>
public class TruncatedCollection<T> : IReadOnlyList<T>, ITruncatedCollection, ICountOptionCollection, IAsyncEnumerable<T>
{
    private const int MinPageSize = 1;
    private const int DefaultCapacity = 4;

    private readonly List<T> _items;
    private readonly IAsyncEnumerable<T> _asyncSource;

    private readonly bool _isTruncated;

    private readonly TruncationState _isTruncatedState;

    /// <summary>
    /// Private constructor used by static Create methods and public constructors.
    /// </summary>
    /// <param name="items">The list of items in the collection.</param>
    /// <param name="pageSize">The maximum number of items per page.</param>
    /// <param name="totalCount">The total number of items in the source collection, if known.</param>
    /// <param name="isTruncated">Indicates whether the collection is truncated.</param>
    private TruncatedCollection(List<T> items, int pageSize, long? totalCount, bool isTruncated)
    {
        _items = items;
        _isTruncated = isTruncated;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    /// <summary>
    /// Private constructor used by static Create methods and public constructors.
    /// </summary>
    /// <param name="asyncSource">The asynchronous source of items (pageSize + 1) in the collection.</param>
    /// <param name="pageSize">The maximum number of items per page.</param>
    /// <param name="totalCount">The total number of items in the source collection, if known.</param>
    /// <param name="isTruncatedState">State to indicate whether the collection is truncated.</param>
    private TruncatedCollection(IAsyncEnumerable<T> asyncSource, int pageSize, long? totalCount, TruncationState isTruncatedState)
    {
        _asyncSource = asyncSource;
        _isTruncatedState = isTruncatedState;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    #region Constructors for Backward Compatibility

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    public TruncatedCollection(IEnumerable<T> source, int pageSize)
        : this(CreateInternal(source, pageSize, totalCount: null)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    public TruncatedCollection(IEnumerable<T> source, int pageSize, long? totalCount)
        : this(CreateInternal(source, pageSize, totalCount)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    public TruncatedCollection(IQueryable<T> source, int pageSize)
        : this(CreateInternal(source, pageSize, false)) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    public TruncatedCollection(IQueryable<T> source, int pageSize, bool parameterize)
        : this(CreateInternal(source, pageSize, parameterize)) { }

    /// <summary>
    /// Wrapper used internally by the backward-compatible constructors.
    /// </summary>
    /// <param name="other">An instance of <see cref="TruncatedCollection{T}"/>.</param>
    private TruncatedCollection(TruncatedCollection<T> other)
        : this(other._items, other.PageSize, other.TotalCount, other._isTruncated)
    {
    }

    #endregion

    #region Static Create Methods

    /// <summary>
    /// Create a truncated collection from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"></see></returns>
    public static TruncatedCollection<T> Create(IEnumerable<T> source, int pageSize)
    {
        return CreateInternal(source, pageSize, null);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"></see></returns>
    public static TruncatedCollection<T> Create(IEnumerable<T> source, int pageSize, long? totalCount)
    {
        return CreateInternal(source, pageSize, totalCount);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"></see></returns>
    public static TruncatedCollection<T> Create(IQueryable<T> source, int pageSize)
    {
        return CreateInternal(source, pageSize, false, null);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"></see></returns>
    public static TruncatedCollection<T> Create(IQueryable<T> source, int pageSize, long? totalCount)
    {
        return CreateInternal(source, pageSize, false, totalCount);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"></see></returns>
    public static TruncatedCollection<T> Create(IQueryable<T> source, int pageSize, bool parameterize)
    {
        return CreateInternal(source, pageSize, parameterize);
    }

    /// <summary>
    /// Create a truncated collection from an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count. Default is null.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"></see></returns>
    [Obsolete("should not be used, will be marked internal in the next major version")]
    public static TruncatedCollection<T> Create(IQueryable<T> source, int pageSize, long? totalCount, bool parameterize)
    {
        return CreateInternal(source, pageSize, parameterize, totalCount);
    }

    /// <summary>
    /// Create an async truncated collection from an <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <param name="source">The AsyncEnumerable to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// /// <param name="totalCount">The total count. Default null.</param>
    /// <param name="cancellationToken">Cancellation token for async operations. Default.</param>
    /// <returns>An instance of the <see cref="TruncatedCollection{T}"/></returns>
    public static TruncatedCollection<T> CreateForAsyncSource(IAsyncEnumerable<T> source, int pageSize, long? totalCount = null, CancellationToken cancellationToken = default)
    {
        return CreateInternal(source, pageSize, totalCount, cancellationToken);
    }

    #endregion

    #region Core Internal (Sync/Async)

    private static TruncatedCollection<T> CreateInternal(IEnumerable<T> source, int pageSize, long? totalCount)
    {
        ValidateArgs(source, pageSize);

        int capacity = pageSize > 0 ? checked(pageSize + 1) : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity);
        var items = source.Take(capacity);

        var smallPossibleCount = capacity < items.Count() ? items.Count() : capacity;
        var buffer = new List<T>(smallPossibleCount);
        buffer.AddRange(items);

        bool isTruncated = buffer.Count > pageSize;
        if (isTruncated)
        {
            buffer.RemoveAt(buffer.Count - 1);
        }

        return new TruncatedCollection<T>(buffer, pageSize, totalCount, isTruncated: isTruncated);
    }

    private static TruncatedCollection<T> CreateInternal(IQueryable<T> source, int pageSize, bool parameterize = false, long? totalCount = null)
    {
        ValidateArgs(source, pageSize);

        int capacity = pageSize > 0 ? pageSize : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity);
        var items = Take(source, capacity, parameterize);

        int count = 0;
        var buffer = new List<T>(pageSize);
        using IEnumerator<T> enumerator = items.GetEnumerator();
        while (count < pageSize && enumerator.MoveNext())
        {
            buffer.Add(enumerator.Current);
            count++;
        }

        return new TruncatedCollection<T>(buffer, pageSize, totalCount, isTruncated: enumerator.MoveNext());
    }

    private static TruncatedCollection<T> CreateInternal(IAsyncEnumerable<T> source, int pageSize, long? totalCount, CancellationToken cancellationToken = default)
    {
        ValidateArgs(source, pageSize);

        int capacity = pageSize > 0 ? pageSize : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity);

        var state = new TruncationState();
        var truncatedSource = new TruncatedAsyncEnumerable<T>(source, capacity, state);
        return new TruncatedCollection<T>(truncatedSource, pageSize, totalCount, state);
    }

    private static IQueryable<T> Take(IQueryable<T> source, int pageSize, bool parameterize)
    {
        // This uses existing ExpressionHelpers from OData to apply Take(pageSize + 1)
        return (IQueryable<T>)ExpressionHelpers.Take(source, checked(pageSize + 1), typeof(T), parameterize);
    }

    private static void ValidateArgs(object source, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (pageSize < MinPageSize)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo("pageSize", pageSize, MinPageSize);
        }
    }

    #endregion

    /// <inheritdoc/>
    public int PageSize { get; }
    /// <inheritdoc/>
    public long? TotalCount { get; }
    /// <inheritdoc/>
    public bool IsTruncated => _isTruncatedState?.IsTruncated ?? _isTruncated;

    /// <inheritdoc/>
    public int Count
    {
        get
        {
            if (_items != null)
            {
                return _items.Count;
            }
            else if (_asyncSource != null)
            {
                throw Error.InvalidOperation("Count cannot be accessed synchronously for an asynchronous source. Use CountAsync instead.");
            }

            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync()
    {
        if (_items != null)
        {
            return await Task.FromResult(_items.Count);
        }
        else if (_asyncSource != null)
        {
            return await _asyncSource.CountAsync().ConfigureAwait(false);
        }

        return 0;
    }

    /// <inheritdoc/>
    public T this[int index] => _items[index];

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _items?.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _items?.GetEnumerator();

    /// <inheritdoc/>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (_asyncSource == null)
        {
            throw new InvalidOperationException("Async enumeration is not supported for sync-only instances.");
        }
        return _asyncSource.GetAsyncEnumerator(cancellationToken);
    }
}
