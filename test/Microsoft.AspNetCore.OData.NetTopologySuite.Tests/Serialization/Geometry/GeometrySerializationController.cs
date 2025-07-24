//-----------------------------------------------------------------------------
// <copyright file="GeometrySerializationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geometry;

public class PlantsController : ODataController
{
    private readonly static GeometryFactory geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0);

    private readonly static List<Plant> plants = Generate();

    // GET /Plants
    [EnableQuery]
    public ActionResult<IEnumerable<Plant>> Get()
    {
        return plants;
    }

    // GET /Plants(1)
    [EnableQuery]
    public ActionResult<Plant> Get(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null)
        {
            return NotFound();
        }

        return plant;
    }

    // GET /Plants(1)/Location
    [EnableQuery]
    public ActionResult<Point> GetLocation(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Location == null)
        {
            return NotFound();
        }

        return plant.Location;
    }

    // GET /Plants(1)/Track
    [EnableQuery]
    public ActionResult<LineString> GetTrack(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Track == null)
        {
            return NotFound();
        }

        return plant.Track;
    }

    // GET /Plants(1)/Zone
    [EnableQuery]
    public ActionResult<Polygon> GetZone(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Zone == null)
        {
            return NotFound();
        }

        return plant.Zone;
    }

    // GET /Plants(1)/Locations
    [EnableQuery]
    public ActionResult<MultiPoint> GetLocations(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Locations == null)
        {
            return NotFound();
        }

        return plant.Locations;
    }

    // GET /Plants(1)/Tracks
    [EnableQuery]
    public ActionResult<MultiLineString> GetTracks(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Tracks == null)
        {
            return NotFound();
        }

        return plant.Tracks;
    }

    // GET /Plants(1)/Zones
    [EnableQuery]
    public ActionResult<MultiPolygon> GetZones(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Zones == null)
        {
            return NotFound();
        }

        return plant.Zones;
    }

    // GET /Plants(1)/Layout
    [EnableQuery]
    public ActionResult<GeometryCollection> GetLayout(int key)
    {
        var plant = plants.FirstOrDefault(s => s.Id == key);
        if (plant == null || plant.Layout == null)
        {
            return NotFound();
        }

        return plant.Layout;
    }

    public static List<Plant> Generate()
    {
        var point = geometryFactory.CreatePoint(new Coordinate(2, 2));

        var lineString = geometryFactory.CreateLineString(
        [
            new Coordinate(-2, 4),
            new Coordinate(12, 6)
        ]);

        var polygon = geometryFactory.CreatePolygon(new LinearRing(
        [
            new Coordinate(0, 10),   // NW
            new Coordinate(10, 10),  // NE
            new Coordinate(10, 0),   // SE
            new Coordinate(0, 0),    // SW
            new Coordinate(0, 10)    // close
        ]),
        [
            new LinearRing(
            [
                new Coordinate(3, 5),
                new Coordinate(5, 5),
                new Coordinate(5, 3),
                new Coordinate(3, 3),
                new Coordinate(3, 5) // close
            ]),
            new LinearRing(
            [
                new Coordinate(7, 3),
                new Coordinate(9, 3),
                new Coordinate(9, 1),
                new Coordinate(7, 1),
                new Coordinate(7, 3) // close
            ])
        ]);

        var multiPoint = geometryFactory.CreateMultiPoint([point]);
        var multiLineString = geometryFactory.CreateMultiLineString([lineString]);
        var multiPolygon = geometryFactory.CreateMultiPolygon([polygon]);

        return new List<Plant>
        {
            new Plant
            {
                Id = 1,
                Location = point,
                Track = lineString,
                Zone = polygon,
                Locations = multiPoint,
                Tracks = multiLineString,
                Zones = multiPolygon,
                Layout = geometryFactory.CreateGeometryCollection([point, lineString, polygon, multiPoint, multiLineString, multiPolygon])
            }
        };
    }

}
