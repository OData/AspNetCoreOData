namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.JsonDeserialization;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using Newtonsoft.Json;
using System.Runtime.Serialization;

/// <summary>
/// An abstract OData entity that describes a RuleDestination.
/// </summary>
[DataContract(Name = "ruleDestination")]
[JsonConverter(typeof(DestinationsJsonConverter))]
public abstract class RuleDestination : ValidatableBase
{
    #region Validations Methods

    public abstract NetworkDestinationType GetMatchingRuleType();

    protected override void ValidateEntityProperties(ILogger logger)
    {
    }

    #endregion
}