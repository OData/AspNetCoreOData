// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests
{
    public class ODataServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddODataCore_ThrowsArgumentNull_Services()
        {
            // Arrange
            IServiceCollection services = null;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => services.AddODataCore(), "services");
        }
    }
}
