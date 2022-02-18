//-----------------------------------------------------------------------------
// <copyright file="WebHostTestBaseOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.Commons
{
    /// <summary>
    /// The WebHostTestBase creates a web host to be used for a test.
    /// </summary>
    public abstract class WebHostTestBase<TTest> : IClassFixture<WebHostTestFixture<TTest>>, IDisposable where TTest: class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostTestBase{TTest}"/> class.
        /// </summary>
        /// <param name="fixture">The fixture used to initialize the web service.</param>
        protected WebHostTestBase(WebHostTestFixture<TTest> fixture)
        {
            this.BaseAddress = fixture.BaseAddress;
            this.Client = new HttpClient();
        }

        /// <summary>
        /// The base address of the server.
        /// </summary>
        public string BaseAddress { get; private set; }

        /// <summary>
        /// An HttpClient to use with the server.
        /// </summary>
        public HttpClient Client { get; set; }

        public void Dispose()
        {
            if (Client != null)
            {
                Client.Dispose();
            }

            Client = null;
        }
    }
}
