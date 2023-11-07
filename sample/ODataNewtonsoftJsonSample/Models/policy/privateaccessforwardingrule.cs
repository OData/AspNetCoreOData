namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

/// <summary>
/// PrivateAccessForwardingRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.PrivateAccessForwardingRule)]
public class PrivateAccessForwardingRule : ForwardingRule
{
    #region Properties

    /// <summary>
    /// Private access application id.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string AppId { get; init; }

    /// <summary>
    /// forwarding protocol.
    /// </summary>
    [DataMember]
    [JsonConverter(typeof(StringEnumConverter))]
    public NetworkingProtocol Protocol { get; init; }

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
        if (obj is PrivateAccessForwardingRule rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AppId, Protocol, Ports, base.GetHashCode());
    }

    public override bool IsHiddenEntity()
    {
        return true;
    }

    protected bool Equals(PrivateAccessForwardingRule other)
    {
        if (!AppId.Equals(other.AppId) || !Protocol.Equals(other.Protocol))
        {
            return false;
        }

        HashSet<string> otherPorts = other.Ports.ToHashSet();
        return Ports.All(port => otherPorts.Contains(port)) && other.Ports.All(port => Ports.ToHashSet().Contains(port));
    }

    #endregion

    #region Validations
    private static List<NetworkingProtocol> _supportedProtocolTypes = new List<NetworkingProtocol>
    {
        NetworkingProtocol.Tcp
    };

    protected override void ValidateEntityStructure(ILogger logger)
    {

        if (!_supportedProtocolTypes.Contains(Protocol))
        {
            throw new Exception($"Protocol of type: {Protocol} is not supported for private access rule.");
        }

        base.ValidateEntityStructure(logger);
    }
    #endregion
}