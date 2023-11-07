namespace Microsoft.Naas.Contracts.ControlPlane.JsonDeserialization;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.InternalApiModels;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;
using Microsoft.Naas.Infra.Odata.Deserialization;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

public class PolicyJsonConverter : OdataBaseClassConverter<Policy>
{
    public override object ReadJsonByType(JObject item, string odataType)
    {
        switch (odataType)
        {
            case OdataTypes.ForwardingPolicy:
                return DeserializeByConcreteType<ForwardingPolicy>(item);
            case OdataTypes.InternalForwardingPolicy:
                return DeserializeByConcreteType<InternalForwardingPolicy>(item);
            case OdataTypes.FilteringPolicy:
                return DeserializeByConcreteType<FilteringPolicy>(item);
            default:
                throw new System.Exception(string.Format(DeserializationErrorMessage, odataType));
        }
    }
}
