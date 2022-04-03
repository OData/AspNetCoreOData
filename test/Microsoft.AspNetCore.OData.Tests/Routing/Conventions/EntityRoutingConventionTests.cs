//-----------------------------------------------------------------------------
// <copyright file="EntityRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class EntityRoutingConventionTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

        [Fact]
        public void AppliesToControllerAndActionOnEntityRoutingConvention_Throws_Context()
        {
            // Arrange
            EntityRoutingConvention convention = new EntityRoutingConvention();

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
            Assert.Equal(2, action.Selectors.Count);
            Assert.Collection(action.Selectors,
                e =>
                {
                    Assert.Equal("/Customers(FirstName={keyFirstName},LastName={keyLastName})", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/Customers/FirstName={keyFirstName},LastName={keyLastName}", e.AttributeRouteModel.Template);
                });
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
            Assert.Equal(4, action.Selectors.Count);
            Assert.Collection(action.Selectors,
                e =>
                {
                    Assert.Equal("/Customers(FirstName={keyFirstName},LastName={keyLastName})", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/Customers/FirstName={keyFirstName},LastName={keyLastName}", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/Customers(FirstName={keyFirstName},LastName={keyLastName})/NS.Customer", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/Customers/FirstName={keyFirstName},LastName={keyLastName}/NS.Customer", e.AttributeRouteModel.Template);
                });
        }
        
        [Fact]
        public void AppliesToActionForEntityActionWithEntityTypeNameSameAsEntityTypeOnEntitySetWorksAndCaseInsensitiveAsExpected()
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CaseInsensitiveCustomersController>("GetCUSTOMER");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;
            context.Options.RouteOptions.EnableActionNameCaseInsensitive = true;

            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool returnValue = entityConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Equal(4, action.Selectors.Count);
            Assert.Collection(action.Selectors,
                e =>
                {
                    Assert.Equal("/CaseInsensitiveCustomers(FirstName={keyFirstName},LastName={keyLastName})", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/CaseInsensitiveCustomers/FirstName={keyFirstName},LastName={keyLastName}", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/CaseInsensitiveCustomers(FirstName={keyFirstName},LastName={keyLastName})/NS.Customer", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/CaseInsensitiveCustomers/FirstName={keyFirstName},LastName={keyLastName}/NS.Customer", e.AttributeRouteModel.Template);
                });
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
            Assert.Equal(2, action.Selectors.Count);
            Assert.Collection(action.Selectors,
                e =>
                {
                    Assert.Equal("/Customers(FirstName={keyFirstName},LastName={keyLastName})/NS.VipCustomer", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/Customers/FirstName={keyFirstName},LastName={keyLastName}/NS.VipCustomer", e.AttributeRouteModel.Template);
                });
        }
        
        [Fact]
        public void AppliesToActionForEntityActionWithDerivedEntityTypeAndCaseInsensitiveWorksAsExpected()
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CaseInsensitiveCustomersController>("GetVIPCUSTOMER");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;
            context.Options.RouteOptions.EnableActionNameCaseInsensitive = true;

            EntityRoutingConvention entityConvention = ConventionHelpers.CreateConvention<EntityRoutingConvention>();

            // Act
            bool returnValue = entityConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Equal(2, action.Selectors.Count);
            Assert.Collection(action.Selectors,
                e =>
                {
                    Assert.Equal("/CaseInsensitiveCustomers(FirstName={keyFirstName},LastName={keyLastName})/NS.VipCustomer", e.AttributeRouteModel.Template);
                },
                e =>
                {
                    Assert.Equal("/CaseInsensitiveCustomers/FirstName={keyFirstName},LastName={keyLastName}/NS.VipCustomer", e.AttributeRouteModel.Template);
                });
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
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Null(selector.AttributeRouteModel);
        }

        #region Split Tests
        [Theory]
        [InlineData("Post", null, null)]
        [InlineData("Get", "Get", null)]
        [InlineData("GetVipCustomer", "Get", "VipCustomer")]
        [InlineData("Put", "Put", null)]
        [InlineData("PutVipCustomer", "Put", "VipCustomer")]
        [InlineData("Patch", "Patch", null)]
        [InlineData("PatchVipCustomer", "Patch", "VipCustomer")]
        [InlineData("Delete", "Delete", null)]
        [InlineData("DeleteVipCustomer", "Delete", "VipCustomer")]
        public void SplitActionNameForEntity_Works(string actionName, string expectMethod, string expectCast)
        {
            // Arrange
            (string httpMethod, string castTypeName) = EntityRoutingConvention.Split(actionName);

            // Act & Assert
            Assert.Equal(expectMethod, httpMethod);
            Assert.Equal(expectCast, castTypeName);
        }
        #endregion

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
            container.AddEntitySet("CaseInsensitiveCustomers", customer);
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
        
        private class CaseInsensitiveCustomersController
        {
            #region Action Name with EntityType of entity set
            public void GetCUSTOMER(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }
            #endregion

            #region Action Name with derived entity type
            public void GetVIPCUSTOMER(string keyLastName, string keyFirstName, CancellationToken cancellation)
            { }
            #endregion
        }        

        private class UnknownController
        { }
    }
}
