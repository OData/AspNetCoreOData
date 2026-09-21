//-----------------------------------------------------------------------------
// <copyright file="TruncatedQueryableOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.OData.Query.Container;

internal class TruncatedQueryable<T> : IOrderedQueryable<T>, ITruncatedCollection
{
    private readonly IQueryable<T> _source;

    protected TruncatedQueryable(IQueryable<T> source, int pageSize)
    {
        _source = source ?? throw Error.ArgumentNull(nameof(source));
        if (pageSize < 1)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(pageSize), pageSize, 1);
        }

        PageSize = pageSize;
    }

    public Type ElementType => typeof(T);

    public Expression Expression => _source.Expression;

    public IQueryProvider Provider => _source.Provider;

    public int PageSize { get; }

    public bool IsTruncated { get; protected set; }

    public IEnumerator<T> GetEnumerator()
    {
        IsTruncated = false;
        using IEnumerator<T> enumerator = _source.GetEnumerator();
        int count = 0;

        while (count < PageSize && enumerator.MoveNext())
        {
            yield return enumerator.Current;
            count++;
        }

        IsTruncated = count == PageSize && enumerator.MoveNext();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal static IQueryable<T> Create(IQueryable<T> source, int pageSize)
    {
        return source is IAsyncEnumerable<T> asyncSource
            ? new TruncatedAsyncQueryable<T>(source, asyncSource, pageSize)
            : new TruncatedQueryable<T>(source, pageSize);
    }
}

internal sealed class TruncatedAsyncQueryable<T> : TruncatedQueryable<T>, IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _source;

    public TruncatedAsyncQueryable(IQueryable<T> queryable, IAsyncEnumerable<T> source, int pageSize)
        : base(queryable, pageSize)
    {
        _source = source;
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        IsTruncated = false;
        await using IAsyncEnumerator<T> enumerator = _source.GetAsyncEnumerator(cancellationToken);
        int count = 0;

        while (count < PageSize && await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            yield return enumerator.Current;
            count++;
        }

        IsTruncated = count == PageSize &&
            await enumerator.MoveNextAsync().ConfigureAwait(false);
    }
}

internal static class TruncatedQueryable
{
    private static readonly MethodInfo _createGenericMethod = typeof(TruncatedQueryable)
        .GetMethod(nameof(CreateGeneric), BindingFlags.NonPublic | BindingFlags.Static);

    public static IQueryable Create(IQueryable source, int pageSize)
    {
        if (source == null)
        {
            throw Error.ArgumentNull(nameof(source));
        }

        MethodInfo createMethod = _createGenericMethod.MakeGenericMethod(source.ElementType);
        return (IQueryable)createMethod.Invoke(null, new object[] { source, pageSize });
    }

    private static IQueryable<T> CreateGeneric<T>(IQueryable source, int pageSize)
    {
        return TruncatedQueryable<T>.Create((IQueryable<T>)source, pageSize);
    }
}
