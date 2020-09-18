// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    public class MockServiceProvider : IServiceProvider
    {
        private IServiceProvider _rootContainer;

        public MockServiceProvider()
        {
            // TODO: to build the default sp.
            _rootContainer = BuilderDefaultServiceProvider(null);
        }

        public MockServiceProvider(IServiceProvider sp)
        {
            _rootContainer = sp;
        }

        public MockServiceProvider(Action<IContainerBuilder> setupAction)
        {
            _rootContainer = BuilderDefaultServiceProvider(setupAction);
        }

        public object GetService(Type serviceType)
        {
            return _rootContainer?.GetService(serviceType);
        }

        private static IServiceProvider BuilderDefaultServiceProvider(Action<IContainerBuilder> setupAction)
        {
            IContainerBuilder odataContainerBuilder = new DefaultContainerBuilder();

            odataContainerBuilder.AddDefaultODataServices();

            // Inject the default Web API OData services.
            odataContainerBuilder.AddDefaultWebApiServices();

            // Inject the customized services.
            setupAction?.Invoke(odataContainerBuilder);

            return odataContainerBuilder.BuildContainer();
        }
    }
}
