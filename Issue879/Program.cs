using Issue879.Models;
using Microsoft.AspNetCore.OData;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddOData(opt => opt.AddRouteComponents("odata", EdmModelBuilder.GetEdmModel()));

var app = builder.Build();

// Configure the HTTP request pipeline.

// 6) send $odata
app.UseODataRouteDebug();

app.UseAuthorization();

app.MapControllers();

app.Run();
