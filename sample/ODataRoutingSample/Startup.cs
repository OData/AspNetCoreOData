// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Http;
using ODataRoutingSample.Models;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData;

namespace ODataRoutingSample
{
    public class Startup
    {
        private IEdmModel model;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            model = EdmModelBuilder.GetEdmModel();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IEdmModel model0 = EdmModelBuilder.GetEdmModel();
            IEdmModel model1 = EdmModelBuilder.GetEdmModelV1();
            IEdmModel model2 = EdmModelBuilder.GetEdmModelV2();

            services.AddControllers(options => {
            //{
            //    options.Conventions.Add(new MetadataApplicationModelConventionAttribute());
            //    options.Conventions.Add(new MetadataActionModelConvention());
            });

            services.AddConvention<MyConvention>();

            services.AddOData()
                .AddODataRouting(options => options
                    .AddModel(EdmModelBuilder.GetEdmModel())
                    .AddModel("v1", EdmModelBuilder.GetEdmModelV1())
                    .AddModel("v2{data}", EdmModelBuilder.GetEdmModelV2()));

            services.AddODataFormatter();

            /*
            services.AddOData(opt => opt.UseODataRouting(model1).
                opt.UseODataRouting("v1", model2).
                opt.UseODataRouting("v2{data}", model3).
                opt.UseODataFormatter().
                opt.UseODataQuery()
                );
            */
            services.AddOData(opt => opt.UseModel(model0)
                .UseModel("v1", model1)
                .UseModel("v2{data}", model2))
                .AddODataRouting()
                .AddFormatter()
                .AddODataQuery();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // Test middelware
            app.Use(next => context =>
            {
                var endpoint = context.GetEndpoint();
                if (endpoint == null)
                {
                    return next(context);
                }

                return next(context);
            });

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MyConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public int Order => -100;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool AppliesToAction(ODataControllerActionContext context)
        {
            return true; // apply to all controller
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool AppliesToController(ODataControllerActionContext context)
        {
            return false; // continue for all others
        }
    }
}
