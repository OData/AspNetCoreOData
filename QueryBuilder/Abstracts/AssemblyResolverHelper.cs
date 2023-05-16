using Microsoft.OData.ModelBuilder;

namespace QueryBuilder.Abstracts
{
    internal static class AssemblyResolverHelper
    {
        public static IAssemblyResolver Default = new DefaultAssemblyResolver();
    }
}
