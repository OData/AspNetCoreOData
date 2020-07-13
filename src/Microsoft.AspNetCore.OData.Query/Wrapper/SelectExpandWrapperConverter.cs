// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OData.Abstracts.Query;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Query.Wrapper
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandWrapper{TElement}"/> instances to JSON.
    /// </summary>
    internal class SelectExpandWrapperConverter : JsonConverter<ISelectExpandWrapper>
    {
        private static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider =
                (IEdmModel model, IEdmStructuredType type) => new JsonPropertyNameMapper(model, type);

        public override bool CanConvert(Type objectType)
        {
            if (objectType == null)
            {
                throw Error.ArgumentNull("objectType");
            }

            return objectType.IsAssignableFrom(typeof(ISelectExpandWrapper));
        }

        public override ISelectExpandWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Contract.Assert(false, "SelectExpandWrapper is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, [DisallowNull] ISelectExpandWrapper value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(value.ToDictionary(_mapperProvider));
        }

        
    }
}
