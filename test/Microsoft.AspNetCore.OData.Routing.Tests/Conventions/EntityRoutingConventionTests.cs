// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
{
    public class EntityRoutingConventionTests
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
            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool actual = entityConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("Put")]
        [InlineData("Patch")]
        [InlineData("Delete")]
        public void AppliesToActionForBasicEntityActionWorksAsExpected(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool returnValue = entityConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Equal("Customers(FirstName={keyFirstName},LastName={keyLastName})", selector.AttributeRouteModel.Template);
        }

        [Theory]
        [InlineData("GetCustomer")]
        [InlineData("PutCustomer")]
        [InlineData("PatchCustomer")]
        [InlineData("DeleteCustomer")]
        public void AppliesToActionForEntityActionWithEntityTypeNameSameAsEntityTypeOnEntitySetWorksAsExpected(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool returnValue = entityConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Equal(2, action.Selectors.Count);
            Assert.Equal(new[]
                {
                    "Customers(FirstName={keyFirstName},LastName={keyLastName})",
                    "Customers(FirstName={keyFirstName},LastName={keyLastName})/NS.Customer"
                },
                action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("GetVipCustomer")]
        [InlineData("PutVipCustomer")]
        [InlineData("PatchVipCustomer")]
        [InlineData("DeleteVipCustomer")]
        public void AppliesToActionForEntityActionWithDerivedEntityTypeWorksAsExpected(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool returnValue = entityConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Equal("Customers(FirstName={keyFirstName},LastName={keyLastName})/NS.VipCustomer", selector.AttributeRouteModel.Template);
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("Get")]
        public void AppliesToActionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<AnotherCustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool returnValue = entityConvention.AppliesToAction(context);
            Assert.False(returnValue);

            // Assert
            Assert.Empty(action.Selectors);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("FirstName", EdmPrimitiveTypeKind.String),
                customer.AddStructuralProperty("LastName", EdmPrimitiveTypeKind.String));
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
            #region Basic Action Names
            public void Get(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }

            public void Put(string keyLastName, string keyFirstName)
            { }

            public void Patch(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }

            public void Delete(string keyLastName, string keyFirstName)
            { }
            #endregion

            #region Action Name with EntityType of entity set
            public void GetCustomer(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }

            public void PutCustomer(string keyLastName, string keyFirstName)
            { }

            public void PatchCustomer(string keyLastName, string keyFirstName)
            { }

            public void DeleteCustomer(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }

            #endregion

            #region Action Name with derived entity type
            public void GetVipCustomer(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }

            public void PutVipCustomer(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }

            public void PatchVipCustomer(CancellationToken cancellation, string keyLastName, string keyFirstName)
            { }

            public void DeleteVipCustomer(string keyFirstName, string keyLastName, CancellationToken cancellation)
            { }
            #endregion
        }

        private class AnotherCustomersController
        {
            public void Post(string keyLastName, string keyFirstName)
            { }

            public void Get(int key)
            { }
        }

        private class UnknownController
        { }
    }
}