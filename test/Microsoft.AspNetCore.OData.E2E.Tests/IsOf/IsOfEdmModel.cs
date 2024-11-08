//-----------------------------------------------------------------------------
// <copyright file="IsOfEdmModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.E2E.Tests.IsOf;

internal class IsOfEdmModel
{
    public static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
        builder.EntitySet<Product>("Products");

        var airPlaneType=builder.EntityType<AirPlane>();
        airPlaneType.DerivesFrom<Product>();

        builder.Namespace = typeof(Product).Namespace;

        var edmModel = builder.GetEdmModel();
        return edmModel;
    }
}
