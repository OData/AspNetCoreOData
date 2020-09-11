// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    /// <summary>
    /// Utils for the <see cref="TestServer"/>.
    /// </summary>
    public static class TestServerUtils
    {
        /// <summary>
        /// Creates the default TestServer
        /// </summary>
        /// <returns>The created test server.</returns>
        public static TestServer Create()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddControllers();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            return new TestServer(builder);
        }

        /// <summary>
        /// Creates the Test server using the config and controller types.
        /// </summary>
        /// <param name="setupConfig">The config action.</param>
        /// <param name="controllers">The controller types.</param>
        /// <returns>The created test server.</returns>
        public static TestServer Create(Action<ODataOptions> setupConfig, params Type[] controllers)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddControllers();
                    services.ConfigureControllers(controllers);
                    services.AddOData(setupConfig);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            return new TestServer(builder);
        }

        /// <summary>
        /// Creates the Test server using the service config and application builder action.
        /// </summary>
        /// <param name="configureServices">The service config.</param>
        /// <param name="configureApp">The application builder.</param>
        /// <returns>The created test server.</returns>
        public static TestServer Create(Action<IServiceCollection> configureServices, Action<IApplicationBuilder> configureApp)
        {
            var builder = new WebHostBuilder()
                .ConfigureServices(configureServices)
                .Configure(configureApp);

            return new TestServer(builder);
        }

        /// <summary>
        /// Creates the Test server using the startup class.
        /// </summary>
        /// <typeparam name="TStartup">The start up class type.</typeparam>
        /// <returns>The created test server.</returns>
        public static TestServer Create<TStartup>() where TStartup : class
        {
            var builder = new WebHostBuilder()
                .UseStartup<TStartup>();

            return new TestServer(builder);
        }
    }
}
