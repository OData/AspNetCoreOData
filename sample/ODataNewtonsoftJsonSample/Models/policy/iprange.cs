namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Microsoft.Naas.Infra.Utilities.IpUtilities;
using System;
using System.Runtime.Serialization;

/// <summary>
/// IpRange OData entity type.
/// </summary>
[DataContract(Name = "ipRange")]
public class IpRange : RuleDestination
{
    #region Properties

    /// <summary>
    /// Begin IP address.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string BeginAddress { get; init; }

    /// <summary>
    /// End IP address.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string EndAddress { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is IpRange ipRange)
        {
            return BeginAddress.Equals(ipRange.BeginAddress) && EndAddress.Equals(ipRange.EndAddress);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BeginAddress, EndAddress);
    }

    #endregion

    #region Validations Methods

    public override NetworkDestinationType GetMatchingRuleType()
    {
        return NetworkDestinationType.IpRange;
    }

    /// <summary>
    /// Validates the object is in the correct structure.
    /// </summary>
    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting IP range validations.");

        if (!IpUtilities.IsValidIpv4Address(BeginAddress) ||
            !IpUtilities.IsValidIpv4Address(EndAddress))
        {
            throw new Exception("Begin or end IP address is invalid");
        }

        if (!IpUtilities.IsEndAddressBiggerOrEqualThanBeginAddress(BeginAddress, EndAddress))
        {
            throw new Exception("Invalid IP range: The end IP address is greater than or equal to the start IP address");
        }

        logger.LogDebug("IP range validated successfully.");
    }

    #endregion
}
