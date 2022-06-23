using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using ODataAlternateKeySample.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddTransient<IAlternateKeyRepository, AlternateKeyRepositoryInMemory>();

// builder.Services.AddSingleton<Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider, IngoreNullEntityPropertiesSerializerProvider>();

var serviceDescriptor = builder.Services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IODataSerializerProvider));
builder.Services.Remove(serviceDescriptor);
builder.Services.AddSingleton<Microsoft.AspNetCore.OData.Formatter.Serialization.IODataSerializerProvider, IngoreNullEntityPropertiesSerializerProvider>();

builder.Services.AddControllers().
    AddOData(opt => opt.EnableQueryFeatures()
        .AddRouteComponents("odata", EdmModelBuilder.GetEdmModel()));

builder.Services.AddTransient<IAlternateKeyRepository, AlternateKeyRepositoryInMemory>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseODataRouteDebug();

app.UseAuthorization();

app.MapControllers();

app.Run();
