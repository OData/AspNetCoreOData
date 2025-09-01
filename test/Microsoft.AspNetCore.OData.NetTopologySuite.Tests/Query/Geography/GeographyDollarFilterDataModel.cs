//-----------------------------------------------------------------------------
// <copyright file="GeographyDollarFilterDataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Attributes;
using NetTopologySuite.Geometries;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geography
{
    public class Site
    {
        public int Id { get; set; }
        [Geography]
        public Point Location { get; set; }
        [Geography]
        public LineString Route { get; set; }
    }
}
