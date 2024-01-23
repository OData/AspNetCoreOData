//-----------------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Wrapper;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataRoutingSample.Models;
using ODataRoutingSample.OpenApi;

namespace ODataRoutingSample
{
    public sealed class FooDeserializer : ODataResourceDeserializer
    {
        public FooDeserializer(IODataDeserializerProvider deserializerProvider) : base(deserializerProvider)
        {
        }

        public override void ApplyStructuralProperties(object resource, ODataResourceWrapper resourceWrapper, IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            if (resourceWrapper.Resource.TypeName == "ODataRoutingSample.Models.fooTemplate" && resource is FooTemplate fooTemplate)
            {
                foreach (var nestedResourceInfo in resourceWrapper.NestedResourceInfos)
                {
                    if (nestedResourceInfo.NestedResourceInfo.Name == "fizz")
                    {
                        fooTemplate.FizzProvided = true;
                    }
                    else if (nestedResourceInfo.NestedResourceInfo.Name == "buzz")
                    {
                        fooTemplate.BuzzProvided = true;
                    }
                    else if (nestedResourceInfo.NestedResourceInfo.Name == "frob")
                    {
                        fooTemplate.FrobProvided = true;
                    }
                }
            }

            base.ApplyStructuralProperties(resource, resourceWrapper, structuredType, readContext);
        }

        public override object CreateResourceInstance(IEdmStructuredTypeReference structuredType, ODataDeserializerContext readContext)
        {
            var resource = base.CreateResourceInstance(structuredType, readContext);
            if (structuredType.FullName() == "ODataRoutingSample.Models.fooTemplate" && resource is FooTemplate fooTemplate)
            {
                fooTemplate.FizzProvided = false;
                fooTemplate.BuzzProvided = false;
                fooTemplate.FrobProvided = false;
            }

            return resource;
        }
    }

    public static class Extensions
    {
        public static bool TryFirst<T>(this IEnumerable<T> self, out T element)
        {
            using (var enumerator = self.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    element = default;
                    return false;
                }

                element = enumerator.Current;
                return true;
            }
        }
    }

    public sealed class FooSerializer : ODataResourceSerializer
    {
        public FooSerializer(IODataSerializerProvider serializerProvider) : base(serializerProvider)
        {
        }

        public override ODataValue CreateODataValue(object graph, IEdmTypeReference expectedType, ODataSerializerContext writeContext)
        {
            if (expectedType.FullName() == "ODataRoutingSample.Models.fizz" || 
                expectedType.FullName() == "ODataRoutingSample.Models.buzz" ||
                expectedType.FullName() == "ODataRoutingSample.Models.frob")
            {
                return new EmptyValue();
            }

            return base.CreateODataValue(graph, expectedType, writeContext);
        }

        private sealed class EmptyValue : ODataValue
        {
        }

        public override ODataResource CreateResource(SelectExpandNode selectExpandNode, ResourceContext resourceContext)
        {
            var resource = base.CreateResource(selectExpandNode, resourceContext);

            if (resourceContext.StructuredType.FullTypeName() == "ODataRoutingSample.Models.fooTemplate" && resourceContext.ResourceInstance is FooTemplate fooTemplate)
            {
                var properties = selectExpandNode.SelectedStructuralProperties.Select(property => base.CreateStructuralProperty(property, resourceContext)).ToList();
                var propertiesToAnnotate = new List<string>();
                if (!fooTemplate.FizzProvided)
                {
                    propertiesToAnnotate.Add("fizz");
                }

                if (!fooTemplate.BuzzProvided)
                {
                    propertiesToAnnotate.Add("buzz");
                }

                if (!fooTemplate.FrobProvided)
                {
                    propertiesToAnnotate.Add("frob");
                }

                foreach (var propertyToAnnotate in propertiesToAnnotate)
                {
                    if (selectExpandNode.SelectedComplexProperties.Where(kvp => kvp.Key.Name == propertyToAnnotate).TryFirst(out var property))
                    {
                        var structuralProperty = base.CreateStructuralProperty(property.Key, resourceContext);
                        structuralProperty.InstanceAnnotations.Add(new ODataInstanceAnnotation("microsoft.notProvided", new ODataPrimitiveValue(true)));
                        properties.Add(structuralProperty);
                        selectExpandNode.SelectedComplexProperties.Remove(property.Key);
                    }
                }

                resource.Properties = properties;
            }

            return resource;
        }
    }

    public class FooDemoData
    {
        public FooDemoData()
        {
            this.FooTemplates = new ConcurrentDictionary<string, FooTemplate>();
            this.Foos = new ConcurrentDictionary<string, Foo>();
        }

        public IDictionary<string, FooTemplate> FooTemplates { get; }

        public IDictionary<string, Foo> Foos { get; }
    }

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

            services.AddSingleton<FooDemoData>();

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
                    .AddRouteComponents("v1", model1, services => services.AddSingleton<ODataResourceSerializer, FooSerializer>().AddSingleton<ODataResourceDeserializer, FooDeserializer>())
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
