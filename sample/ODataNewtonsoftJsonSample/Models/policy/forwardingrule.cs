namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// ForwardingRule abstract OData entity type.
/// </summary>
[DataContract(Name = "forwardingRule")]
public abstract class ForwardingRule : PolicyRule
{
    #region Properties

    /// <summary>
    /// Rule type.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public NetworkDestinationType? RuleType { get; init; }

    /// <summary>
    /// Rule action.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public ForwardingRuleAction? Action { get; set; }

    /// <summary>
    /// Destinations list.
    /// </summary>
    [DataMember]
    public IList<RuleDestination> Destinations { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is ForwardingRule rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RuleType, Destinations, base.GetHashCode());
    }

    protected bool Equals(ForwardingRule other)
    {
        if (!RuleType.Equals(other.RuleType) ||
            !Action.Equals(other.Action) ||
            Destinations.Count != other.Destinations.Count)
        {
            return false;
        }

        HashSet<RuleDestination> otherDestinations = other.Destinations.ToHashSet();
        return Destinations.All(destination => otherDestinations.Contains(destination)) && other.Destinations.All(destination => Destinations.ToHashSet().Contains(destination));
    }

    #endregion

    #region Validations Methods

    [InternalProperty]
    public override ISet<string> AllowedPropertiesForUpdate
    {
        get
        {
            return new HashSet<string>
            {
                nameof(Action)
            };
        }
    }

    public override void ValidateEntityCreation(ILogger logger)
    {
        base.ValidateEntityCreation(logger);

        foreach (RuleDestination destination in Destinations)
        {
            destination.ValidateEntityCreation(logger);
        }
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting Forwarding Rule validations.");

        if (RuleType is null)
        {
            throw new ArgumentNullException("NetworkDestinationType cannot be null");
        }

        foreach (RuleDestination destination in Destinations)
        {
            if (!destination.GetMatchingRuleType().Equals(RuleType))
            {
                throw new Exception($"A {typeof(ForwardingRule)} of type: {RuleType} contains a {typeof(RuleDestination)} that is not of the matching type ({destination.GetType()})");
            }
        }

        logger.LogDebug("Forwarding Rule validated successfully.");
    }

    #endregion
}