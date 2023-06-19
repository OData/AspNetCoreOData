using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryBuilder.Query.Wrapper
{
    internal class NoGroupByWrapper : GroupByWrapper
    {
    }

    internal class NoGroupByWrapperConverter : JsonConverter<NoGroupByWrapper>
    {
        public override NoGroupByWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, nameof(NoGroupByWrapper)));
        }

        public override void Write(Utf8JsonWriter writer, NoGroupByWrapper value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
