namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Linq;
using System;

/// <summary>
/// An OData entity that describes a forwarding policy.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.ForwardingPolicy)]
public class ForwardingPolicy : Policy
{
    #region Properties

    /// <summary>
    /// The type of the forwarding policy.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    [JsonConverter(typeof(StringEnumConverter))]
    public TrafficForwardingType? TrafficForwardingType { get; init; }

    #endregion

    #region Validations Methods

    [InternalProperty]
    public override ISet<string> AllowedPropertiesForUpdate
    {
        get
        {
            return base.AllowedPropertiesForUpdate.Concat(new HashSet<string> { nameof(PolicyRules) }).ToHashSet();
        }
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting Forwarding Policy validations.");

        if (PolicyRules is not null)
        {
            ValidateForwardingPolicyRules();
        }
        logger.LogDebug("Forwarding policy validated successfully.");
    }

    public override bool IsHiddenEntity()
    {
        if (TrafficForwardingType == Enums.TrafficForwardingType.Private)
        {
            return false;
        }
        return PolicyRules.All(rule => rule.IsHiddenEntity());
    }

    public override void ValidateEntityDelta(ILogger logger)
    {
        logger.LogDebug("Starting Forwarding Policy delta validations.");
        if (PolicyRules is not null)
        {
            foreach (var rule in PolicyRules)
            {
                rule.ValidateEntityCreation(logger);
                ValidateForwardingPolicyRules();
            }
        }
        base.ValidateEntityDelta(logger);
        logger.LogDebug("Forwarding policy validated delta successfully.");
    }

    private void ValidateForwardingPolicyRules()
    {
        HashSet<string> ruleAppIds = new HashSet<string>();
        foreach (PolicyRule rule in PolicyRules)
        {
            if (!rule.GetType().IsSubclassOf(typeof(ForwardingRule)))
            {
                throw new Exception($"A {typeof(ForwardingPolicy)} contains a {typeof(PolicyRule)} that is not of type: {typeof(ForwardingRule)}");
            }

            if (TrafficForwardingType == Enums.TrafficForwardingType.Private)
            {
                var privateAccessRule = rule as PrivateAccessForwardingRule;
                if (privateAccessRule == null)
                {
                    throw new Exception($"Rule {rule.Id} on private access forwarding policy {Id} is not a private access rule");
                }

                ruleAppIds.Add(privateAccessRule.AppId);
            }
        }

        if (ruleAppIds.Count > 1)
        {
            throw new Exception("Private access forwarding policy has rules linked to more than 1 appId");
        }
    }
    #endregion
}
