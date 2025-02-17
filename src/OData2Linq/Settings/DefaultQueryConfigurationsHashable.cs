using Microsoft.AspNetCore.OData.Query;

namespace OData2Linq.Settings
{
    public class DefaultQueryConfigurationsHashable : DefaultQueryConfigurations
    {
        public override int GetHashCode()
        {
            return HashCode.Combine(EnableExpand, EnableSelect, EnableCount, EnableOrderBy, EnableFilter, MaxTop, EnableSkipToken);
        }
    }
}
