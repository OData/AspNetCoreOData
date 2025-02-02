//-----------------------------------------------------------------------------
// <copyright file="AggregationWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class AggregationWrapper<T> : GroupByWrapper<T>
    {
    }

    internal class AggregationWrapperConverter<T> : JsonConverter<AggregationWrapper<T>>
    {
        public override AggregationWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(AggregationWrapper<T>).Name));
        }

        public override void Write(Utf8JsonWriter writer, AggregationWrapper<T> value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
