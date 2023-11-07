namespace Microsoft.Naas.Contracts.ControlPlane.ApiConstants;

public static class OdataTypes
{
    public const string GraphTypePrefix = $"#{NetworkAccessCsdlConstants.NamespaceName}.";

    // concrete profiles types
    public const string ForwardingProfile = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.ForwardingProfile}";
    public const string FilteringProfile = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.FilteringProfile}";

    // concrete policy links types
    public const string ForwardingPolicyLink = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.ForwardingPolicyLink}";
    public const string FilteringPolicyLink = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.FilteringPolicyLink}";

    // concrete policy types
    public const string ForwardingPolicy = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.ForwardingPolicy}";
    public const string InternalForwardingPolicy = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.InternalForwardingPolicy}";
    public const string FilteringPolicy = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.FilteringPolicy}";

    // concrete rules types
    public const string M365ForwardingRule = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.M365ForwardingRule}";
    public const string M365ForwardingRuleExtended = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.M365ForwardingRuleExtended}";
    public const string PrivateAccessForwardingRule = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.PrivateAccessForwardingRule}";
    public const string InternetAccessForwardingRule = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.InternetAccessForwardingRule}";
    public const string UrlFilteringRule = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.UrlFilteringRule}";
    public const string FqdnFilteringRule = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.FqdnFilteringRule}";
    public const string WebCategoryFilteringRule = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.WebCategoryFilteringRule}";

    // concrete destinations types
    public const string IpAddress = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.IpAddress}";
    public const string Fqdn = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.Fqdn}";
    public const string IpRange = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.IpRange}";
    public const string IpSubnet = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.IpSubnet}";
    public const string Url = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.Url}";
    public const string WebCategory = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.WebCategory}";

    // concrete TunnelConfiguration types
    public const string TunnelConfigurationIKEv2Default = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.TunnelConfigurationIKEv2Default}";
    public const string TunnelConfigurationIKEv2Custom = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.TunnelConfigurationIKEv2Custom}";

    // concrete Association types
    public const string AssociatedBranch = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.AssociatedBranch}";
    public const string AssociatedDevice = $"{GraphTypePrefix}{NetworkAccessCsdlConstants.AssociatedDevice}";
}
