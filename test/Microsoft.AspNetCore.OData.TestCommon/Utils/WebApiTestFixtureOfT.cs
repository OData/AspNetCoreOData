//-----------------------------------------------------------------------------
// <copyright file="WebApiTestFixtureOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace Microsoft.AspNetCore.OData.TestCommon;

public class WebApiTestFixture<T> : IDisposable where T : class
{
    /// <summary>
    /// The test server.
    /// </summary>
    private TestServer _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApiTestFixture{T}"/> class
    /// </summary>
    public WebApiTestFixture()
    {
        Initialize();
    }

    /// <summary>
    /// Create the <see cref="HttpClient"/>.
    /// </summary>
    /// <returns>The created HttpClient.</returns>
    public HttpClient CreateClient()
    {
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
    private void Initialize()
    {
        Type testType = typeof(T);
        // Be noted:
        // We use the convention as follows
        // 1) if you want to configure the service, add "protected static void UpdateConfigureServices(IServiceCollection)" method into your test class.
        MethodInfo configureServicesMethod = testType.GetMethod("UpdateConfigureServices", BindingFlags.NonPublic | BindingFlags.Static);

        // 2) if you want to configure the routing, add "protected static void updateConfigure(IApplicationBuilder)" method into your test class.
        MethodInfo configureMethod = testType.GetMethod("UpdateConfigure", BindingFlags.NonPublic | BindingFlags.Static);
        // Owing that this is used in Test only, I assume every developer can following the convention.
        // So I skip the method parameter checking.

        var builder = new WebHostBuilder()
           .ConfigureServices(services =>
           {
               configureServicesMethod?.Invoke(null, new object[] { services });
           })
           .Configure(app =>
           {
               if (configureMethod == null)
               {
                   app.UseRouting();
                   app.UseEndpoints(endpoints =>
                   {
                       endpoints.MapControllers();
                   });
               }
               else
               {
                   configureMethod.Invoke(null, new object[] { app });
               }
           });

        _server = new TestServer(builder);
    }
}
