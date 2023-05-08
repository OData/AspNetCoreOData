using Microsoft.Azure.WebJobs.Host.Bindings;
using System.Reflection;

namespace Microsoft.OData.AzureFunctions
{
    public class ODataBindingProvider : IBindingProvider
    {
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            ODataAttribute attribute = context.Parameter.GetCustomAttribute<ODataAttribute>(inherit: false);
            IBinding binding = CreateODataBinding(context.Parameter.ParameterType, attribute);
            return Task.FromResult(binding);
        }

        private IBinding CreateODataBinding(Type T, ODataAttribute attribute)
        {
            Type type = typeof(ODataBinding<>).MakeGenericType(T);
            var bindingInstance = Activator.CreateInstance(type, new object[] { attribute });
            return (IBinding)bindingInstance;
        }
    }
}
