//-----------------------------------------------------------------------------
// <copyright file="GeographySerializationDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Attributes;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geography;

public class Site
{
    public int Id { get; set; }
    [Geography]
    public Point Marker { get; set; }
    [Geography]
    public LineString Route { get; set; }
    [Geography]
    public Polygon Park { get; set; }
    [Geography]
    public MultiPoint Markers { get; set; }
    [Geography]
    public MultiLineString Routes { get; set; }
    [Geography]
    public MultiPolygon Parks { get; set; }
    [Geography]
    public GeometryCollection Features { get; set; }
}

[Geography]
public class Warehouse
{
    public int Id { get; set; }
    public Point Location { get; set; }
    public LineString Route { get; set; }
}

public class SqlSite
{
    public int Id { get; set; }
    public Point Marker { get; set; }
    public LineString Route { get; set; }
}
