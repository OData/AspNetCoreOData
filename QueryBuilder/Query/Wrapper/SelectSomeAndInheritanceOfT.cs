using QueryBuilder.Query.Wrapper;
using QueryBuilder;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryBuilder.Query.Wrapper
{
    internal class SelectSomeAndInheritance<TEntity> : SelectExpandWrapper<TEntity>
    {
    }

    internal class SelectSomeAndInheritanceConverter<TEntity> : JsonConverter<SelectSomeAndInheritance<TEntity>>
    {
        public override SelectSomeAndInheritance<TEntity> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, typeof(SelectSomeAndInheritance<>).Name));
        }

        public override void Write(Utf8JsonWriter writer, SelectSomeAndInheritance<TEntity> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.ToDictionary(SelectExpandWrapperConverter.MapperProvider), options);
        }
    }
}
