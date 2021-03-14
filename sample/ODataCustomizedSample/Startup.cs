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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseOData();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
