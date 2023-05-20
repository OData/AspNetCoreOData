namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;

using System.Runtime.Serialization;

/// <summary>
/// Traffic forwarding type.
/// </summary>
[DataContract(Name = "trafficForwardingType")]
public enum TrafficForwardingType
{
    /// <summary>
    /// M365 type.
    /// </summary>
    [EnumMember(Value = "m365")]
    M365 = 0,

    /// <summary>
    /// Internet access type.
    /// </summary>
    [EnumMember(Value = "internet")]
    Internet = 1,

    /// <summary>
    /// Private access type.
    /// </summary>
    [EnumMember(Value = "private")]
    Private = 2,

    /// <summary>
    /// A required property for an evolvable enum.
    /// </summary>
    [EnumMember(Value = "unknownFutureValue")]
    UnknownFutureValue = 3
}
