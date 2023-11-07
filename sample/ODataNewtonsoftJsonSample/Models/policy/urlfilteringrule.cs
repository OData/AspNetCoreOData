namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Extensions.Logging;
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
/// UrlFilteringRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.UrlFilteringRule)]
public class UrlFilteringRule : FilteringRule
{
    #region Properties

    /// <summary>
    /// URL Protocol type.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public ApplicationProtocol? Protocol { get; init; }

    /// <summary>
    /// Ports list.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public IList<string> Ports { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is UrlFilteringRule rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Protocol, Ports, base.GetHashCode());
    }

    protected bool Equals(UrlFilteringRule other)
    {
        if (!Protocol.Equals(other.Protocol) || Ports.Count != other.Ports.Count)
        {
            return false;
        }

        var otherPorts = other.Ports.ToHashSet();
        return Ports.All(port => otherPorts.Contains(port)) && otherPorts.All(port => Ports.Contains(port));
    }

    #endregion

    #region Validations Methods

    /// <summary>
    /// This value will change and eventually will be deleted, as more features are supported.
    /// </summary>
    private static List<NetworkDestinationType> supportedRuleTypes = new List<NetworkDestinationType>
    {
        Enums.NetworkDestinationType.Fqdn
    };

    protected override void ValidateEntityStructure(ILogger logger)
    {
        base.ValidateEntityStructure(logger);
        // TODO: add ports and protocoal valdation
    }

    #endregion
}