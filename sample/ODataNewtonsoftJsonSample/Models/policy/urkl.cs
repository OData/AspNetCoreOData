namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Destinations;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;
using System;
using System.Runtime.Serialization;

/// <summary>
/// Url OData entity type.
/// </summary>
[DataContract(Name = "url")]
public class Url : RuleDestination
{
    #region Properties

    /// <summary>
    /// URL value.
    /// </summary>
    [DataMember]
    [RequiredForCreation]
    public string Value { get; init; }

    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is Url url)
        {
            return Value.Equals(url.Value);
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
        return NetworkDestinationType.Url;
    }

    protected override void ValidateEntityStructure(ILogger logger)
    {
        logger.LogDebug("Starting URL validations.");

        bool isUriValid = Uri.TryCreate(Value, UriKind.Absolute, out Uri createdUri) && (createdUri.Scheme == Uri.UriSchemeHttp || createdUri.Scheme == Uri.UriSchemeHttps);

        if (!isUriValid)
        {
            throw new Exception("Please enter a valid URL in the format \"http://www.example.com\" or \"https://www.example.com\".");
        }

        logger.LogDebug("URL validated successfully.");
    }

    #endregion
}