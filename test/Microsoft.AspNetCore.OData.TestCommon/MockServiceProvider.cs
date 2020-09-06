// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData.Tests.Commons
{
    public class MockServiceProvider : IServiceProvider
    {
        private IServiceProvider _rootContainer;

        public object GetService(Type serviceType)
        {
            return _rootContainer.GetService(serviceType);
        }
    }
}
