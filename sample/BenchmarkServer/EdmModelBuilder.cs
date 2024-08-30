//-----------------------------------------------------------------------------
// <copyright file="EdmModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataPerformanceProfile.Models;

namespace ODataPerformanceProfile;

public static class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.EntitySet<Product>("Products");
        builder.EntitySet<Supplier>("Suppliers");
        builder.EntitySet<Order>("Orders");

        builder.EntityType<Product>().Collection
            .Function("mostRecent")
            .Returns<string>();

        builder.EntityType<Product>()
            .Action("rate")
            .Parameter<int>("rating");

        var model = builder.GetEdmModel();
        model.MarkAsImmutable();

        return model;
    }
}
