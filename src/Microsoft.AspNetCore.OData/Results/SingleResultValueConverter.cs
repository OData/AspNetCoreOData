// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Results
{
    internal class SingleResultValueConverter : JsonConverterFactory
    {
        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null || !typeToConvert.IsGenericType)
            {
                return false;
            }

            Type generaticType = typeToConvert.GetGenericTypeDefinition();
            return generaticType == typeof(SingleResult<>);
        }

        /// <summary>
        /// Creates a converter for a specified type.
        /// </summary>
        /// <param name="type">The type handled by the converter.</param>
        /// <param name="options">The serialization options to use.</param>
        /// <returns>A converter for which T is compatible with typeToConvert.</returns>
        public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
        {
            // Since 'type' is tested in 'CanConvert()', it must be a generic type
            Type generaticType = type.GetGenericTypeDefinition();
            Type entityType = type.GetGenericArguments()[0];

            if (generaticType == typeof(SingleResult<>))
            {
                return (JsonConverter)Activator.CreateInstance(typeof(SingleResultConverter<>).MakeGenericType(new Type[] { entityType }));
            }

            return null;
        }

        private class SingleResultConverter<TEntity> : JsonConverter<SingleResult<TEntity>>
        {
            public override SingleResult<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                Contract.Assert(false, "SingleResult{TEntity} should never be deserialized into");
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, SingleResult<TEntity> value, JsonSerializerOptions options)
            {
                if (value != null)
                {
                    var singleObject = value.Queryable.FirstOrDefault();
                    if (singleObject !=  null)
                    {
                        JsonSerializer.Serialize(writer, singleObject, options);
                    }
                }
            }
        }
    }
}