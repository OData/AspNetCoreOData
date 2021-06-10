// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class SelectAllAndExpand<TEntity> : SelectExpandWrapper<TEntity>
    {
    }

    internal class SelectAllAndExpandConverter<TEntity> : JsonConverter<SelectAllAndExpand<TEntity>>
    {
        public override SelectAllAndExpand<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(SelectAllAndExpand<>).Name));
        }

        public override void Write(Utf8JsonWriter writer, SelectAllAndExpand<TEntity> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToDictionary(SelectExpandWrapperConverter.MapperProvider), options);
        }
    }
}