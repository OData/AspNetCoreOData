//-----------------------------------------------------------------------------
// <copyright file="GeographySerializationEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Serialization.Geography;

public class GeographySerializationEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder()
            .UseNetTopologySuite();

        modelBuilder.EntitySet<Site>("Sites");
        modelBuilder.EntitySet<Warehouse>("Warehouses");

        var model = modelBuilder.GetEdmModel()
            .UseNetTopologySuite();

        return model;
    }
}
