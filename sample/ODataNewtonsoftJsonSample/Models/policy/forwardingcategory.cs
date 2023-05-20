namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;

using System.Runtime.Serialization;

/// <summary>
/// Traffic forwarding category.
/// </summary>
[DataContract(Name = "forwardingCategory")]
public enum ForwardingCategory
{
    /// <summary>
    /// Default.
    /// </summary>
    [EnumMember(Value = "default")]
    Default = 0,

    /// <summary>
    /// Optimized.
    /// </summary>
    [EnumMember(Value = "optimized")]
    Optimized = 1,

    /// <summary>
    /// Allow.
    /// </summary>
    [EnumMember(Value = "allow")]
    Allow = 2,

    /// <summary>
    /// A required property for an evolvable enum.
    /// </summary>
    [EnumMember(Value = "unknownFutureValue")]
    UnknownFutureValue = 3
}
