// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.OData.TestCommon
{

    /// <summary>
    /// The WebApiTestBase used for the test base class.
    /// </summary>
    public abstract class WebApiTestBase<TTest> : IClassFixture<WebApiTestFixture<TTest>> where TTest : class
    {

        private readonly WebApiTestFixture<TTest> _fixture;
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebODataTestBase{TStartup}"/> class.
        /// </summary>
        /// <param name="fixture">The factory used to initialize the web service client.</param>
        protected WebApiTestBase(WebApiTestFixture<TTest> fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        /// <summary>
        /// 
        /// </summary>
        public Action<IServiceCollection> ConfigureServicesAction
        
        { 
            get => _fixture.ConfigureServicesAction;
            set => _fixture.ConfigureServicesAction = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public Action<IApplicationBuilder> ConfigureAction 
        {
            get => _fixture.ConfigureAction;
            set => _fixture.ConfigureAction = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public ITestOutputHelper Output { get => _output; }

        /// <summary>
        /// Create the httpClient
        /// </summary>
        /// <returns></returns>
        public virtual HttpClient CreateClient() => _fixture.CreateClient();

    }

}
