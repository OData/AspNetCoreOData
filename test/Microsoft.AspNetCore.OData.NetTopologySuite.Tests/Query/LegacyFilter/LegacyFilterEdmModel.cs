using Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.LegacyFilter;

public class LegacyFilterEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        modelBuilder.EntitySet<Site>("Sites");

        var model = modelBuilder.GetEdmModel();

        return model;
    }
}
