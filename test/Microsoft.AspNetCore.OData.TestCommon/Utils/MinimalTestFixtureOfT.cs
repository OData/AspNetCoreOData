//-----------------------------------------------------------------------------
// <copyright file="MinimalTestFixtureOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.TestCommon;

public class MinimalTestFixture<T> : IDisposable where T : class
{
    /// <summary>
    /// The test server.
    /// </summary>
    private TestServer _server;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApiTestFixture{T}"/> class
    /// </summary>
    public MinimalTestFixture()
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

            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Initialize the fixture.
    /// </summary>
    private async void Initialize()
    {
        Type testType = typeof(T);

        // Be noted:
        // We use the convention as follows
        // 1) if you want to configure the service, add "protected static void ConfigureServices(IServiceCollection)" method into your test class.
        MethodInfo configureServicesMethod = testType.GetMethod("ConfigureServices", BindingFlags.NonPublic | BindingFlags.Static);

        // 2) if you want to configure the routing, add "protected static void ConfigureAPIs(WebApplication)" method into your test class.
        MethodInfo configureMethod = testType.GetMethod("ConfigureAPIs", BindingFlags.NonPublic | BindingFlags.Static);
        // Owing that this is used in Test only, I assume every developer can following the convention.
        // So I skip the method parameter checking.

        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        configureServicesMethod?.Invoke(null, [builder.Services]);

        var app = builder.Build();

        _server = (TestServer)app.Services.GetRequiredService<IServer>();

        configureMethod?.Invoke(null, [app]);

        await app.RunAsync();
    }
}
