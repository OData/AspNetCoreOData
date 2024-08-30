//-----------------------------------------------------------------------------
// <copyright file="WebODataTestBaseOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using Xunit;

namespace Microsoft.AspNetCore.OData.TestCommon;

/// <summary>
/// The WebODataTestBase used for the test base class.
/// </summary>
public abstract class WebODataTestBase<TStartup> : IClassFixture<WebODataTestFixture<TStartup>> where TStartup : class
{
    private HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebODataTestBase{TStartup}"/> class.
    /// </summary>
    /// <param name="factory">The factory used to initialize the web service client.</param>
    protected WebODataTestBase(WebODataTestFixture<TStartup> factory)
    {
        Factory = factory ?? throw new ArgumentNullException(nameof(factory));
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
                _client.Timeout = TimeSpan.FromSeconds(3600);
            }

            return _client;
        }
    }

    /// <summary>
    /// Gets the factory.
    /// </summary>
    public WebODataTestFixture<TStartup> Factory { get; }
}
