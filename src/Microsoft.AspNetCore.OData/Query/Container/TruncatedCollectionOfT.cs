//-----------------------------------------------------------------------------
// <copyright file="TruncatedCollectionOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Microsoft.AspNetCore.OData.Query.Container;

// Add the jsonconverter for this?
internal class TruncatedCollectionValueConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == null || !typeToConvert.IsGenericType)
        {
            return false;
        }

        Type genericType = typeToConvert.GetGenericTypeDefinition();
        return genericType == typeof(TruncatedCollection<>);
    }

    /// <summary>
    /// Creates a converter for a specified type.
    /// </summary>
    /// <param name="type">The type handled by the converter.</param>
    /// <param name="options">The serialization options to use.</param>
    /// <returns>A converter for which T is compatible with typeToConvert.</returns>
    public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
    {
        // Since 'type' is tested in 'CanConvert()', it must be a generic type
        Type genericType = type.GetGenericTypeDefinition();
        Type entityType = type.GetGenericArguments()[0];

        if (genericType == typeof(TruncatedCollection<>))
        {
            return (JsonConverter)Activator.CreateInstance(typeof(TruncatedCollectionConverter<>).MakeGenericType(new Type[] { entityType }));
        }

        return null;
    }
}

internal class TruncatedCollectionConverter<T> : JsonConverter<TruncatedCollection<T>>
{
    public override TruncatedCollection<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Contract.Assert(false, "SingleResult{TEntity} should never be deserialized into");
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, TruncatedCollection<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        {
            writer.WritePropertyName("count");
            writer.WriteNumberValue(value.TotalCount.Value);
        }

        writer.WritePropertyName("items");
        JsonSerializer.Serialize(writer, value, options);

        writer.WriteEndObject();
    }
}

public class Truncated<T>
{
    private IEnumerable<T> sources;
    private int TotalCount;

    public Truncated(IEnumerable<T> source, int count)
    {
        this.sources = source;
        this.TotalCount = TotalCount;
    }
}


/// <summary>
/// Represents a class that truncates a collection to a given page size.
/// </summary>
/// <typeparam name="T">The collection element type.</typeparam>
[JsonConverter(typeof(TruncatedCollectionValueConverter))]
public class TruncatedCollection<T> : List<T>, ITruncatedCollection, IEnumerable<T>, ICountOptionCollection
{
    // The default capacity of the list.
    // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/List.cs#L23
    private const int DefaultCapacity = 4;
    private const int MinPageSize = 1;

    private bool _isTruncated;
    private int _pageSize;
    private long? _totalCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    public TruncatedCollection(IEnumerable<T> source, int pageSize)
        : base(checked(pageSize + 1))
    {
        var items = source.Take(Capacity);
        AddRange(items);
        Initialize(pageSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
    // the enumerable version just enumerates and is inefficient.
    public TruncatedCollection(IQueryable<T> source, int pageSize) : this(source, pageSize, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
    // the enumerable version just enumerates and is inefficient.
    public TruncatedCollection(IQueryable<T> source, int pageSize, bool parameterize)
        : base(checked(pageSize + 1))
    {
        var items = Take(source, pageSize, parameterize);
        AddRange(items);
        Initialize(pageSize);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    public TruncatedCollection(IEnumerable<T> source, int pageSize, long? totalCount)
        : base(pageSize > 0
            ? checked(pageSize + 1)
            : (totalCount > 0 ? (totalCount < int.MaxValue ? (int)totalCount : int.MaxValue) : DefaultCapacity))
    {
        if (pageSize > 0)
        {
            AddRange(source.Take(Capacity));
        }
        else
        {
            AddRange(source);
        }

        if (pageSize > 0)
        {
            Initialize(pageSize);
        }

        _totalCount = totalCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
    // the enumerable version just enumerates and is inefficient.
    [Obsolete("should not be used, will be marked internal in the next major version")]
    public TruncatedCollection(IQueryable<T> source, int pageSize, long? totalCount) : this(source, pageSize,
        totalCount, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TruncatedCollection{T}"/> class.
    /// </summary>
    /// <param name="source">The queryable collection to be truncated.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalCount">The total count.</param>
    /// <param name="parameterize">Flag indicating whether constants should be parameterized</param>
    // NOTE: The queryable version calls Queryable.Take which actually gets translated to the backend query where as 
    // the enumerable version just enumerates and is inefficient.
    [Obsolete("should not be used, will be marked internal in the next major version")]
    public TruncatedCollection(IQueryable<T> source, int pageSize, long? totalCount, bool parameterize)
        : base(pageSize > 0 ? Take(source, pageSize, parameterize) : source)
    {
        if (pageSize > 0)
        {
            Initialize(pageSize);
        }

        _totalCount = totalCount;
    }

    private void Initialize(int pageSize)
    {
        if (pageSize < MinPageSize)
        {
            throw Error.ArgumentMustBeGreaterThanOrEqualTo("pageSize", pageSize, MinPageSize);
        }

        _pageSize = pageSize;

        if (Count > pageSize)
        {
            _isTruncated = true;
            RemoveAt(Count - 1);
        }
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
