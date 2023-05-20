namespace Microsoft.Naas.Infra.Odata.Deserialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

public class ConcreteClassContractResolver<T> : DefaultContractResolver
    where T : class
{
    protected override JsonConverter? ResolveContractConverter(Type objectType)
    {
        if (typeof(T).IsAssignableFrom(objectType) && !objectType.IsAbstract)
        {
            return null;
        }

        return base.ResolveContractConverter(objectType);
    }
}