//-----------------------------------------------------------------------------
// <copyright file="Program.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using ODataPerformanceProfile;
using ODataPerformanceProfile.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductsContext>(opt => opt.UseInMemoryDatabase("MyDataContextList"));

// Add services to the container.
builder.Services.AddControllers().AddOData(
    opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5).AddRouteComponents("odata",EdmModelBuilder.GetEdmModel())
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
