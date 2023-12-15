//-----------------------------------------------------------------------------
// <copyright file="Program.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Extensions;
using ODataMiniApi;
using ODataMiniApi.Students;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(options => options.UseInMemoryDatabase("SchoolStudentList"));
builder.Services.AddOData(opt => opt.EnableQueryFeatures()// This line is required
    .AddRouteComponents("customized", EdmModelBuilder.GetEdmModel()) // This line is used to test 'UseOData' extension
); 

var app = builder.Build();
app.MakeSureDbCreated();

#region School Endpoints

app.MapGet("/schools", (AppDb db, ODataQueryOptions<School> options) => {
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return options.ApplyTo(db.Schools);
});

app.MapGet("/schools/{id}", async (int id, AppDb db, ODataQueryOptions<School> options) => {
    School school = await db.Schools.Include(c => c.MailAddress)
        .Include(c => c.Students).FirstOrDefaultAsync(s => s.SchoolId == id);
    if (school == null)
    {
        return Results.NotFound($"Cannot find school with id '{id}'");
    }
    else
    {
        return Results.Ok(options.ApplyTo(school, new ODataQuerySettings()));
    }
});

app.MapGet("/customized/schools", (AppDb db, ODataQueryOptions<School> options) => {
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return options.ApplyTo(db.Schools);
}).UseOData("customized"); // In customized OData, MailAddress and Student are complex type, you can also use 'IODataModelConfiguration'

#endregion

// Endpoints for students
app.MapStudentEndpoints();

app.Run();

