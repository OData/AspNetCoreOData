//-----------------------------------------------------------------------------
// <copyright file="SchoolEndpoints.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;

namespace ODataMiniApi.Students;

/// <summary>
/// Add school endpoint
/// </summary>
public static class SchoolEndpoints
{
    public static IEndpointRouteBuilder MapSchoolEndpoints(this IEndpointRouteBuilder app, IEdmModel model)
    {
        app.MapGet("/getschools1", (AppDb db) => db.Schools.Include(s => s.Students));

        app.MapGet("/odata/getschools1", (AppDb db) => db.Schools.Include(s => s.Students))
            .WithODataResult();

        app.MapGet("/odata/getschools2", (AppDb db) => db.Schools.Include(s => s.Students))
            .WithODataModel(model);

        app.MapGet("/odata/getschools2", (AppDb db) => db.Schools.Include(s => s.Students))
            .WithODataResult()
            .WithODataModel(model);
        return app;
    }

}
