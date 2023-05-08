using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.OData.Edm;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataValueProvider<T> : IValueProvider
    {
        private readonly HttpRequest request;
        private readonly IEdmModelProvider modelProvider;
        private readonly string routePrefix;

        public ODataValueProvider(HttpRequest request, IEdmModelProvider modelProvider)
        {
            this.request = request;
            this.modelProvider = modelProvider;
            this.routePrefix = "api";
        }
        public Task<object> GetValueAsync()
        {
            // This is where we handle OData related logic using 
            // the IEdmModel, HttpRequest and clrType
            IEdmModel model = this.modelProvider.GetEdmModel();

            // TODO: Add routePrefix to constructor to be passed from the function
            Type clrType = this.Type.GenericTypeArguments.First();
            return Task.FromResult<object>(ODataBindingHelper.BuildODataQueryOptions(this.request, model, clrType, this.routePrefix));
        }

        public Type Type => typeof(T);
        public string ToInvokeString() => string.Empty;
    }
}
