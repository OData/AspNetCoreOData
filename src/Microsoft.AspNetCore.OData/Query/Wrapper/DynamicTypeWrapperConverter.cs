// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="DynamicTypeWrapper"/> instances to JSON.
    /// </summary>
    internal class DynamicTypeWrapperConverter : JsonConverter<DynamicTypeWrapper>
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == null)
            {
                throw Error.ArgumentNull("objectType");
            }

            return objectType.IsAssignableFrom(typeof(DynamicTypeWrapper));
        }

        public override DynamicTypeWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Contract.Assert(false, "DynamicTypeWrapper is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, [DisallowNull] DynamicTypeWrapper value, JsonSerializerOptions options)
        {
            DynamicTypeWrapper dynamicTypeWrapper = value as DynamicTypeWrapper;
            if (dynamicTypeWrapper != null)
            {
                JsonSerializer.Serialize(dynamicTypeWrapper.Values);
            }
        }
    }
}
