//-----------------------------------------------------------------------------
// <copyright file="GeographySerializationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geography;

public class SitesController : ODataController
{
    private readonly static GeometryFactory geographyFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private readonly static List<Site> sites = Generate();

    // GET /Sites
    [EnableQuery]
    public ActionResult<IEnumerable<Site>> Get()
    {
        return sites;
    }

    // GET /Sites(1)
    [EnableQuery]
    public ActionResult<Site> Get(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null)
        {
            return NotFound();
        }

        return site;
    }

    // GET /Sites(1)/Marker
    [EnableQuery]
    public ActionResult<Point> GetMarker(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Marker == null)
        {
            return NotFound();
        }

        return site.Marker;
    }

    // GET /Sites(1)/Route
    [EnableQuery]
    public ActionResult<LineString> GetRoute(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Route == null)
        {
            return NotFound();
        }

        return site.Route;
    }

    // GET /Sites(1)/Park
    [EnableQuery]
    public ActionResult<Polygon> GetPark(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Park == null)
        {
            return NotFound();
        }

        return site.Park;
    }

    // GET /Sites(1)/Markers
    [EnableQuery]
    public ActionResult<MultiPoint> GetMarkers(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Markers == null)
        {
            return NotFound();
        }

        return site.Markers;
    }

    // GET /Sites(1)/Routes
    [EnableQuery]
    public ActionResult<MultiLineString> GetRoutes(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Routes == null)
        {
            return NotFound();
        }

        return site.Routes;
    }

    // GET /Sites(1)/Parks
    [EnableQuery]
    public ActionResult<MultiPolygon> GetParks(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Parks == null)
        {
            return NotFound();
        }

        return site.Parks;
    }

    // GET /Sites(1)/Features
    [EnableQuery]
    public ActionResult<GeometryCollection> GetFeatures(int key)
    {
        var site = sites.FirstOrDefault(s => s.Id == key);
        if (site == null || site.Features == null)
        {
            return NotFound();
        }

        return site.Features;
    }

    private static List<Site> Generate()
    {
        var point = geographyFactory.CreatePoint(new Coordinate(37.30750, -0.15083));

        var lineString = geographyFactory.CreateLineString(
        [
            new Coordinate(37.30750, -0.15083),
            new Coordinate(37.32890, -0.1647)
        ]);

        var polygon = geographyFactory.CreatePolygon(new LinearRing(
        [
            new Coordinate(37.0000, 0.1010), // (lon, lat) NW
            new Coordinate(37.0020, 0.1010), // NE
            new Coordinate(37.0020, 0.0990), // SE
            new Coordinate(37.0000, 0.0990), // SW
            new Coordinate(37.0000, 0.1010)  // back to NW to close
        ]),
        [
            new LinearRing(
            [
                new Coordinate(37.0014, 0.1006),
                new Coordinate(37.0016, 0.1002),
                new Coordinate(37.0010, 0.1002),
                new Coordinate(37.0014, 0.1006) // close
            ]),
            new LinearRing(
            [
                new Coordinate(37.0015, 0.1007),
                new Coordinate(37.0017, 0.1005),
                new Coordinate(37.0015, 0.1003),
                new Coordinate(37.0013, 0.1005),
                new Coordinate(37.0015, 0.1007) // close
            ])
        ]);

        var multiPoint = geographyFactory.CreateMultiPoint([point]);
        var multiLineString = geographyFactory.CreateMultiLineString([lineString]);
        var multiPolygon = geographyFactory.CreateMultiPolygon([polygon]);

        return new List<Site>
        {
            new Site
            {
                Id = 1,
                Marker = point,
                Route = lineString,
                Park = polygon,
                Markers = multiPoint,
                Routes = multiLineString,
                Parks = multiPolygon,
                Features = geographyFactory.CreateGeometryCollection([point, lineString, polygon, multiPoint, multiLineString, multiPolygon])
            }
        };
    }
}

public class WarehousesController : ODataController
{
    private readonly static GeometryFactory geographyFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    private readonly List<Warehouse> parks = new List<Warehouse>
    {
        new Warehouse
        {
            Id = 1,
            Location = geographyFactory.CreatePoint(new Coordinate(37.30750, -0.15083)),
            Route = geographyFactory.CreateLineString(new[]
            {
                new Coordinate(37.30750, -0.15083),
                new Coordinate(37.32890, -0.1647)
            })
        }
    };

    // GET /Warehouses
    [EnableQuery]
    public ActionResult<IEnumerable<Warehouse>> Get()
    {
        return parks;
    }

    // GET /Warehouses(1)
    [EnableQuery]
    public ActionResult<Warehouse> Get(int key)
    {
        var warehouse = parks.FirstOrDefault(p => p.Id == key);
        if (warehouse == null)
        {
            return NotFound();
        }

        return warehouse;
    }

    // GET /Warehouses(1)/Location
    public ActionResult<Point> GetLocation(int key)
    {
        var warehouse = parks.FirstOrDefault(p => p.Id == key);
        if (warehouse == null || warehouse.Location == null)
        {
            return NotFound();
        }

        return warehouse.Location;
    }

    // GET /Warehouses(1)/Route
    public ActionResult<LineString> GetRoute(int key)
    {
        var warehouse = parks.FirstOrDefault(p => p.Id == key);
        if (warehouse == null || warehouse.Route == null)
        {
            return NotFound();
        }
        return warehouse.Route;
    }
}
