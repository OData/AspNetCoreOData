// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class FlatteningWrapper<T> : GroupByWrapper
    {
        // TODO: how to use 'Source'?
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
}