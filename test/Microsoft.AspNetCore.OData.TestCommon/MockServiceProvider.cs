//-----------------------------------------------------------------------------
// <copyright file="MockServiceProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    /// <summary>
    /// A mock to represent a service provider.
    /// </summary>
    public class MockServiceProvider : IServiceProvider
    {
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockServiceProvider"/> class.
        /// </summary>
        public MockServiceProvider()
        {
            _serviceProvider = BuilderDefaultServiceProvider(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockServiceProvider"/> class.
        /// </summary>
        /// <param name="sp">The input service provider.</param>
        public MockServiceProvider(IServiceProvider sp)
        {
            _serviceProvider = sp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockServiceProvider"/> class.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public MockServiceProvider(IEdmModel model)
        {
            _serviceProvider = BuilderDefaultServiceProvider(b => b.AddService(ServiceLifetime.Singleton, sp => model));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockServiceProvider"/> class.
        /// </summary>
        /// <param name="setupAction">The setup action.</param>
        public MockServiceProvider(Action<IContainerBuilder> setupAction)
        {
            _serviceProvider = BuilderDefaultServiceProvider(setupAction);
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider?.GetService(serviceType);
        }

        private static IServiceProvider BuilderDefaultServiceProvider(Action<IContainerBuilder> setupAction)
        {
            IContainerBuilder odataContainerBuilder = new DefaultContainerBuilder();

            odataContainerBuilder.AddDefaultODataServices();

            odataContainerBuilder.AddService(ServiceLifetime.Singleton, sp => new DefaultQuerySettings());

            odataContainerBuilder.AddService(ServiceLifetime.Singleton, typeof(ODataUriResolver),
                sp => new UnqualifiedODataUriResolver { EnableCaseInsensitive = true });

            // Inject the default Web API OData services.
            odataContainerBuilder.AddDefaultWebApiServices();

            // Inject the customized services.
            setupAction?.Invoke(odataContainerBuilder);

            return odataContainerBuilder.BuildContainer();
        }
    }
}
