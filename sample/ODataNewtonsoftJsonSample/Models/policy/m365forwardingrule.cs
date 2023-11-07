namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// M365ForwardingRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.M365ForwardingRule)]
public class M365ForwardingRule : ForwardingRule
{
    #region Properties

    /// <summary>
    /// forwarding protocol.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public NetworkingProtocol? Protocol { get; init; }

    /// <summary>
    /// Ports list.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public IList<string> Ports { get; init; }

    /// <summary>
    /// forwarding category.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public ForwardingCategory? Category { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is M365ForwardingRule rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Protocol, Ports, Category, base.GetHashCode());
    }

    protected bool Equals(M365ForwardingRule other)
    {
        if (!Category.Equals(other.Category) || !Protocol.Equals(other.Protocol) || Ports.Count != other.Ports.Count)
        {
            return false;
        }

        HashSet<string> otherPorts = other.Ports.ToHashSet();
        return Ports.All(port => otherPorts.Contains(port)) && otherPorts.All(port => Ports.Contains(port));
    }

    #endregion
}
