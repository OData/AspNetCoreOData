//-----------------------------------------------------------------------------
// <copyright file="NoGroupByAggregationWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class NoGroupByAggregationWrapper<T> : GroupByWrapper<T>
    {
    }

    internal class NoGroupByAggregationWrapperConverter<T> : JsonConverter<NoGroupByAggregationWrapper<T>>
    {
        public override NoGroupByAggregationWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(NoGroupByAggregationWrapper<T>).Name));
        }

        public override void Write(Utf8JsonWriter writer, NoGroupByAggregationWrapper<T> value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
