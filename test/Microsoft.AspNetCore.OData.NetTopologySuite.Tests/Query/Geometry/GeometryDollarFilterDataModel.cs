//-----------------------------------------------------------------------------
// <copyright file="GeometryDollarFilterDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geometry
{
    public class Plant
    {
        public int Id { get; set; }
        public Point Location { get; set; }
        public LineString Route { get; set; }
    }
}
