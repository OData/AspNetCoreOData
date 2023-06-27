using ODataQueryBuilder.Query.Wrapper;
using ODataQueryBuilder;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ODataQueryBuilder.Query.Wrapper
{
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
}
