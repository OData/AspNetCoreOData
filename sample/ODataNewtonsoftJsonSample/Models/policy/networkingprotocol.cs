namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;

using System.Runtime.Serialization;

/// <summary>
/// Network forwarding protocol.
/// </summary>
[DataContract(Name = "networkingProtocol")]
public enum NetworkingProtocol
{
    /// <summary>
    /// Internet Protocol.
    /// </summary>
    [EnumMember(Value = "ip")]
    Ip = 0,

    /// <summary>
    /// Internet Control Message Protocol.
    /// </summary>
    [EnumMember(Value = "icmp")]
    Icmp = 1,

    /// <summary>
    /// Internet Group Management Protocol.
    /// </summary>
    [EnumMember(Value = "igmp")]
    Igmp = 2,

    /// <summary>
    /// Gateway To Gateway Protocol.
    /// </summary>
    [EnumMember(Value = "ggp")]
    Ggp = 3,

    /// <summary>
    /// Internet Protocol version 4.
    /// </summary>
    [EnumMember(Value = "ipv4")]
    Ipv4 = 4,

    /// <summary>
    /// Transmission Control Protocol.
    /// </summary>
    [EnumMember(Value = "tcp")]
    Tcp = 6,

    /// <summary>
    /// PARC Universal Packet Protocol.
    /// </summary>
    [EnumMember(Value = "pup")]
    Pup = 12,

    /// <summary>
    /// User Datagram Protocol.
    /// </summary>
    [EnumMember(Value = "udp")]
    Udp = 17,

    /// <summary>
    /// Internet Datagram Protocol.
    /// </summary>
    [EnumMember(Value = "idp")]
    Idp = 22,

    /// <summary>
    /// Internet Protocol version 6 (ipv6).
    /// </summary>
    [EnumMember(Value = "ipv6")]
    Ipv6 = 41,

    /// <summary>
    /// ipv6 Routing header.
    /// </summary>
    [EnumMember(Value = "ipv6RoutingHeader")]
    Ipv6RoutingHeader = 43,

    /// <summary>
    /// ipv6 Fragment header.
    /// </summary>
    [EnumMember(Value = "ipv6FragmentHeader")]
    Ipv6FragmentHeader = 44,

    /// <summary>
    /// ipv6 Encapsulating Security Payload header.
    /// </summary>
    [EnumMember(Value = "ipSecEncapsulatingSecurityPayload")]
    IpSecEncapsulatingSecurityPayload = 50,

    /// <summary>
    /// ipv6 Authentication header.
    /// </summary>
    [EnumMember(Value = "ipSecAuthenticationHeader")]
    IpSecAuthenticationHeader = 51,

    /// <summary>
    /// Internet Control Message Protocol for ipv6.
    /// </summary>
    [EnumMember(Value = "icmpV6")]
    IcmpV6 = 58,

    /// <summary>
    /// ipv6 No next header.
    /// </summary>
    [EnumMember(Value = "ipv6NoNextHeader")]
    ipv6NoNextHeader = 59,

    /// <summary>
    /// ipv6 Destination Options header.
    /// </summary>
    [EnumMember(Value = "ipv6DestinationOptions")]
    ipv6DestinationOptions = 60,

    /// <summary>
    /// Net Disk Protocol (unofficial).
    /// </summary>
    [EnumMember(Value = "nd")]
    Nd = 77,

    /// <summary>
    /// Raw IP packet protocol.
    /// </summary>
    [EnumMember(Value = "Raw IP packet protocol")]
    Raw = 255,

    /// <summary>
    /// Internet Packet Exchange Protocol.
    /// </summary>
    [EnumMember(Value = "ipx")]
    Ipx = 1000,

    /// <summary>
    /// Sequenced Packet Exchange protocol.
    /// </summary>
    [EnumMember(Value = "spx")]
    Spx = 1256,

    /// <summary>
    /// Sequenced Packet Exchange version 2 protocol.
    /// </summary>
    [EnumMember(Value = "spxII")]
    SpxII = 1257,

    /// <summary>
    /// A required property for an evolvable enum.
    /// </summary>
    [EnumMember(Value = "unknownFutureValue")]
    UnknownFutureValue = 1258
}
