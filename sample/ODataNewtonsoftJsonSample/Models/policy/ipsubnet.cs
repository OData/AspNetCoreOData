namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Microsoft.Naas.Infra.Utilities.IpUtilities;
using System.Runtime.Serialization;

/// <summary>
/// IpSubnet OData entity type.
/// </summary>
[DataContract(Name = "ipSubnet")]
public class IpSubnet : RuleDestination
{
    #region Properties

    /// <summary>
    /// IP subnet value.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string Value { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is IpSubnet ipSubnet)
        {
            return Value.Equals(ipSubnet.Value);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Value is null ? 0 : Value.GetHashCode();
    }

    #endregion

    #region Validations Methods

    public override NetworkDestinationType GetMatchingRuleType()
    {
        return NetworkDestinationType.IpSubnet;
    }

    /// <summary>
    /// Validates the object contains a valid content based on an expected format.
    /// </summary>
    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting IP subnet validations.");

        if (!IpUtilities.IsValidIpv4SubnetAddress(Value))
        {
            throw new System.Exception("IP subnet address is invalid");
        }

        logger.LogDebug("IP subnet validated successfully.");
    }

    #endregion
}