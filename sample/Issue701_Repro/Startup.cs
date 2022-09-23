using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Issue701_Repro.Models;

namespace Issue701_Repro
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddOData(opt => opt.Count().Filter().Expand().Select().OrderBy().SetMaxTop(5)
                .AddRouteComponents(GetSampleEntityDataModel()));
        }

        // This method gets called by the runtime to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public IEdmModel GetSampleEntityDataModel()
        {
            var builder = new ODataConventionModelBuilder()
            {
                Namespace = "Samples",
                ContainerName = "SamplesContainer"
            };

            //Catalog endpoint
            builder.Singleton<Sample>("sample");
            builder.EntityType<SampleItems>();
            builder.EnableLowerCamelCase();
            IEdmModel model = builder.GetEdmModel();

            return model;
        }
    }
}