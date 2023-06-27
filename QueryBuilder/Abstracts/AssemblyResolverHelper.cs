using Microsoft.OData.ModelBuilder;

namespace ODataQueryBuilder.Abstracts
{
    internal static class AssemblyResolverHelper
    {
        public static IAssemblyResolver Default = new DefaultAssemblyResolver();
    }
}
