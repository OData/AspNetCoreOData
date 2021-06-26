// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Extensions
{
    public class ActionDescriptorExtensionsTests
    {
        [Fact]
        public void GetEdmModel_ThrowsArgumentNull_ActionDescriptor()
        {
            // Arrange & Act & Assert
            ActionDescriptor actionDescriptor = null;
            ExceptionAssert.ThrowsArgumentNull(() => actionDescriptor.GetEdmModel(null, null), "actionDescriptor");
        }

        [Fact]
        public void GetEdmModel_ThrowsArgumentNull_Request()
        {
            // Arrange & Act & Assert
            ActionDescriptor actionDescriptor = new Mock<ActionDescriptor>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => actionDescriptor.GetEdmModel(null, null), "request");
        }

        [Fact]
        public void GetEdmModel_ThrowsArgumentNull_EntityClrType()
        {
            // Arrange & Act & Assert
            ActionDescriptor actionDescriptor = new Mock<ActionDescriptor>().Object;
            HttpRequest request = new Mock<HttpRequest>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => actionDescriptor.GetEdmModel(request, null), "entityClrType");
        }

        [Fact]
        public void GetEdmModel_Returns_CachedModel()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            ActionDescriptor actionDescriptor = new ActionDescriptor();
            string key = "Microsoft.AspNetCore.OData.Model+" + typeof(ActionCustomer).FullName;
            actionDescriptor.Properties.Add(key, model);
            HttpRequest request = new Mock<HttpRequest>().Object;

            // Act
            IEdmModel actual = actionDescriptor.GetEdmModel(request, typeof(ActionCustomer));

            // Assert
            Assert.Same(model, actual);
        }

        [Fact]
        public void GetEdmModel_Returns_BuiltEdmModel()
        {
            // Arrange
            string key = "Microsoft.AspNetCore.OData.Model+" + typeof(ActionCustomer).FullName;
            ActionDescriptor actionDescriptor = new ActionDescriptor();
            Assert.False(actionDescriptor.Properties.TryGetValue(key, out _)); // Guard

            HttpContext context = new DefaultHttpContext();
            HttpRequest request = context.Request;
            IServiceCollection services = new ServiceCollection();
            context.RequestServices = services.BuildServiceProvider();

            // Act
            IEdmModel actual = actionDescriptor.GetEdmModel(request, typeof(ActionCustomer));

            // Assert
            Assert.NotNull(actual);
            actionDescriptor.Properties.TryGetValue(key, out object cachedModel);

            Assert.Same(cachedModel, actual);
        }

        private class ActionCustomer
        {
            public int Id { get; set; }
        }
    }
}