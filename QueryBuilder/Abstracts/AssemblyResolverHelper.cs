using Microsoft.OData.ModelBuilder;

namespace ODataQueryBuilder.Abstracts
{
    public static class AssemblyResolverHelper
    {
        public static IAssemblyResolver Default = new DefaultAssemblyResolver();
    }
}
