// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    public class IServiceCollectionExtensionsTests
    {

        [Fact]
        public void AddODataCore_ThrowsArgumentNull_Services()
        {
            // Arrange & Act & Assert
            IServiceCollection services = null;
            ExceptionAssert.ThrowsArgumentNull(() => services.AddODataCore(), "services");
        }

        [Fact]
        public void AddODataDefaultServices_ThrowsArgumentNull_Services()
        {
            // Arrange & Act & Assert
            IServiceCollection services = null;
            ExceptionAssert.ThrowsArgumentNull(() => services.AddODataDefaultServices(), "services");
        }

        [Fact]
        public void AddODataWebApiServices_ThrowsArgumentNull_Services()
        {
            // Arrange & Act & Assert
            IServiceCollection services = null;
            ExceptionAssert.ThrowsArgumentNull(() => services.AddODataWebApiServices(), "services");
        }

        [Fact]
        public void AddScopedODataServices_ThrowsArgumentNull_Services()
        {
            // Arrange & Act & Assert
            IServiceCollection services = null;
            ExceptionAssert.ThrowsArgumentNull(() => services.AddScopedODataServices(), "services");
        }

    }

}
