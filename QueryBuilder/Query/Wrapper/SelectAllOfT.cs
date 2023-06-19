using QueryBuilder.Query.Wrapper;
using QueryBuilder;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryBuilder.Query.Wrapper
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
