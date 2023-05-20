namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.JsonDeserialization;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

/// <summary>
/// PolicyRule abstract OData entity type.
/// </summary>
[DataContract(Name = "policyRule")]
[JsonConverter(typeof(RulesJsonConverter))]
public abstract class PolicyRule : ValidatableBase
{
    #region Properties

    /// <summary>
    /// The id of the rule.
    /// </summary>
    [Key]
    [DataMember]
    public string Id { get; init; }

    /// <summary>
    /// The name of the rule.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string Name { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is PolicyRule rule)
        {
            return Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    protected bool Equals(PolicyRule other)
    {
        return Id.Equals(other.Id) && Name.Equals(other.Name);
    }

    #endregion

    #region Validations Methods

    protected override void ValidateEntityProperties(ILogger logger)
    {
        logger.LogDebug("Starting Policy Rule validations for entity creation.");

        if (Id is not null)
        {
            throw new Exception("A new rule with a defined ID cannot be created");
        }

        logger.LogDebug("Policy Rule entity creation validated successfully.");
    }

    #endregion
}