using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryBuilder.Query.Wrapper
{
    internal class NoGroupByAggregationWrapper : GroupByWrapper
    {
    }

    internal class NoGroupByAggregationWrapperConverter : JsonConverter<NoGroupByAggregationWrapper>
    {
        public override NoGroupByAggregationWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, nameof(NoGroupByAggregationWrapper)));
        }

        public override void Write(Utf8JsonWriter writer, NoGroupByAggregationWrapper value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
