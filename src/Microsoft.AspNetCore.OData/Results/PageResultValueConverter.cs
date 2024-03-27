//-----------------------------------------------------------------------------
// <copyright file="PageResultValueConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.OData.Results
{
    internal class PageResultValueConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null || !typeToConvert.IsGenericType)
            {
                return false;
            }

            Type genericType = typeToConvert.GetGenericTypeDefinition();
            return genericType == typeof(PageResult<>);
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
            Type genericType = type.GetGenericTypeDefinition();
            Type entityType = type.GetGenericArguments()[0];

            if (genericType == typeof(PageResult<>))
            {
                return (JsonConverter)Activator.CreateInstance(typeof(PageResultConverter<>).MakeGenericType(new Type[] { entityType }));
            }

            return null;
        }
    }

    internal class PageResultConverter<TEntity> : JsonConverter<PageResult<TEntity>>
    {
        public override PageResult<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Contract.Assert(false, "PageResult{TEntity} should never be deserialized into");
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, PageResult<TEntity> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("items");
            JsonSerializer.Serialize(writer, value.Items, options);

            if (value.NextPageLink != null)
            {
                writer.WritePropertyName("nextpagelink");
                writer.WriteStringValue(value.NextPageLink.OriginalString);
            }

            if (value.Count != null)
            {
                writer.WritePropertyName("count");
                writer.WriteNumberValue(value.Count.Value);
            }

            writer.WriteEndObject();
        }
    }
}
