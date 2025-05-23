//-----------------------------------------------------------------------------
// <copyright file="FlatteningWrapperOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Query.Container;

namespace Microsoft.AspNetCore.OData.Query.Wrapper;

[JsonConverter(typeof(DynamicTypeWrapperConverter))]
internal class FlatteningWrapper<T> : GroupByWrapper, IGroupByWrapper<AggregationPropertyContainer, GroupByWrapper>, IFlatteningWrapper<T>
{
    public T Source { get; set; }
}

internal class FlatteningWrapperConverter<T> : JsonConverter<FlatteningWrapper<T>>
{
    public override FlatteningWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(FlatteningWrapper<>).Name));
    }

    public override void Write(Utf8JsonWriter writer, FlatteningWrapper<T> value, JsonSerializerOptions options)
    {
        if (value != null)
        {
            JsonSerializer.Serialize(writer, value.Values, options);
        }
    }
}
