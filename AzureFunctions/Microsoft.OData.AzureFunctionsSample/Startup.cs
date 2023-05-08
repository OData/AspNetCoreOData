using Microsoft.AspNetCore.OData;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Microsoft.OData.AzureFunctionsSample.Startup))]

namespace Microsoft.OData.AzureFunctionsSample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMvcCore().AddOData();
        }
    }
}
