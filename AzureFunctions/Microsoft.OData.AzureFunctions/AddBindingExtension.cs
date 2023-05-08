using Microsoft.Azure.WebJobs;

namespace Microsoft.OData.AzureFunctions
{
    public static class AddBindingExtension
    {
        public static IWebJobsBuilder AddODataBindingProvider(this IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.AddExtension<ODataBindingProviderConfig>();
            return builder;
        }
    }
}
