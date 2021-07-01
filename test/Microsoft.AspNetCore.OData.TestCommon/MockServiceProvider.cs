// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;

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
            _serviceProvider = BuilderDefaultServiceProvider(services => services.AddSingleton(sp => model));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MockServiceProvider"/> class.
        /// </summary>
        /// <param name="setupAction">The setup action.</param>
        public MockServiceProvider(Action<IServiceCollection> setupAction)
        {
            _serviceProvider = BuilderDefaultServiceProvider(setupAction);
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider?.GetService(serviceType);
        }

        private static IServiceProvider BuilderDefaultServiceProvider(Action<IServiceCollection> setupAction)
        {
            ServiceCollection services = new ServiceCollection();

            services
                .AddODataCore()
                .AddODataDefaultServices()
                .AddODataWebApiServices()
                .AddScopedODataServices();

            // Inject the customized services.
            setupAction?.Invoke(services);

            return services.BuildServiceProvider();
        }
    }
}
