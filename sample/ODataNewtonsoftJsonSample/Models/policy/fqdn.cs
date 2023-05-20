namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Microsoft.Naas.Infra.Utilities.FqdnUtilities;
using System.Runtime.Serialization;

/// <summary>
/// Fqdn OData entity type.
/// </summary>
[DataContract(Name = "fqdn")]
public class Fqdn : RuleDestination
{
    #region Properties

    /// <summary>
    /// FQDN value.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string Value { get; init; }

    [InternalProperty]
    public bool AllowSingleWordDomains { get; init; }
    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is Fqdn fqdn)
        {
            return Value.Equals(fqdn.Value);
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
        return NetworkDestinationType.Fqdn;
    }

    protected override void ValidateEntityProperties(ILogger logger)
    {
        logger.LogDebug("Fqdn::Starting FQDN properties validations");

        if (!FqdnUtilities.IsValidFqdn(Value, AllowSingleWordDomains))
        {
            logger.LogError("Fqdn::The specified FQDN is invalid");
            throw new System.Exception($"Invalid FQDN value: {Value}");
        }

        logger.LogDebug("Fqdn::FQDN properties validated successfully");
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
    }

    #endregion
}