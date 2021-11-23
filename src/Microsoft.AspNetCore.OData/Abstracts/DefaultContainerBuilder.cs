//-----------------------------------------------------------------------------
// <copyright file="DefaultContainerBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// The default container builder implementation based on the Microsoft dependency injection framework.
    /// </summary>
    internal class DefaultContainerBuilder : IContainerBuilder
    {
        internal IServiceCollection Services { get; } = new ServiceCollection();

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationType">The implementation type of the service.</param>
        /// <returns>The <see cref="IContainerBuilder"/> instance itself.</returns>
        public virtual IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime,
            Type serviceType,
            Type implementationType)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw Error.ArgumentNull(nameof(implementationType));
            }

            Services.Add(new ServiceDescriptor(
                serviceType, implementationType, TranslateServiceLifetime(lifetime)));

            return this;
        }

        /// <summary>
        /// Adds a service of <paramref name="serviceType"/> with an <paramref name="implementationFactory"/>.
        /// </summary>
        /// <param name="lifetime">The lifetime of the service to register.</param>
        /// <param name="serviceType">The type of the service to register.</param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <returns>The <see cref="Microsoft.OData.IContainerBuilder"/> instance itself.</returns>
        public virtual IContainerBuilder AddService(Microsoft.OData.ServiceLifetime lifetime,
            Type serviceType,
            Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType == null)
            {
                throw Error.ArgumentNull(nameof(serviceType));
            }

            if (implementationFactory == null)
            {
                throw Error.ArgumentNull(nameof(implementationFactory));
            }

            Services.Add(new ServiceDescriptor(
                serviceType, implementationFactory, TranslateServiceLifetime(lifetime)));

            return this;
        }

        /// <summary>
        /// Builds a container which implements <see cref="IServiceProvider"/> and contains
        /// all the services registered.
        /// </summary>
        /// <returns>The container built by this builder.</returns>
        public virtual IServiceProvider BuildContainer()
        {
            return Services.BuildServiceProvider();
        }

        private static Microsoft.Extensions.DependencyInjection.ServiceLifetime TranslateServiceLifetime(
            Microsoft.OData.ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case Microsoft.OData.ServiceLifetime.Scoped:
                    return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped;

                case Microsoft.OData.ServiceLifetime.Singleton:
                    return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton;

                default:
                    return Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient;
            }
        }
    }
}
