//-----------------------------------------------------------------------------
// <copyright file="EntitySetAggregationWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class EntitySetAggregationWrapper<T> : GroupByWrapper<T>
    {
    }

    internal class EntitySetAggregationWrapperConverter<T> : JsonConverter<EntitySetAggregationWrapper<T>>
    {
        public override EntitySetAggregationWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(EntitySetAggregationWrapper<T>).Name));
        }

        public override void Write(Utf8JsonWriter writer, EntitySetAggregationWrapper<T> value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
