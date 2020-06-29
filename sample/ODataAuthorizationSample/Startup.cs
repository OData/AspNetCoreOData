using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using AspNetCore3ODataPermissionsSample.Models;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore3ODataPermissionsSample
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
            services.AddDbContext<AppDbContext>(opt => opt.UseLazyLoadingProxies().UseInMemoryDatabase("CustomerOrderList"));
            services.AddOData();

            // add OData authorization services
            services.AddODataAuthorization((options) =>
            {
                // we setup a custom scope finder that will extract the user's scopes from the Permission claims
                options.ScopesFinder = (context) =>
                {
                    var permissions = context.User?.FindAll("Permission");
                    if (permissions == null)
                    {
                        return Task.FromResult(Enumerable.Empty<string>());
                    }

                    return Task.FromResult(permissions.Select(p => p.Value));
                };
            });

            // OData authorization depends on the AspNetCore authentication and authorization services
            // we need to specify at least one authentication scheme and handler. Here we opt for a simple custom handler defined
            // later in this file, for demonstration purposes. Could also use cookie-based or JWT authentication
            services.AddAuthentication("AuthScheme")
                .AddScheme<CustomAuthentationOptions, CustomAuthenticationHandler>("AuthScheme", options => { });
            services.AddAuthorization();

            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // add OData authorization middleware
            // we don't need to UseAuthorization() if we don't need to handle authorizaiton for non-odata routes
            app.UseODataAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapODataRoute("odata", "odata", AppModel.GetEdmModel());
            });
        }
    }

    // our customer authentication handler
    internal class CustomAuthenticationHandler : AuthenticationHandler<CustomAuthentationOptions>
    {
        public CustomAuthenticationHandler(IOptionsMonitor<CustomAuthentationOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new System.Security.Principal.GenericIdentity("Me");
            // in this dummy authentication scheme, we assume that the permissions granted
            // to the user are stored as a comma-separate list in a header called Permissions
            var scopeValues = Request.Headers["Permissions"];
            if (scopeValues.Count != 0)
            {
                var scopes = scopeValues.ToArray()[0].Split(",").Select(s => s.Trim());
                var claims = scopes.Select(scope => new Claim("Permission", scope));
                identity.AddClaims(claims);
            }

            var principal = new GenericPrincipal(identity, Array.Empty<string>());
            // we use the same auhentication scheme as the one specified in the OData model permissions
            var ticket = new AuthenticationTicket(principal, "AuthScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    internal class CustomAuthentationOptions : AuthenticationSchemeOptions
    {
    }
}
