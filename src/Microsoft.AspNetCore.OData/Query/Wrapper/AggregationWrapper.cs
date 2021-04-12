// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    [JsonConverter(typeof(AggregationTypeWrapperConverter))]
    internal class AggregationWrapper : GroupByWrapper
    {
    }

    internal class AggregationWrapperConverter : JsonConverter<AggregationWrapper>
    {
        public override AggregationWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Contract.Assert(false, "AggregationWrapper is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, AggregationWrapper value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}