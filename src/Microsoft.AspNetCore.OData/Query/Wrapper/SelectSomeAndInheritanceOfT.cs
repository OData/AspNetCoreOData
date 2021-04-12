// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class SelectSomeAndInheritance<TEntity> : SelectExpandWrapper<TEntity>
    {
    }

    internal class SelectSomeAndInheritanceConverter<TEntity> : JsonConverter<SelectSomeAndInheritance<TEntity>>
    {
        public override SelectSomeAndInheritance<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Contract.Assert(false, "SelectSomeAndInheritance{TEntity} is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, SelectSomeAndInheritance<TEntity> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToDictionary(SelectExpandWrapperConverter.MapperProvider), options);
        }
    }
}