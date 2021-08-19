//-----------------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using ODataCustomizedSample.Extensions;
using ODataCustomizedSample.Models;

namespace ODataCustomizedSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IEdmModel model1 = EdmModelBuilder.GetEdmModel();
            IEdmModel model2 = EdmModelBuilder.BuildEdmModel();
            IEdmModel model3 = EnumsEdmModel.GetConventionModel();
            IEdmModel model4 = EnumsEdmModel.GetExplicitModel();

            services.AddControllers().AddOData(opt =>
                opt
                    .AddRouteComponents(model1)
                    .AddRouteComponents("odata", model2)
                    .AddRouteComponents("v{version}", model1)
                    .AddRouteComponents("convention", model3)
                    .AddRouteComponents("explicit", model4)
                    .Conventions.Add(new MyEntitySetRoutingConvention()));

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 8.x OpenAPI");
            });

            app.UseRouting();

            // for route debug page. be noted: you can put the middleware after UseRouting.
            // and you can use different route pattern name.
            app.UseODataRouteDebug("$odata2");

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
