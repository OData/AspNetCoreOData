// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.OData;
using ODataRoutingSample.Models;
using ODataRoutingSample.OpenApi;
using System.Collections.Generic;
using System;
using Microsoft.OData.ModelBuilder;

namespace ODataRoutingSample
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
            services.AddDbContext<MyDataContext>(opt => opt.UseLazyLoadingProxies().UseInMemoryDatabase("MyDataContextList"));

            IEdmModel model0 = EdmModelBuilder.GetEdmModel();
            IEdmModel model1 = EdmModelBuilder.GetEdmModelV1();
            IEdmModel model2 = EdmModelBuilder.GetEdmModelV2();

            services.AddControllers(options => {
            //{
            //    options.Conventions.Add(new MetadataApplicationModelConventionAttribute());
            //    options.Conventions.Add(new MetadataActionModelConvention());
            });

            /*services.AddConvention<MyConvention>();
            
            services.AddOData()
                .AddODataRouting(options => options
                    .AddModel(EdmModelBuilder.GetEdmModel())
                    .AddModel("v1", EdmModelBuilder.GetEdmModelV1())
                    .AddModel("v2{data}", EdmModelBuilder.GetEdmModelV2()));

            services.AddODataFormatter();
            services.AddODataQuery(options => options.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5));
            */

            //services.AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5)
            //    .AddModel(model0)
            //    .AddModel("v1", model1)
            //    .AddModel("v2{data}", model2, builder => builder.AddService<ODataBatchHandler, DefaultODataBatchHandler>(Microsoft.OData.ServiceLifetime.Singleton))
            //    //.ConfigureRoute(route => route.EnableQualifiedOperationCall = false) // use this to configure the built route template
            //    .Conventions.Add(new MyConvention())
            //    );

            services.AddOData(opt => opt.AddModel("odata", GetEdmModel()));

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Use odata route debug, /$odata
            app.UseODataRouteDebug();

            // If you want to use /$openapi, enable the middleware.
            //app.UseODataOpenApi();

            // Add OData /$query middleware
            app.UseODataQueryRequest();

            // Add the OData Batch middleware to support OData $Batch
            app.UseODataBatching();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 8.x OpenAPI");
            });

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

        static IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<Student>("Student");
            var entity = odataBuilder.EntityType<Student>();
            entity.DerivesFrom<EntityBase>();
            entity.Ignore(s => s.Test);
            entity.Ignore(s => s.Test2);

            var baseEntity = odataBuilder.EntityType<EntityBase>();
            baseEntity.Abstract();
            baseEntity.Ignore(s => s.Test);
            baseEntity.Ignore(s => s.Test2);

            return odataBuilder.GetEdmModel();
        }
    }

    /// <summary>
    /// My simple convention
    /// </summary>
    public class MyConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// Order value.
        /// </summary>
        public int Order => -100;

        /// <summary>
        /// Apply to action,.
        /// </summary>
        /// <param name="context">Http context.</param>
        /// <returns>true/false</returns>
        public bool AppliesToAction(ODataControllerActionContext context)
        {
            return true; // apply to all controller
        }

        /// <summary>
        /// Apply to controller
        /// </summary>
        /// <param name="context">Http context.</param>
        /// <returns>true/false</returns>
        public bool AppliesToController(ODataControllerActionContext context)
        {
            return false; // continue for all others
        }
    }

    public class Student : EntityBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public abstract class EntityBase
    {
        public Dictionary<string, object> Test { get; set; }
        public Dictionary<string, object> Test2 { get; set; }
    }
}
