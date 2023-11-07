namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Microsoft.Naas.Infra.Utilities.IpUtilities;
using System.Runtime.Serialization;

/// <summary>
/// IpAddress OData entity type.
/// </summary>
[DataContract(Name = "ipAddress")]
public class IpAddress : RuleDestination
{
    #region Properties

    /// <summary>
    /// IP address value.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string Value { get; init; }

    public override NetworkDestinationType GetMatchingRuleType()
    {
        return NetworkDestinationType.IpAddress;
    }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is IpAddress ipAddress)
        {
            return Value.Equals(ipAddress.Value);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Value is null ? 0 : Value.GetHashCode();
    }

    #endregion

    #region Validations Methods

    /// <summary>
    /// Validates the object is in the correct structure.
    /// </summary>
    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting IP address validations.");

        // TODO: Task 1971663: Add wildcard validations to the right destinations
        if (!IpUtilities.IsValidIpv4Address(Value))
        {
            throw new System.Exception($"Invaid IP address: {Value}");
        }

        logger.LogDebug("IP address validated successfully.");
    }

    #endregion
}