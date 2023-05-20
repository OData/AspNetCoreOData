namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Extensions.Logging;
using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// FqdnFilteringRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.FqdnFilteringRule)]
public class FqdnFilteringRule : FilteringRule
{
    #region Properties

    #endregion


    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is FqdnFilteringRule rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion

    #region Validations Methods

    /// <summary>
    /// This value will change and eventually will be deleted, as more features are supported.
    /// </summary>
    private static List<NetworkDestinationType> supportedRuleTypes = new List<NetworkDestinationType>
    {
        Enums.NetworkDestinationType.Fqdn
    };

    protected override void ValidateEntityStructure(ILogger logger)
    {
        base.ValidateEntityStructure(logger);
    }

    #endregion
}