namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using System.Runtime.Serialization;

/// <summary>
/// WebCategoryFilteringRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.WebCategoryFilteringRule)]
public class WebCategoryFilteringRule : FilteringRule
{
    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is WebCategoryFilteringRule rule)
        {
            return base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion
}