namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// InternetAccessForwardingRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.InternetAccessForwardingRule)]
public class InternetAccessForwardingRule : ForwardingRule
{
    /// <summary>
    /// Ports list.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public IList<string> Ports { get; init; }

    /// <summary>
    /// forwarding protocol.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public NetworkingProtocol? Protocol { get; init; }

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is InternetAccessForwardingRule rule)
        {
            return base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion
}
