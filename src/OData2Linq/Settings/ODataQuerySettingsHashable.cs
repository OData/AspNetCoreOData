using Microsoft.AspNetCore.OData.Query;

namespace OData2Linq.Settings
{
    public class ODataQuerySettingsHashable : ODataQuerySettings
    {
        public override int GetHashCode()
        {
            var x = base.GetHashCode();
            return HashCode.Combine(HandleNullPropagation, PageSize, ModelBoundPageSize, EnsureStableOrdering, EnableConstantParameterization, TimeZone,
                HashCode.Combine(EnableCorrelatedSubqueryBuffering, IgnoredQueryOptions, IgnoredNestedQueryOptions, HandleReferenceNavigationPropertyExpandFilter));
        }
    }
}
