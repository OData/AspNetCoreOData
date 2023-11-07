namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using System.Runtime.Serialization;

/// <summary>
/// WebCategory OData entity type.
/// </summary>
[DataContract(Name = "webCategory")]
public class WebCategory : RuleDestination
{
    #region Properties

    [RequiredForCreation]
    [DataMember(Name = "name")]
    public string Value { get; init; }

    [DataMember(Name = "displayName")]
    public string Name { get; set; }

    [DataMember]
    public string Group { get; set; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is WebCategory webCategory)
        {
            return Value.Equals(webCategory.Value);
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
        return NetworkDestinationType.WebCategory;
    }

    protected override void ValidateEntityProperties(ILogger logger)
    {
        if (Name is not null)
        {
            throw new System.Exception("Cannot create a Web Category Destination with an existing name.");
        }
        if (Group is not null)
        {
            throw new System.Exception("Cannot create a Web Category Destination with an existing group.");
        }
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
        // TODO: Task 2083214: Add validations for web categories
    }

    #endregion
}