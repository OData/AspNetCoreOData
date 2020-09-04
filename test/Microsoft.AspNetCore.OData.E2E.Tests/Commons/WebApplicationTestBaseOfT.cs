// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    public class WebApiODataTestFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .UseStartup<TStartup>()
                .UseContentRoot("")
                //       .UseEnvironment("Production")
                .ConfigureServices(
                    services =>
                    {
                        //var testSink = new TestSink();
                        //var loggerFactory = new TestLoggerFactory(testSink, enabled: true);
                        //services.AddSingleton<ILoggerFactory>(loggerFactory);
                    });

     //       base.ConfigureWebHost(builder);
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                return base.CreateServer(builder);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                return base.CreateHost(builder);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base.CreateWebHostBuilder();
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder();
         //   return base.CreateHostBuilder();
        }
    }

    /// <summary>
    /// The WebApplicationTestBase creates a web host to be used for a test.
    /// </summary>
    public abstract class WebApplicationTestBase<TEntryPoint> : IClassFixture<WebApiODataTestFixture<TEntryPoint>> where TEntryPoint : class
    {
        private HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApplicationTestBase{TEntryPoint}"/> class.
        /// </summary>
        /// <param name="fixture">The fixture used to initialize the web service.</param>
        protected WebApplicationTestBase(WebApiODataTestFixture<TEntryPoint> factory)
        {
            // Factory = factory.Factories.FirstOrDefault() ?? factory.WithWebHostBuilder(builder => builder.UseStartup<TEntryPoint>());
            Factory = factory;
        }

        /// <summary>
        /// An HttpClient to use with the server.
        /// </summary>
        public virtual HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = Factory.CreateClient();
                }

                return _client;
            }
        }

        public WebApplicationFactory<TEntryPoint> Factory { get; }
    }

    public static class ApplicationBuilderExtensions
    {
        public static void AddControllers(this IApplicationBuilder app, params Type[] controllers)
        {
            ApplicationPartManager partManager = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();

            IList<ApplicationPart> parts = partManager.ApplicationParts;
            IList<ApplicationPart> nonAssemblyParts = parts.Where(p => p.GetType() != typeof(IApplicationPartTypeProvider)).ToList();
            partManager.ApplicationParts.Clear();
            partManager.ApplicationParts.Concat(nonAssemblyParts);

            // Add a new AssemblyPart with the controllers.
            AssemblyPart part = new AssemblyPart(new TestAssembly(controllers));
            partManager.ApplicationParts.Add(part);

        }
    }

}
