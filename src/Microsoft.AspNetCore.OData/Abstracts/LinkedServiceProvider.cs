// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// 
    /// </summary>
    public class LinkedServiceProvider : IServiceProvider
    {

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public List<IServiceProvider> ServiceProviders { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProviders"></param>
        public LinkedServiceProvider(params IServiceProvider[] serviceProviders)
        {
            ServiceProviders = new List<IServiceProvider>();
            //RWM: Checking for nulls to account for setup issues in unit tests.
            foreach (var provider in serviceProviders)
            {
                if (provider == null)
                {
                    Trace.TraceWarning("One of the ServiceProviders registered to the LinkedServiceProvider was null. Please check your configuration.");
                }
                else
                {
                    ServiceProviders.Add(provider);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>() where T : class
        {
            foreach (var provider in ServiceProviders)
            {
                var service = provider.GetService<T>();
                if (service is not null) return service;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetServices<T>() where T : class
        {
            var services = new List<T>();
            foreach (var provider in ServiceProviders)
            {
                services.AddRange(provider.GetServices<T>());
            }
            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            foreach (var provider in ServiceProviders)
            {
                var service = provider.GetService(serviceType);
                if (service is not null) return service;
            }
            return null;
        }

        #endregion

    }

}
