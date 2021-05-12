// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Abstracts
{
    public class DefaultODataBuilderTests
    {
        [Fact]
        public void CtorThrowsArgumentNullServices()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new DefaultODataBuilder(null), "services");
        }

        [Fact]
        public void Ctor_SetServices()
        {
            // Arrange
            var services = new Mock<IServiceCollection>();

            // Act
            DefaultODataBuilder builder = new DefaultODataBuilder(services.Object);

            // Assert
            Assert.Same(services.Object, builder.Services);
        }
    }
}
