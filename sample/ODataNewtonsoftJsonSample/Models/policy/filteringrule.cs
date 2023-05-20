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
/// FilteringRule abstract OData entity type.
/// </summary>
[DataContract(Name = "filteringRule")]
public abstract class FilteringRule : PolicyRule
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
    /// Destinations list.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public IList<RuleDestination> Destinations { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is FilteringRule rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RuleType, Destinations, base.GetHashCode());
    }

    protected bool Equals(FilteringRule other)
    {
        if (!RuleType.Equals(other.RuleType) || Destinations.Count != other.Destinations.Count)
        {
            return false;
        }

        HashSet<RuleDestination> otherDestinations = other.Destinations.ToHashSet();
        return Destinations.All(destination => otherDestinations.Contains(destination)) && other.Destinations.All(destination => Destinations.Contains(destination));
    }

    #endregion

    #region Validations Methods

    /// <summary>
    /// This value will change and eventually will be deleted, as more features are supported.
    /// </summary>
    private static IList<NetworkDestinationType> _supportedRuleTypes = new List<NetworkDestinationType>
    {
        Enums.NetworkDestinationType.Fqdn,
        Enums.NetworkDestinationType.WebCategory,
        Enums.NetworkDestinationType.Url
    };

    [InternalProperty]
    public override ISet<string> AllowedPropertiesForUpdate
    {
        get
        {
            return new HashSet<string>
            {
                nameof(Destinations),
                nameof(Name)
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

    public override void ValidateEntityDelta(ILogger logger)
    {
        logger.LogDebug("FilteringRule::Starting Filtering Rule Delta validations.");

        if (Destinations is not null)
        {
            foreach (RuleDestination destination in Destinations)
            {
                destination.ValidateEntityCreation(logger);
            }
        }

        logger.LogDebug("FilteringRule::Filtering Rule Delta validated successfully.");
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting Filtering Rule validations.");

        if (RuleType is null)
        {
            throw new ArgumentNullException("NetworkDestinationType cannot be null");
        }

        // This eventually will be deleted, as more features are supported.
        if (!_supportedRuleTypes.Contains((NetworkDestinationType)RuleType))
        {
            throw new Exception($"The rule type: {RuleType} is not supported");
        }

        foreach (RuleDestination destination in Destinations)
        {
            if (!destination.GetMatchingRuleType().Equals(RuleType))
            {
                throw new Exception($"A FilteringRule of type: {RuleType} contains a RuleDestination that is not of the matching type ({destination.GetType()})");
            }
        }

        logger.LogDebug("Filtering Rule validated successfully.");
    }

    #endregion
}
