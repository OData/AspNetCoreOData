//-----------------------------------------------------------------------------
// <copyright file="Program.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Query;
using ODataMiniApi;
using ODataMiniApi.Students;
using Microsoft.OData.Edm;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(options => options.UseInMemoryDatabase("SchoolStudentList"));

// In pre-built OData, MailAddress and Student are complex types
IEdmModel model = EdmModelBuilder.GetEdmModel();

builder.Services.AddOData(q => q.EnableAll());

var app = builder.Build();
app.MakeSureDbCreated();

// OData $metadata, I use $odata for routing.
app.MapODataMetadata("/v1/$odata", model);
app.MapODataMetadata("/customized/$odata", model, new CustomizedMetadataHandler());

#region School Endpoints

app.MapGet("/myschools", (AppDb db) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return db.Schools;
})
    .WithModel(model)
 //   .AddODataQueryEndpointFilter(new ODataQueryEndpointFilter(app.Services.GetRequiredService<ILoggerFactory>()));
    .AddODataQueryEndpointFilter();

app.MapGet("/schools", (AppDb db, ODataQueryOptions<School> options) => {
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return options.ApplyTo(db.Schools);
}).WithModel(model);

// Use the model built on the fly
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

// Use the model built on the fly
app.MapGet("/customized/schools", (AppDb db, ODataQueryOptions<School> options) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return options.ApplyTo(db.Schools);
});

#endregion

// Endpoints for students
app.MapStudentEndpoints();

app.Run();

