namespace Microsoft.Naas.Contracts.ControlPlane.JsonDeserialization;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;
using Microsoft.Naas.Infra.Odata.Deserialization;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Runtime.Serialization;
using System;

public class DestinationsJsonConverter : OdataBaseClassConverter<RuleDestination>
{
    public override object ReadJsonByType(JObject item, string odataType)
    {
        switch (odataType)
        {
            case OdataTypes.IpAddress:
                return DeserializeByConcreteType<IpAddress>(item);
            case OdataTypes.Fqdn:
                return DeserializeByConcreteType<Fqdn>(item);
            case OdataTypes.IpRange:
                return DeserializeByConcreteType<IpRange>(item);
            case OdataTypes.IpSubnet:
                return DeserializeByConcreteType<IpSubnet>(item);
            case OdataTypes.Url:
                return DeserializeByConcreteType<Url>(item);
            case OdataTypes.WebCategory:
                return DeserializeByConcreteType<WebCategory>(item);
            default:
                throw new Exception(string.Format(DeserializationErrorMessage, odataType));
        }
    }
}
