//-----------------------------------------------------------------------------
// <copyright file="SelectSomeOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper;

internal class SelectSome<TEntity> : SelectAllAndExpand<TEntity>
{
}

internal class SelectSomeConverter<TEntity> : JsonConverter<SelectSome<TEntity>>
{
    public override SelectSome<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(SelectSome<>).Name));
    }

    public override void Write(Utf8JsonWriter writer, SelectSome<TEntity> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.ToDictionary(SelectExpandWrapperConverter.MapperProvider), options);
    }
}
