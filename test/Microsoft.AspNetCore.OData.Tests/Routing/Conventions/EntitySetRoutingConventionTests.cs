//-----------------------------------------------------------------------------
// <copyright file="EntitySetRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class EntitySetRoutingConventionTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

        [Fact]
        public void AppliesToControllerAndActionOnEntitySetRoutingConvention_Throws_Context()
        {
            // Arrange
            EntitySetRoutingConvention convention = new EntitySetRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => convention.AppliesToController(null), "context");
            ExceptionAssert.ThrowsArgumentNull(() => convention.AppliesToAction(null), "context");
        }

        [Theory]
        [InlineData(typeof(CustomersController), true)]
        [InlineData(typeof(AnotherCustomersController), true)]
        [InlineData(typeof(UnknownController), false)]
        public void AppliesToControllerReturnsExpectedForController(Type controllerType, bool expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType);
            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            EntitySetRoutingConvention entitySetConvention = ConventionHelpers.CreateConvention<EntitySetRoutingConvention>();

            // Act
            bool actual = entitySetConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Get", "Customers")]
        [InlineData("GetCustomersFromVipCustomer", "Customers/NS.VipCustomer")]
        public void AppliesToActionForGetActionWorksAsExpected(string actionName, string expectedTemplate)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            EntitySetRoutingConvention entitySetConvention = ConventionHelpers.CreateConvention<EntitySetRoutingConvention>();

            // Act
            bool returnValue = entitySetConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Equal(2, action.Selectors.Count);
            Assert.Equal(new[]
                {
                    $"/{expectedTemplate}",
                    $"/{expectedTemplate}/$count"
                },
                action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("Post", "/Customers")]
        [InlineData("PostFromVipCustomer", "/Customers/NS.VipCustomer")]
        public void AppliesToActionForPostActionWorksAsExpected(string actionName, string expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            EntitySetRoutingConvention entitySetConvention = ConventionHelpers.CreateConvention<EntitySetRoutingConvention>();

            // Act
            bool returnValue = entitySetConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Equal(expected, selector.AttributeRouteModel.Template);
        }

        [Theory]
        [InlineData("Patch", "/Customers")]
        [InlineData("PatchCustomers", "/Customers")]
        public void AppliesToAction_Works_ForPatchActionWorksAsExpected(string actionName, string expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            EntitySetRoutingConvention entitySetConvention = ConventionHelpers.CreateConvention<EntitySetRoutingConvention>();

            // Act
            bool returnValue = entitySetConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Equal(expected, selector.AttributeRouteModel.Template);
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("PostTo")]
        [InlineData("GetFrom")]
        [InlineData("PostFrom")]
        [InlineData("PatchFrom")]
        [InlineData("GetAnotherCustomersFrom")]
        [InlineData("PatchAnotherCustomersFrom")]
        [InlineData("PostAnotherCustomerFrom")]
        public void AppliesToActionDoesNothingForNonConventionAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<AnotherCustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            EntitySetRoutingConvention entitySetConvention = ConventionHelpers.CreateConvention<EntitySetRoutingConvention>();

            // Act
            entitySetConvention.AppliesToAction(context);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Null(selector.AttributeRouteModel);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);

            // VipCustomer
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            container.AddEntitySet("Customers", customer);
            container.AddEntitySet("AnotherCustomers", customer);
            model.AddElement(container);
            return model;
        }

        private class CustomersController
        {
            public void Get()
            { }

            public void GetCustomersFromVipCustomer()
            { }

            public void Post()
            { }

            public void PostFromVipCustomer()
            { }

            public void Patch()
            { }

            public void PatchCustomers()
            { }
        }

        private class AnotherCustomersController
        {
            public void Get(int key)
            { }

            public void PostTo()
            { }

            public void GetFrom()
            {
            }

            public void PostFrom()
            {
            }

            public void PatchFrom()
            {
            }

            public void GetAnotherCustomersFrom()
            {
            }

            public void PatchAnotherCustomersFrom()
            {
            }

            public void PostAnotherCustomerFrom()
            {
            }
        }

        private class UnknownController
        { }
    }
}
