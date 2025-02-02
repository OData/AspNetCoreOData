//-----------------------------------------------------------------------------
// <copyright file="NoGroupByWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    internal class NoGroupByWrapper<T> : GroupByWrapper<T>
    {
    }

    internal class NoGroupByWrapperConverter<T> : JsonConverter<NoGroupByWrapper<T>>
    {
        public override NoGroupByWrapper<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(NoGroupByWrapper<T>).Name));
        }

        public override void Write(Utf8JsonWriter writer, NoGroupByWrapper<T> value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
