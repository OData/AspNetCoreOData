namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.JsonDeserialization;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Microsoft.OData.ModelBuilder;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

/// <summary>
/// An OData entity that describes a policy.
/// </summary>
[DataContract(Name = "policy")]
[JsonConverter(typeof(PolicyJsonConverter))]
public class Policy : ValidatableBase
{
    #region Properties

    /// <summary>
    /// The id of the policy.
    /// </summary>
    [Key]
    [DataMember]
    public string Id { get; init; }

    /// <summary>
    /// The name of the policy.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string Name { get; init; }

    /// <summary>
    /// A description of the policy.
    /// </summary>
    [DataMember]
    public string Description { get; init; }

    /// <summary>
    /// The version of the policy.
    /// </summary>
    [DataMember]
    public string Version { get; set; } = "1.0.0"; // Task 1888183: Handle CP entities versioning

    /// <summary>
    /// The rules associated to the policy.
    /// </summary>
    [Contained]
    [DataMember]
    public IList<PolicyRule> PolicyRules { get; set; }

    #endregion

    #region Validations Methods

    [InternalProperty]
    public override ISet<string> AllowedPropertiesForUpdate
    {
        get
        {
            return new HashSet<string>
            {
                nameof(Name),
                nameof(Description)
            };
        }
    }

    public override void ValidateEntityCreation(ILogger logger)
    {
        if (!IsBindedEntity(logger))
        {
            ValidateRequiredProperties(logger);
        }

        ValidateEntityProperties(logger);
        ValidateEntityStructure(logger);

        if (PolicyRules is not null)
        {
            foreach (PolicyRule policyRule in PolicyRules)
            {
                policyRule.ValidateEntityCreation(logger);
            }
        }
    }

    protected override void ValidateEntityProperties(ILogger logger)
    {
        logger.LogDebug("Starting Policy base validations for entity creation.");

        // check also "Version is not null" after Task 1888183 (Handle CP entities versioning) is done
        if (!IsBindedEntity(logger) && Id is not null)
        {
            throw new System.Exception("A new policy with a defined ID cannot be created");
        }

        logger.LogDebug("Policy base entity creation validated successfully.");
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
    }

    #endregion
}