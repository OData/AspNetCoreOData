//-----------------------------------------------------------------------------
// <copyright file="WebApiTestBaseOfT.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using Xunit;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    /// <summary>
    /// The WebApiTestBase used for the test base class.
    /// </summary>
    public abstract class WebApiTestBase<TTest> : IClassFixture<WebApiTestFixture<TTest>> where TTest : class
    {
        private WebApiTestFixture<TTest> _fixture;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebODataTestBase{TStartup}"/> class.
        /// </summary>
        /// <param name="fixture">The factory used to initialize the web service client.</param>
        protected WebApiTestBase(WebApiTestFixture<TTest> fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Create the httpClient
        /// </summary>
        /// <returns></returns>
        public virtual HttpClient CreateClient() => _fixture.CreateClient();
    }
}
