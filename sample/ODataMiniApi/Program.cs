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
using Microsoft.AspNetCore.OData.Results;
using System.Text.Json;
using Microsoft.AspNetCore.OData.Query.Expressions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(options => options.UseInMemoryDatabase("SchoolStudentList"));

// In pre-built OData, MailAddress and Student are complex types
IEdmModel model = EdmModelBuilder.GetEdmModel();


builder.Services.ConfigureHttpJsonOptions(options => {
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});

builder.Services.AddOData(q => q.EnableAll());

var app = builder.Build();
app.MakeSureDbCreated();

ODataMiniMetadata me = new ODataMiniMetadata();
me.Services = services =>
{
    services.AddSingleton<IFilterBinder, FilterBinder>();
};

app.MapGet("test", () => "hello world").WithODataResult();

// Group
var group = app.MapGroup("")
//    .WithOData2(metadata => metadata.IsODataFormat = true, services => services.AddSingleton<IFilterBinder>(new FilterBinder()))
    ;

group.MapGet("v1", () => "hello v1")
    .AddEndpointFilter(async (efiContext, next) =>
    {
        var endpoint = efiContext.HttpContext.GetEndpoint();
        app.Logger.LogInformation("----Before calling");
        var result = await next(efiContext);
        app.Logger.LogInformation($"----After calling, {result?.GetType().Name}");
        return result;
    }
    ).Finally(v =>
    {
        v.Metadata.Add(new School());
    });
group.MapGet("v2", () => "hello v2");

//
app.MapGet("/giveschools1", (AppDb db) =>
{
    return db.Schools.Include(s => s.Students);
});

app.MapGet("/giveschools2", (AppDb db, ODataQueryOptions<School> options) =>
{
    return options.ApplyTo(db.Schools);
})
    .WithODataResult();

app.MapGet("/giveschools3", (AppDb db) =>
{
    return db.Schools.Include(s => s.Students);
})
    .WithODataResult()
    .AddODataQueryEndpointFilter(querySetup: q => q.PageSize = 3, validationSetup: q => q.MaxSkip = 4); 

//app.MapGet("/odata/schools", (AppDb db) =>
//{
//    return Results.Extensions.OData(db.Schools.Include(s => s.Students));
//});

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{ WriteIndented = true };
app.MapGet("/", () =>
    Results.Json(new School { SchoolName = "Walk dog", SchoolId = 8 }, options));

// OData $metadata, I use $odata for routing.
#region ODataMetadata
//app.MapODataMetadata1("/v1/$odata", model);
//app.MapODataMetadata("/customized/$odata", model, new CustomizedMetadataHandler());

app.MapODataServiceDocument("v1/$document", model);

app.MapODataServiceDocument("v2/$document", model)
    .WithODataBaseAddressFactory(c => new Uri("http://localhost:5177/v2"));

app.MapODataMetadata("v1/$metadata", model);

#endregion

#region School Endpoints

app.MapGet("/myschools", (AppDb db) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return db.Schools;
})
    .WithODataModel(model)
 //   .AddODataQueryEndpointFilter(new ODataQueryEndpointFilter(app.Services.GetRequiredService<ILoggerFactory>()));
    .AddODataQueryEndpointFilter(querySetup: q => q.PageSize = 3, validationSetup: q => q.MaxSkip = 4);

// use the server side pagesize?
app.MapGet("/schoolspage", (AppDb db) =>
{
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return new PageResult<School>(db.Schools, new Uri("/schoolspage?$skip=2", UriKind.Relative), count: db.Schools.Count());
    //return db.Schools;
})
    .WithODataModel(model)
    .AddODataQueryEndpointFilter(querySetup: q => q.PageSize = 3);

app.MapGet("/schools", (AppDb db, ODataQueryOptions<School> options) => {
    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    return options.ApplyTo(db.Schools);
}).WithODataModel(model);

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

app.MapCustomersEndpoints(model);

app.MapOrdersEndpoints(model);

app.MapSchoolEndpoints(model);

// Endpoints for students
app.MapStudentEndpoints();

app.Run();

/// <summary>
/// This is required in E2E test to identify the assembly.
/// </summary>
public partial class Program
{ }
