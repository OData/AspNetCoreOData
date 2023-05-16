using Microsoft.OData.ModelBuilder.Config;

namespace QueryBuilder.Query
{
    /// <summary>
    /// This class describes the default configurations to use during query composition.
    /// </summary>
    public class DefaultQueryConfigurations : DefaultQuerySettings
    {
        // We will add other query settings, for example, $compute, $search here
        // In the next major release, we should remove the inheritance from 'DefaultQuerySettings'.
    }
}
