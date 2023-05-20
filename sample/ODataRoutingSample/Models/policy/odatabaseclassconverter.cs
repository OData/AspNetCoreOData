namespace Microsoft.Naas.Infra.Odata.Deserialization;

using Microsoft.Naas.Infra.ErrorHandling.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public abstract class OdataBaseClassConverter<TBaseClass> : JsonConverter
    where TBaseClass : class
{
    private const string _deserializationErrorMessage = "Failed to deserialized odata object due to unexpected odataType: {0}.";
    private JsonSerializerSettings _specifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new ConcreteClassContractResolver<TBaseClass>() };

    protected string DeserializationErrorMessage => _deserializationErrorMessage;

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TBaseClass);
    }

    public abstract object ReadJsonByType(JObject item, string odataType);

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JObject item = JObject.Load(reader);
        string? type = Parse<string>(item, "@odata.type");
        if (type is not null)
        {
            return ReadJsonByType(item, type);
        }

        if (!objectType.IsAbstract)
        {
            return JsonConvert.DeserializeObject(item.ToString(), objectType, _specifiedSubclassConversion);
        }

        throw new DeserializationException(
            $"Failed to deserialize odata object." +
            $" The type property is missed and the {nameof(objectType)}: {objectType} is abstract.");
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException(); // won't be called because CanWrite returns false
    }

    protected static T? Parse<T>(JObject item, string propertyName)
    {
        if (item.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out JToken? token))
        {
            return token.ToObject<T>();
        }

        return default;
    }

    protected TConcreteType? DeserializeByConcreteType<TConcreteType>(JObject item)
        where TConcreteType : TBaseClass
    {
        return JsonConvert.DeserializeObject<TConcreteType>(item.ToString(), _specifiedSubclassConversion);
    }
}
