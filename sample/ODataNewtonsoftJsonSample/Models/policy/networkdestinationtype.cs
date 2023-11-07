namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;

using System.Runtime.Serialization;

/// <summary>
/// Rule type.
/// </summary>
[DataContract(Name = "networkDestinationType")]
public enum NetworkDestinationType
{
    /// <summary>
    /// URL type.
    /// </summary>
    [EnumMember(Value = "url")]
    Url = 0,

    /// <summary>
    /// FQDN type.
    /// </summary>
    [EnumMember(Value = "fqdn")]
    Fqdn = 1,

    /// <summary>
    /// IP address type.
    /// </summary>
    [EnumMember(Value = "ipAddress")]
    IpAddress = 2,

    /// <summary>
    /// IP range type.
    /// </summary>
    [EnumMember(Value = "ipRange")]
    IpRange = 3,

    /// <summary>
    /// IP subnet type.
    /// </summary>
    [EnumMember(Value = "ipSubnet")]
    IpSubnet = 4,

    /// <summary>
    /// IP subnet type.
    /// </summary>
    [EnumMember(Value = "webCategory")]
    WebCategory = 5,

    /// <summary>
    /// A required property for an evolvable enum.
    /// </summary>
    [EnumMember(Value = "unknownFutureValue")]
    UnknownFutureValue = 6
}
