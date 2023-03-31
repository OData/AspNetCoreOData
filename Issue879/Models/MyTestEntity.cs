using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Issue879.Models
{
    public class MyTestEntity
    {
        public int Id { get; set; }

        public IDictionary<string, object> MyProperties { get; set; }
    }

    public static class EdmModelBuilder
    {
        public static IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();

            // make sure the entity set name is same as the controller name, (case-sensitive, except you enable case-insensitive)
            odataBuilder.EntitySet<MyTestEntity>("MyTestEntities");
            return odataBuilder.GetEdmModel();
        }
    }
}
