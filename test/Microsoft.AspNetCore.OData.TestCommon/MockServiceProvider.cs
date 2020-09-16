// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.TestCommon
{
    public class MockServiceProvider : IServiceProvider
    {
        private IServiceProvider _rootContainer;

        public MockServiceProvider()
        {
            // TODO: to build the default sp.
            _rootContainer = BuilderDefaultServiceProvider();
        }

        public MockServiceProvider(IServiceProvider sp)
        {
            _rootContainer = sp;
        }

        public object GetService(Type serviceType)
        {
            return _rootContainer?.GetService(serviceType);
        }

        private static IServiceProvider BuilderDefaultServiceProvider()
        {
            IServiceCollection services = new ServiceCollection();
            return services.BuildServiceProvider();
        }
    }
}
