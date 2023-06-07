using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QueryBuilder.Query.Wrapper
{
    internal class AggregationWrapper : GroupByWrapper
    {
    }

    internal class AggregationWrapperConverter : JsonConverter<AggregationWrapper>
    {
        public override AggregationWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException(Error.Format(SRResources.JsonConverterDoesnotSupportRead, nameof(AggregationWrapper)));
        }

        public override void Write(Utf8JsonWriter writer, AggregationWrapper value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                JsonSerializer.Serialize(writer, value.Values, options);
            }
        }
    }
}
