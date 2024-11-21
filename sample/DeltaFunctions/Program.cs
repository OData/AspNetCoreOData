using Microsoft.AspNetCore.OData;
using microsoft.graph;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().
    AddOData(opt => opt.EnableQueryFeatures()
        .AddRouteComponents("", GraphModel.Model));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseODataRouteDebug();

app.UseAuthorization();

app.MapControllers();

app.Run();
