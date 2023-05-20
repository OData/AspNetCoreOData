namespace Microsoft.Naas.Contracts.ControlPlane.JsonDeserialization;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;
using Microsoft.Naas.Infra.Odata.Deserialization;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;

public class RulesJsonConverter : OdataBaseClassConverter<PolicyRule>
{
    public override object ReadJsonByType(JObject item, string odataType)
    {
        switch (odataType)
        {
            case OdataTypes.M365ForwardingRule:
                return DeserializeByConcreteType<M365ForwardingRuleExtended>(item);
            case OdataTypes.M365ForwardingRuleExtended:
                return DeserializeByConcreteType<M365ForwardingRuleExtended>(item);
            case OdataTypes.PrivateAccessForwardingRule:
                return DeserializeByConcreteType<PrivateAccessForwardingRule>(item);
            case OdataTypes.InternetAccessForwardingRule:
                return DeserializeByConcreteType<InternetAccessForwardingRule>(item);
            case OdataTypes.UrlFilteringRule:
                return DeserializeByConcreteType<UrlFilteringRule>(item);
            case OdataTypes.FqdnFilteringRule:
                return DeserializeByConcreteType<FqdnFilteringRule>(item);
            case OdataTypes.WebCategoryFilteringRule:
                return DeserializeByConcreteType<WebCategoryFilteringRule>(item);
            default:
                throw new Exception(string.Format(DeserializationErrorMessage, odataType));
        }
    }
}