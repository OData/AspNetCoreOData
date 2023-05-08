using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.OData.AzureFunctions;

[assembly: WebJobsStartup(typeof(WebJobsStartup))]
namespace Microsoft.OData.AzureFunctions
{
    public class WebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));

            builder.AddODataBindingProvider();
        }
    }
}
