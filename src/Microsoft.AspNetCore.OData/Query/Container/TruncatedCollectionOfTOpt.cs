using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData.Query.Container;

public class TruncatedCollectionOfTOpt<T> : List<T>, ITruncatedCollection, IReadOnlyList<T>, IEnumerable<T>, ICountOptionCollection
{
    // The default capacity of the list.
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs#L23
    private const int DefaultCapacity = 4;
    private const int MinPageSize = 1;

    private bool _isTruncated;
    private int _pageSize;
    private long? _totalCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    public TruncatedCollectionOfTOpt(IEnumerable<T> source, int pageSize)
        : base(checked(pageSize + 1))
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (pageSize < MinPageSize)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo("pageSize", pageSize, MinPageSize);
        }

        int count = 0;
        var items = source.Take(checked(pageSize + 1));
        using (var enumerator = items.GetEnumerator())
        {
            while (enumerator.MoveNext() && count < pageSize)
            {
                Add(enumerator.Current);
                count++;
            }

            // Check if there are more items beyond the page size
            _isTruncated = enumerator.MoveNext();
        }

        _pageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query whereas 
    // the enumerable version just enumerates and is inefficient.
    public TruncatedCollectionOfTOpt(IQueryable<T> source, int pageSize) : this(source, pageSize, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query whereas 
    // the enumerable version just enumerates and is inefficient.
    public TruncatedCollectionOfTOpt(IQueryable<T> source, int pageSize, bool parameterize)
        : base(pageSize)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (pageSize < MinPageSize)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo("pageSize", pageSize, MinPageSize);
        }

        int count = 0;
        var items = Take(source, checked(pageSize + 1), parameterize);
        using (var enumerator = items.GetEnumerator())
        {
            while (count < pageSize && enumerator.MoveNext())
            {
                Add(enumerator.Current);
                count++;
            }

            // Check if there are more items beyond the page size
            _isTruncated = enumerator.MoveNext();
        }

        _pageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    public TruncatedCollectionOfTOpt(IEnumerable<T> source, int pageSize, long? totalCount)
        : base(pageSize > 0
            ? checked(pageSize + 1)
            : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity))
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (pageSize > 0)
        {
            int count = 0;
            var items = source.Take(checked(pageSize + 1));
            using (var enumerator = items.GetEnumerator())
            {
                while (enumerator.MoveNext() && count < pageSize + 1)
                {
                    Add(enumerator.Current);
                    count++;
                }

                // Check if there are more items beyond the page size
                _isTruncated = enumerator.MoveNext();
            }

            _pageSize = pageSize;
        }
        else
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Add(enumerator.Current);
            }
        }

        _totalCount = totalCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query whereas 
    // the enumerable version just enumerates and is inefficient.
    [Obsolete("should not be used, will be marked internal in the next major version")]
    public TruncatedCollectionOfTOpt(IQueryable<T> source, int pageSize, long? totalCount) : this(source, pageSize,
        totalCount, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollectionOfTOpt{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query whereas 
    // the enumerable version just enumerates and is inefficient.
    [Obsolete("should not be used, will be marked internal in the next major version")]
    public TruncatedCollectionOfTOpt(IQueryable<T> source, int pageSize, long? totalCount, bool parameterize)
        : base(pageSize > 0 ? Take(source, pageSize, parameterize) : source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (pageSize > 0)
        {
            int count = 0;
            var items = Take(source, checked(pageSize + 1), parameterize);
            using (var enumerator = items.GetEnumerator())
            {
                while (count < pageSize && enumerator.MoveNext())
                {
                    Add(enumerator.Current);
                    count++;
                }

                // Check if there are more items beyond the page size
                _isTruncated = enumerator.MoveNext();
            }

            _pageSize = pageSize;
        }
        else
        {
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Add(enumerator.Current);
            }
        }

        _totalCount = totalCount;
    }

    private static IEnumerable<T> Take(IQueryable<T> source, int pageSize, bool parameterize)
    {
        if (source == null)
        {
            throw Error.ArgumentNull("source");
        }

        return ExpressionHelpers.Take(source, pageSize, typeof(T), parameterize) as IQueryable<T>;
    }

    /// <inheritdoc />
    public int PageSize
    {
        get { return _pageSize; }
    }

    /// <inheritdoc />
    public bool IsTruncated
    {
        get { return _isTruncated; }
    }

    /// <inheritdoc />
    public long? TotalCount
    {
        get { return _totalCount; }
    }
}
