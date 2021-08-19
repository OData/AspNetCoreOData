//-----------------------------------------------------------------------------
// <copyright file="ODataControllerActionContextTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class ODataControllerActionContextTests
    {
        [Fact]
        public void CtorODataControllerActionContext_ThrowsArgumentNull()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => new ODataControllerActionContext(null, null, null), "prefix");

            // Arrange & Act & Assert
            string prefix = "odata";
            ExceptionAssert.ThrowsArgumentNull(() => new ODataControllerActionContext(prefix, null, null), "model");

            // Arrange & Act & Assert
            IEdmModel model = new Mock<IEdmModel>().Object;
            ExceptionAssert.ThrowsArgumentNull(() => new ODataControllerActionContext(prefix, model, null), "controller");
        }

        [Fact]
        public void CtorODataControllerActionContext_SetProperties()
        {
            // Arrange & Act
            string prefix = "odata";
            IEdmModel model = new Mock<IEdmModel>().Object;
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmSingleton singleton = container.AddSingleton("Me", entityType);
            ControllerModel controller = new ControllerModel(typeof(TestController).GetTypeInfo(), new List<object>());
            ODataControllerActionContext context = new ODataControllerActionContext(prefix, model, controller)
            {
                NavigationSource = singleton
            };

            // Assert
            Assert.Equal(prefix, context.Prefix);
            Assert.Same(model, context.Model);
            Assert.Same(controller, context.Controller);

            Assert.Null(context.EntitySet);
            Assert.Same(singleton, context.Singleton);
            Assert.Same(singleton, context.NavigationSource);
            Assert.Same(entityType, context.EntityType);

            Assert.NotNull(context.Options);
        }

        private class TestController
        {
        }
    }
}
