using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.OData.ModelBuilder;
using MinimalApisTestProj.EndPoints;
using MinimalApisTestProj.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMinimalOData(options =>
{
    var conventionModelBuilder = new ODataConventionModelBuilder();
    conventionModelBuilder.EnableLowerCamelCase();
    conventionModelBuilder.EntitySet<Customer>("customers");
    conventionModelBuilder.EntityType<Customer>();
    options.Count().Select().Expand().Count().Filter().SetMaxTop(5).AddRouteComponents("api/v1.0", conventionModelBuilder.GetEdmModel(), new DefaultODataBatchHandler());
});

var app = builder.Build();

app.MapGroup("api/v1.0")
    .WithODataRoutingMetadata(app.Services)
    .MapCustomerEndpoints();



app.Run();

