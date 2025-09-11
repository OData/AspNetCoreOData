using Microsoft.AspNetCore.OData.NetTopologySuite.Extensions;
//-----------------------------------------------------------------------------
// <copyright file="GeographyDollarFilterEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.Geography;

public class GeographyDollarFilterEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder()
            .UseNetTopologySuite();

        modelBuilder.EntitySet<Site>("Sites");

        var model = modelBuilder.GetEdmModel()
            .UseNetTopologySuite();

        return model;
    }
}
