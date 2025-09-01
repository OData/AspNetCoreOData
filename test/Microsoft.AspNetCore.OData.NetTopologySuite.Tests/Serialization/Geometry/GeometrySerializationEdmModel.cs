//-----------------------------------------------------------------------------
// <copyright file="GeometrySerializationEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geometry;

internal class GeometrySerializationEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder()
            .UseNetTopologySuite();

        modelBuilder.EntitySet<Plant>("Plants");

        var model = modelBuilder.GetEdmModel()
            .UseNetTopologySuite();

        return model;
    }
}
