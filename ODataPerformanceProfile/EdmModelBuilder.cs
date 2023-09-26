using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataPerformanceProfile.Models;

namespace ODataPerformanceProfile
{
    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
          //  builder.EntitySet<Supplier>("Suppliers");
          //  builder.EntitySet<Order>("Orders");

            return builder.GetEdmModel();
        }
    }
}
