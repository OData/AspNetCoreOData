namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using System;
using System.Runtime.Serialization;

/// <summary>
/// FilteringPolicy abstract OData entity type.
/// </summary>
[DataContract(Name = "filteringPolicy")]
public class FilteringPolicy : Policy
{
    #region Properties

    /// <summary>
    /// The last date this policy was modified.
    /// </summary>
    [DataMember]
    public DateTimeOffset? LastModifiedDateTime { get; init; }

    /// <summary>
    /// The date this policy was created.
    /// </summary>
    [DataMember]
    public DateTimeOffset? CreatedDateTime { get; init; }

    #endregion

    #region Validations Methods

    protected override void ValidateEntityProperties(ILogger logger)
    {
        logger.LogDebug("Starting Filtering Policy base validations for entity creation.");

        base.ValidateEntityProperties(logger);

        if (LastModifiedDateTime is not null && CreatedDateTime is not null)
        {
            throw new Exception("A new Filtering Policy with a defined datetime cannot be created");
        }

        logger.LogDebug("Filtering Policy base entity creation validated successfully.");
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting Filtering Policy validations.");

        if (PolicyRules is not null)
        {
            foreach (PolicyRule rule in PolicyRules)
            {
                Type ruleType = rule.GetType();
                if (!ruleType.IsSubclassOf(typeof(FilteringRule)))
                {
                    throw new Exception($"A FilteringPolicy contains a rule that is not of type FilteringRule, the rule is of type: {ruleType}");
                }
            }
        }

        logger.LogDebug("Filtering Policy validated successfully.");
    }

    #endregion
}
