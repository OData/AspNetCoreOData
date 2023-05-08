using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataBinding<T> : IBinding
    {
        private readonly ODataAttribute attribute;
        public ODataBinding(ODataAttribute attribute)
        {
            this.attribute = attribute;
        }
        bool IBinding.FromAttribute => true;

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            HttpRequest request = value as HttpRequest ?? throw new ArgumentException($"{nameof(value)} must be a {nameof(HttpRequest)}", nameof(value));
            IEdmModelProvider modelProvider = (IEdmModelProvider)Activator.CreateInstance(attribute.ModelProvider);

            return Task.FromResult<IValueProvider>(new ODataValueProvider<T>(request, modelProvider));
        }

        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            // Get the HTTP request
            if (context.BindingData["$request"] is not HttpRequest request)
            {
                throw new ArgumentException("Binding can only be used with HttpTrigger");
            }

            return BindAsync(request, context.ValueContext);
        }

        public ParameterDescriptor ToParameterDescriptor() => new ParameterDescriptor();
    }
}
