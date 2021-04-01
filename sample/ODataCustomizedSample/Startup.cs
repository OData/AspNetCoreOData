// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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

            services.AddControllers();

            services.AddOData(opt =>
                opt
                    .AddModel(model1)
                    .AddModel("odata", model2)
                    .AddModel("v{version}", model1)
                    .AddModel("convention", model3)
                    .AddModel("explicit", model4))
                .AddConvention<MyEntitySetRoutingConvention>();

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
