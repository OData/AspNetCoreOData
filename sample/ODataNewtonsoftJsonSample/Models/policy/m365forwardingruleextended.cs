namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Rules;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// M365ForwardingRule OData entity type.
/// </summary>
[DataContract(Name = NetworkAccessCsdlConstants.M365ForwardingRuleExtended)]
public class M365ForwardingRuleExtended : M365ForwardingRule
{
    #region Properties

    /// <summary>
    /// The immutable ID number of the M365 endpoint set that this rule was created from.
    /// </summary>
    [DataMember]
    public int M365EndpointSetId { get; init; }

    #endregion

    #region Static

    /// <summary>
    /// M365 endpoints sets ids to be forwarded (tunneled) by default.
    /// </summary>
    public static readonly IReadOnlyList<int> M365EndpointSetIdsToBeForwarded = new List<int>() {
        1, 8, 9, 154, // EXO
        31, 32, 33, 35, 36, 37, 39, // SPO
        56, 97 // AAD
    };
    #endregion

    #region Object Override Methods

    public override bool Equals(object obj)
    {
        if (obj is M365ForwardingRuleExtended rule)
        {
            return Equals(rule) && base.Equals(rule);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), M365EndpointSetId);
    }

    protected bool Equals(M365ForwardingRuleExtended other)
    {
        return M365EndpointSetId == other.M365EndpointSetId;
    }

    public override bool IsHiddenEntity()
    {
        return !M365EndpointSetIdsToBeForwarded.Contains(M365EndpointSetId) || Protocol == NetworkingProtocol.Udp;
    }

    #endregion
}
