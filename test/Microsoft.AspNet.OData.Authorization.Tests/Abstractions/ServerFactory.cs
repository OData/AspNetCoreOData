// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Authorization.Tests.Abstractions
{
    /// <summary>
    /// Factory for creating a test servers.
    /// </summary>
    public class TestServerFactory
    {
        
        /// <summary>
        /// Create a TestServer that uses endpoint routing.
        /// </summary>
        /// <param name="controllers">The controllers to use.</param>
        /// <param name="configureEndpoints">The endpoints configuration action.</param>
        /// <param name="configureService">The service collection configuration action.</param>
        /// <param name="configureBuilder">The app builder configuration action.
        /// This can be used to add additional middleware before the endpoints middleware.</param>
        /// <returns>An TestServer.</returns>
        public static TestServer CreateWithEndpointRouting(
            Type[] controllers,
            Action<IEndpointRouteBuilder> configureEndpoints,
            Action<IServiceCollection> configureService = null,
            Action<IApplicationBuilder> configureBuilder = null)
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder();
            builder.ConfigureServices(services =>
            {
                services.AddOData();
                configureService?.Invoke(services);
            });

            builder.Configure(app =>
            {
                app.Use(next => context =>
                {
                    var body = context.Features.Get<IHttpBodyControlFeature>();
                    if (body != null)
                    {
                        body.AllowSynchronousIO = true;
                    }

                    return next(context);
                });

                app.UseODataBatching();
                app.UseRouting();
                configureBuilder?.Invoke(app);
                app.UseEndpoints((endpoints) =>
                {

                    var appBuilder = endpoints.CreateApplicationBuilder();
                    ApplicationPartManager applicationPartManager = appBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
                    applicationPartManager.ApplicationParts.Clear();

                    if (controllers != null)
                    {
                        AssemblyPart part = new AssemblyPart(new MockAssembly(controllers));
                        applicationPartManager.ApplicationParts.Add(part);
                    }

                    // Insert a custom ControllerFeatureProvider to bypass the IsPublic restriction of controllers
                    // to allow for nested controllers which are excluded by the built-in ControllerFeatureProvider.
                    applicationPartManager.FeatureProviders.Clear();
                    applicationPartManager.FeatureProviders.Add(new TestControllerFeatureProvider());

                    configureEndpoints(endpoints);
                });
            });

            return new TestServer(builder);
        }

        /// <summary>
        /// Create an HttpClient from a server.
        /// </summary>
        /// <param name="server">The TestServer.</param>
        /// <returns>An HttpClient.</returns>
        public static HttpClient CreateClient(TestServer server)
        {
            return server.CreateClient();
        }

        private class TestControllerFeatureProvider : ControllerFeatureProvider
        {
            /// <inheritdoc />
            /// <remarks>
            /// Identical to ControllerFeatureProvider.IsController except for the typeInfo.IsPublic check.
            /// </remarks>
            protected override bool IsController(TypeInfo typeInfo)
            {
                if (!typeInfo.IsClass)
                {
                    return false;
                }

                if (typeInfo.IsAbstract)
                {
                    return false;
                }

                if (typeInfo.ContainsGenericParameters)
                {
                    return false;
                }

                if (typeInfo.IsDefined(typeof(NonControllerAttribute)))
                {
                    return false;
                }

                if (!typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) &&
                    !typeInfo.IsDefined(typeof(ControllerAttribute)))
                {
                    return false;
                }

                return true;
            }
        }
    }
}
