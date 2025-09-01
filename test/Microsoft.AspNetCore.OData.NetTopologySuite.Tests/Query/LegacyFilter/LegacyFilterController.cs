using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.LegacyFilter;

public class SitesController : ODataController
{
    [EnableQuery]
    public ActionResult<IEnumerable<Site>> Get()
    {
        return new List<Site>
        {
            new Site { Id = 1, Location = GeometryFactory.Point(CoordinateSystem.DefaultGeometry, 47.669444 , -122.123889) },
            new Site { Id = 2, Location = GeometryFactory.Point(CoordinateSystem.DefaultGeometry, 47.608013, -122.335167) }
        };
    }
}
