namespace Microsoft.Naas.Contracts.ControlPlane.ApiConstants;

public static class NetworkAccessCsdlConstants
{
    public const string ContainerName = "networkAccess";

    public const string NamespaceName = "microsoft.graph.networkaccess";

    #region Public
    public const string FilteringProfilesEntitySetName = "filteringProfiles";

    public const string FilteringPoliciesEntitySetName = "filteringPolicies";

    public const string ForwardingProfilesEntitySetName = "forwardingProfiles";

    public const string ForwardingPoliciesEntitySetName = "forwardingPolicies";

    // TODO: Task 1987983: Remove support for branches and tenantRestrictionsConfiguration old api paths after all clients move to the new version
    public const string TenantRestrictionsConfigurationSingletonName = "tenantRestrictionsConfiguration";

    public const string ConnectivityEntitySingletonName = "connectivity";

    public const string SettingsEntitySingletonName = "settings";

    public const string CrossTenantAccessSettingsEntitySetName = "crossTenantAccess";

    public const string ConditionalAccessSettingsEntitySetName = "conditionalAccess";

    public const string ForwardingOptionsEntitySetName = "forwardingOptions";

    public const string EnrichedAuditLogsEntitySetName = "enrichedAuditLogs";

    public const string BranchEntitySetName = "branches";

    public const string DeviceLinkEntitySetName = "deviceLinks";

    public const string OnboardActionName = "onboard";

    public const string OffboardActionName = "offboard";

    public const string TenantStatusSingletonName = "tenantStatus";

    // TODO: Task 2571167: Refactor UpdateForwardingPolicyById logic with OData bulk operations api when support will be available in the latest version of Microsoft.AspNetCore.OData (https://devblogs.microsoft.com/odata/bulk-operations-support-in-odata-web-api/)
    public const string UpdatePolicyRulesAction = "updatePolicyRules";
    #endregion

    #region Internal
    public const string InternalFilteringProfilesEntitySetName = "internalFilteringProfiles";

    public const string InternalFilteringPoliciesEntitySetName = "internalFilteringPolicies";

    public const string InternalForwardingProfilesEntitySetName = "internalForwardingProfiles";

    public const string InternalForwardingPoliciesEntitySetName = "internalForwardingPolicies";

    public const string InternalTenantRestrictionsConfigurationSingletonName = "internalTenantRestrictionsConfiguration";

    public const string InternalConditionalAccessSettingsEntitySetName = "internalConditionalAccess";

    public const string InternalForwardingOptionsEntitySetName = "internalForwardingOptions";

    public const string InternalBranchConfigurationSingletonName = "internalBranchConfiguration";

    public const string InternalDistributionUpdatesEntitySetName = "distributionUpdates";

    public const string InternalRedistributePoliciesEntitySetName = "redistribute";

    public const string InternalOnboardedTenantsEntitySetName = "onboardedTenants";

    public const string InternalForwardingPolicy = "internalForwardingPolicy";

    #endregion

    #region Private

    // profiles types
    public const string ForwardingProfile = "forwardingProfile";
    public const string FilteringProfile = "filteringPolicyProfile";

    // policy links types
    public const string ForwardingPolicyLink = "forwardingPolicyLink";
    public const string FilteringPolicyLink = "filteringPolicyLink";

    // policy types
    public const string ForwardingPolicy = "forwardingPolicy";
    public const string FilteringPolicy = "filteringPolicy";

    // rules types
    public const string M365ForwardingRule = "m365ForwardingRule";
    public const string M365ForwardingRuleExtended = "m365ForwardingRuleExtended";
    public const string PrivateAccessForwardingRule = "privateAccessForwardingRule";
    public const string InternetAccessForwardingRule = "internetAccessForwardingRule";
    public const string UrlFilteringRule = "urlFilteringRule";
    public const string FqdnFilteringRule = "fqdnFilteringRule";
    public const string WebCategoryFilteringRule = "webCategoryFilteringRule";

    // destinations types
    public const string IpAddress = "ipAddress";
    public const string Fqdn = "fqdn";
    public const string IpRange = "ipRange";
    public const string IpSubnet = "ipSubnet";
    public const string Url = "url";
    public const string WebCategory = "webCategory";

    // TunnelConfiguration types
    public const string TunnelConfigurationIKEv2Default = "tunnelConfigurationIKEv2Default";
    public const string TunnelConfigurationIKEv2Custom = "tunnelConfigurationIKEv2Custom";

    // Association types
    public const string AssociatedBranch = "associatedBranch";
    public const string AssociatedDevice = "associatedDevice";

    #endregion
}
