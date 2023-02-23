//-----------------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using ODataRoutingSample.Models;
using ODataRoutingSample.OpenApi;

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
            #region Codes for backup and use to compare with preview design/implementation
            //services.AddControllers(options => {
            //{
            //    options.Conventions.Add(new MetadataApplicationModelConventionAttribute());
            //    options.Conventions.Add(new MetadataActionModelConvention());
            //});

            /*services.AddConvention<MyConvention>();
            
            services.AddOData()
                .AddODataRouting(options => options
                    .AddModel(EdmModelBuilder.GetEdmModel())
                    .AddModel("v1", EdmModelBuilder.GetEdmModelV1())
                    .AddModel("v2{data}", EdmModelBuilder.GetEdmModelV2()));

            services.AddODataFormatter();
            services.AddODataQuery(options => options.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5));
            */
            #endregion

            services.AddDbContext<MyDataContext>(opt => opt.UseLazyLoadingProxies().UseInMemoryDatabase("MyDataContextList"));

            IEdmModel model0 = EdmModelBuilder.GetEdmModel();
            IEdmModel model1 = EdmModelBuilder.GetEdmModelV1();
            IEdmModel model2 = EdmModelBuilder.GetEdmModelV2();
            IEdmModel model3 = EdmModelBuilder.GetEdmModelV3();

            services.AddControllers()
                /*  If you want to remove $metadata endpoint, you can use ControllerFeatureProvider as follows
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.Remove(manager.FeatureProviders.OfType<ControllerFeatureProvider>().FirstOrDefault());
                    manager.FeatureProviders.Add(new RemoveMetadataControllerFeatureProvider());
                })

                or, remove MetadataRoutingConvention in AddOData as
                     opt.Conventions.Remove(opt.Conventions.First(convention => convention is MetadataRoutingConvention));
                */
                .AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5)
                    .AddRouteComponents(model0)
                    .AddRouteComponents("v1", model1)
                    .AddRouteComponents("v2{data}", model2, services => services.AddSingleton<ODataBatchHandler, DefaultODataBatchHandler>())
                    .AddRouteComponents("v3", model3)
                    .Conventions.Add(new MyConvention())
                );

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
            app.UseODataOpenApi();

            // Add OData /$query middleware
            app.UseODataQueryRequest();

            // Add the OData Batch middleware to support OData $Batch
            app.UseODataBatching();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OData 8.x OpenAPI");
                c.SwaggerEndpoint("/$openapi", "OData raw OpenAPI");
            });

            app.UseRouting();

            // Test middleware
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

    public class RemoveMetadataControllerFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (typeInfo.FullName == "Microsoft.AspNetCore.OData.Routing.Controllers.MetadataController")
            {
                return false;
            }

            return base.IsController(typeInfo);
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
}
