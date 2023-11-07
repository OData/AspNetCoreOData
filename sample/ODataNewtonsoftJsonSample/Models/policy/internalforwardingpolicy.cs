namespace Microsoft.Naas.Contracts.ControlPlane.InternalApiModels;

using Microsoft.Naas.Contracts.ControlPlane.ApiConstants;
using Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Policies;
using System;
using System.Runtime.Serialization;

[DataContract(Name = NetworkAccessCsdlConstants.InternalForwardingPolicy)]
public class InternalForwardingPolicy : ForwardingPolicy
{
    /// <summary>
    /// The last date this policy was modified.
    /// </summary>
    [DataMember]
    public DateTimeOffset? LastModifiedDateTime { get; init; }
}