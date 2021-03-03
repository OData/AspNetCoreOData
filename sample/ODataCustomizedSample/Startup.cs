using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using ODataCustomizedSample.Extensions;

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
            services.AddControllers();
            services.AddOData(opt => opt.AddModel("odata", GetEdmModel())).AddConvention<MyEntitySetRoutingConvention>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private IEdmModel GetEdmModel()
        {
            var models = new Dictionary<string, string>()
            {
                    {"Customer", "CustomerId" },
                    {"Car", "CarId" },
                    {"School", "SchoolId" }
            };

            var model = new EdmModel();

            EdmEntityContainer container = new EdmEntityContainer("Default", "Container");

            for (int i = 0; i < models.Count; i++)
            {
                var name = models.ElementAt(i);
                EdmEntityType element = new EdmEntityType("Default", name.Key, null, false, true);
                element.AddKeys(element.AddStructuralProperty(name.Value, EdmPrimitiveTypeKind.Int32));

                model.AddElement(element);
                container.AddEntitySet(name.Key, element);
            }

            model.AddElement(container);
            return model;
        }
    }
}
