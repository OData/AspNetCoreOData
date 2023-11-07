namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;

using System.Runtime.Serialization;

/// <summary>
/// Application protocol type.
/// </summary>
[DataContract(Name = "applicationProtocol")]
public enum ApplicationProtocol
{
    /// <summary>
    /// Http protocol.
    /// </summary>
    [EnumMember(Value = "http")]
    Http = 0,

    /// <summary>
    /// Https protocol.
    /// </summary>
    [EnumMember(Value = "https")]
    Https = 1,

    /// <summary>
    /// SMB protocol.
    /// </summary>
    [EnumMember(Value = "smb")]
    Smb = 2,

    /// <summary>
    /// RDP protocol.
    /// </summary>
    [EnumMember(Value = "rdp")]
    Rdp = 3,

    /// <summary>
    /// FTP protocol.
    /// </summary>
    [EnumMember(Value = "ftp")]
    Ftp = 4,

    /// <summary>
    /// SSH protocol.
    /// </summary>
    [EnumMember(Value = "ssh")]
    Ssh = 5,

    /// <summary>
    /// SAP protocol.
    /// </summary>
    [EnumMember(Value = "sap")]
    Sap = 6,

    /// <summary>
    /// A required property for an evolvable enum.
    /// </summary>
    [EnumMember(Value = "unknownFutureValue")]
    UnknownFutureValue = 7
}