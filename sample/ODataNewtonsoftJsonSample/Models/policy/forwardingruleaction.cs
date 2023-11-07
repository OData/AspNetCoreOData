namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;

using System.Runtime.Serialization;

/// <summary>
/// Forwarding rule action.
/// </summary>
[DataContract(Name = "forwardingRuleAction")]
public enum ForwardingRuleAction
{
    /// <summary>
    /// Bypass.
    /// </summary>
    [EnumMember(Value = "bypass")]
    Bypass = 0,

    /// <summary>
    /// Tunnel traffic.
    /// </summary>
    [EnumMember(Value = "forward")]
    Forward = 1,

    /// <summary>
    /// A required property for an evolvable enum.
    /// </summary>
    [EnumMember(Value = "unknownFutureValue")]
    UnknownFutureValue = 2
}
