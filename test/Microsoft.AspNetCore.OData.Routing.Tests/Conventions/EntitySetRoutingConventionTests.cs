// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
{
    public class EntitySetRoutingConventionTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

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
                    $"{expectedTemplate}",
                    $"{expectedTemplate}/$count"
                },
                action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("Post", "Customers")]
        [InlineData("PostFromVipCustomer", "Customers/NS.VipCustomer")]
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
        [InlineData("Get")]
        [InlineData("PostTo")]
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
            Assert.Empty(action.Selectors);
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
        }

        private class AnotherCustomersController
        {
            public void Get(int key)
            { }

            public void PostTo()
            { }
        }

        private class UnknownController
        { }
    }
}