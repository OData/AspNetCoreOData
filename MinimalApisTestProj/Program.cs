using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
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

app.MapGet("api/v1.0/customers", CustomerEndPoints.GetAllCustomers)
    .WithODataRoutingMetadata(app.Services)
    .AddEndpointFilter<EnableQueryFilter>();

app.MapPost("api/v1.0/customers", CustomerEndPoints.PostCustomer)
    .WithODataRoutingMetadata(app.Services);



app.Run();

