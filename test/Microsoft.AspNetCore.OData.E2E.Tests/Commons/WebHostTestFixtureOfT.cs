//-----------------------------------------------------------------------------
// <copyright file="WebHostTestFixtureOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    // https://sourcegraph.com/github.com/dotnet/aspnetcore@bbb851e3ebf40f79531bc13dd5c1b56b332237fc/-/blob/src/Mvc/Mvc.Testing/src/WebApplicationFactory.cs#L24:1

    /// <summary>
    /// The WebHostTestFixture is create a web host to be used for a test.
    /// </summary>
    /// <remarks>
    /// This is a Class Fixture (see https://xunit.github.io/docs/shared-context.html).
    /// As such, it is instantiated per-class, which is the behavior needed here to ensure
    /// each test class has its own web server, as opposed to Collection Fixtures even though
    /// there is one assembly-wide collection used for serialization purposes.
    /// </remarks>
    public class WebHostTestFixture<TTest> : IDisposable, IWebHostTestFixture where TTest: class
    {
        private static readonly string NormalBaseAddressTemplate = "http://{0}:{1}";

        private int _port;
        private bool disposedValue = false;

        private IHost _hostedService = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostTestFixture{TTest}"/> class
        /// </summary>
        public WebHostTestFixture()
        {
            Initialize();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WebHostTestFixture{TTest}"/> class.
        /// </summary>
        ~WebHostTestFixture()
        {
            Dispose(false);
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether error details should be included.
        /// </summary>
        public bool IncludeErrorDetail { get; set; } = true;

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <summary>
        /// Cleanup the server.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_hostedService != null)
                    {
                        _hostedService.StopAsync();
                        _hostedService.WaitForShutdownAsync();
                    }
                }

                disposedValue = true;
            }
        }

        private Action<IServiceCollection> GetConfigureServicesMethod()
        {
            Type testType = typeof(TTest);

            MethodInfo method  = testType.GetMethod("UpdateConfigureServices",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new Type[] { typeof(IServiceCollection) },
                null);

            return method == null ? null : (Action<IServiceCollection>)Delegate.CreateDelegate(typeof(Action<IServiceCollection>), method);
        }

        private Action<IApplicationBuilder/*, IWebHostEnvironment*/> GetConfigurationMethod()
        {
            Type testType = typeof(TTest);

            MethodInfo method = testType.GetMethod("UpdateConfigure",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                new Type[] { typeof(IApplicationBuilder)/*, typeof(IWebHostEnvironment)*/ },
                null);

            return method == null ? null : (Action<IApplicationBuilder/*, IWebHostEnvironment*/>)Delegate.CreateDelegate(typeof(Action<IApplicationBuilder/*, IWebHostEnvironment*/>), method);
        }

        /// <summary>
        /// Initialize the fixture.
        /// </summary>
        /// <returns>true of the server is initialized, false otherwise.</returns>
        /// <remarks>
        /// This is done lazily to allow the update configuration
        /// function to be passed in from the first test class instance.
        /// </remarks>
        private void Initialize()
        {
            var configServicesDelete = GetConfigureServicesMethod();

            var configApps = GetConfigurationMethod();

            string serverName = "localhost";

            // setup base address
            _port = PortArranger.Reserve();
            this.BaseAddress = string.Format(NormalBaseAddressTemplate, serverName, _port.ToString());

            _hostedService = Host.CreateDefaultBuilder()
               .ConfigureAppConfiguration((hostingContext, config) =>
               {
               })
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder
                        .UseKestrel(options =>
                        {
                            options.Listen(IPAddress.Loopback, _port);
                        })
                        //.UseStartup<TTest>()
                       .ConfigureServices(services =>
                        {
                            // Add ourself to the container so WebHostTestStartup
                            // can call UpdateConfiguration.
                            // services.AddSingleton<IWebHostTestFixture>(this);

                            configServicesDelete?.Invoke(services);
                        })
                       .Configure(app =>
                       {
                           configApps?.Invoke(app);
                       })
                       .ConfigureLogging((hostingContext, logging) =>
                       {
                           //logging.AddDebug();
                           //logging.SetMinimumLevel(LogLevel.Warning);
                       });
               })
               .Build();

            _hostedService.StartAsync();
        }
    }
}
