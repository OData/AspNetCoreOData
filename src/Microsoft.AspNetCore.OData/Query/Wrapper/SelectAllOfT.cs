//-----------------------------------------------------------------------------
// <copyright file="SelectAllOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class SelectAll<TEntity> : SelectExpandWrapper<TEntity>
    {
    }

    internal class SelectAllConverter<TEntity> : JsonConverter<SelectAll<TEntity>>
    {
        public override SelectAll<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(SelectAll<>).Name));
        }

        public override void Write(Utf8JsonWriter writer, SelectAll<TEntity> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToDictionary(SelectExpandWrapperConverter.MapperProvider), options);
        }
    }
}
