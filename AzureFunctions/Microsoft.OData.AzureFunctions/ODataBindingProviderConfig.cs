using Microsoft.Azure.WebJobs.Host.Config;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataBindingProviderConfig : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<ODataAttribute>().Bind(new ODataBindingProvider());
        }
    }
}
