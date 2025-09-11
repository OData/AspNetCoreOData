//-----------------------------------------------------------------------------
// <copyright file="GeometrySerializationDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geometry;

public class Plant
{
    public int Id { get; set; }
    public Point Location { get; set; }
    public LineString Track { get; set; }
    public Polygon Zone { get; set; }
    public MultiPoint Locations { get; set; }
    public MultiLineString Tracks { get; set; }
    public MultiPolygon Zones { get; set; }
    public GeometryCollection Layout { get; set; }
}
