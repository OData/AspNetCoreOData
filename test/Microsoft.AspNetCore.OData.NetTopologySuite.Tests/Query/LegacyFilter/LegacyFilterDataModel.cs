using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.LegacyFilter
{
    public class Site
    {
        public int Id { get; set; }
        public GeometryPoint Location { get; set; }
    }
}
