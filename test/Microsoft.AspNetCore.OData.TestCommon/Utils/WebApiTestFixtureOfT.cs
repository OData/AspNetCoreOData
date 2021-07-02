// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Microsoft.AspNetCore.OData.TestCommon
{

    public class WebApiTestFixture<T> : IDisposable where T : class
    {

        #region Private Members

        /// <summary>
        /// The test server.
        /// </summary>
        private TestServer _server;

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public Action<IServiceCollection> ConfigureServicesAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Action<IApplicationBuilder> ConfigureAction { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiTestFixture{T}"/> class
        /// </summary>
        public WebApiTestFixture()
        {
        }

        #endregion

        /// <summary>
        /// Create the <see cref="HttpClient"/>.
        /// </summary>
        /// <returns>The created HttpClient.</returns>
        public HttpClient CreateClient()
        {
            EnsureInitialized();
            return _server.CreateClient();
        }

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
            if (disposing)
            {
                if (_server != null)
                {
                    _server.Dispose();
                    _server = null;
                }
            }
        }

        /// <summary>
        /// Initialize the fixture.
        /// </summary>
        private void EnsureInitialized()
        {
            //Type testType = typeof(T);
            //// Be noted:
            //// We use the convention as follows
            //// 1) if you want to configure the service, add "protected static void ConfigureServicesAction(IServiceCollection)" method into your test class.
            //MethodInfo configureServicesMethod = testType.GetMethod("ConfigureServicesAction", BindingFlags.NonPublic | BindingFlags.Static);

            //// 2) if you want to configure the routing, add "protected static void updateConfigure(IApplicationBuilder)" method into your test class.
            //MethodInfo configureMethod = testType.GetMethod("UpdateConfigure", BindingFlags.NonPublic | BindingFlags.Static);
            //// Owing that this is used in Test only, I assume every developer can following the convention.
            //// So I skip the method parameter checking.

            if (_server != null) return;

            var builder = new WebHostBuilder()
               .ConfigureServices(services =>
               {
                   ConfigureServicesAction?.Invoke(services);
               })
               .Configure(app =>
               {
                   if (ConfigureAction == null)
                   {
                       app.UseRouting();
                       app.UseEndpoints(endpoints =>
                       {
                           endpoints.MapControllers();
                       });
                   }
                   else
                   {
                       ConfigureAction.Invoke(app);
                   }
               });

            _server = new TestServer(builder);
        }

    }

}
