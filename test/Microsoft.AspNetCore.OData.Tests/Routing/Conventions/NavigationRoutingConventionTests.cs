// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class NavigationRoutingConventionTests
    {
        private static NavigationRoutingConvention NavigationConvention = ConventionHelpers.CreateConvention<NavigationRoutingConvention>();
        private static IEdmModel EdmModel = GetEdmModel();

        [Theory]
        [InlineData(typeof(CustomersController), true)]
        [InlineData(typeof(MeController), true)]
        [InlineData(typeof(UnknownController), false)]
        public void AppliesToControllerReturnsExpectedForController(Type controllerType, bool expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType);
            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);

            // Act
            bool actual = NavigationConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryDataSet<Type, string, string[]> NavigationRoutingTestData
        {
            get
            {
                return new TheoryDataSet<Type, string, string[]>()
                {
                    // Get
                    {
                        typeof(CustomersController),
                        "GetOrders",
                        new[]
                        {
                            "Customers({key})/Orders",
                            "Customers/{key}/Orders"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "GetSubOrdersFromVipCustomer",
                        new[]
                        {
                            "Customers({key})/NS.VipCustomer/SubOrders",
                            "Customers/{key}/NS.VipCustomer/SubOrders"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "PostToOrders",
                        new[]
                        {
                            "Customers({key})/Orders",
                            "Customers/{key}/Orders"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "PutToSubOrderFromVipCustomer",
                        new[]
                        {
                            "Customers({key})/NS.VipCustomer/SubOrder",
                            "Customers/{key}/NS.VipCustomer/SubOrder"
                        }
                    },
                    // singleton
                    {
                        typeof(MeController),
                        "PostToSubOrdersFromVipCustomer",
                        new[]
                        {
                            "Me/NS.VipCustomer/SubOrders"
                        }
                    },
                    {
                        typeof(MeController),
                        "PutToOrder",
                        new[]
                        {
                            "Me/Order"
                        }
                    },
                    {
                        typeof(MeController),
                        "PatchToSubOrderFromVipCustomer",
                        new[]
                        {
                            "Me/NS.VipCustomer/SubOrder"
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(NavigationRoutingTestData))]
        public void NavigationRoutingConventionTestDataRunsAsExpected(Type controllerType, string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType, actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            bool returnValue = NavigationConvention.AppliesToAction(context);
            Assert.True(returnValue);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("PostToName")]
        [InlineData("Get")]
        public void PropertyRoutingConventionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            // Act
            bool returnValue = NavigationConvention.AppliesToAction(context);
            Assert.False(returnValue);

            // Assert
            Assert.Empty(action.Selectors);
        }

        [Theory]
        [InlineData("GetOrders", "Get", "Orders", null)]
        [InlineData("GetOrdersFromVipCustomer", "Get", "Orders", "VipCustomer")]
        [InlineData("PostToOrdersFromVipCustomer", "PostTo", "Orders", "VipCustomer")]
        [InlineData("PatchToSubOrder", "PatchTo", "SubOrder", null)]
        [InlineData("DeleteToSubOrder", null, null, null)]
        public void SplitActionNameForNavigationRoutingConventionWorksAsExpected(string action, string method, string property, string cast)
        {
            // Arrange
            string actual = NavigationRoutingConvention.SplitActionName(action, out string actualProperty, out string actualCast);

            // Act & Assert
            Assert.Equal(actual, method);
            Assert.Equal(property, actualProperty);
            Assert.Equal(actualCast, cast);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Order
            EdmEntityType order = new EdmEntityType("NS", "Order", null, false, true);
            order.AddKeys(order.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);

            // VipCustomer
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            EdmEntitySet orders = container.AddEntitySet("Orders", order);
            EdmEntitySet customers = container.AddEntitySet("Customers", customer);
            EdmSingleton me = container.AddSingleton("Me", customer);
            model.AddElement(container);

            EdmNavigationProperty ordersNavProp = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Orders",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                });

            customers.AddNavigationTarget(ordersNavProp, orders);
            me.AddNavigationTarget(ordersNavProp, orders);

            EdmNavigationProperty orderNavProp = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "Order",
                    TargetMultiplicity = EdmMultiplicity.One,
                    Target = order
                });
            customers.AddNavigationTarget(orderNavProp, orders);
            me.AddNavigationTarget(orderNavProp, orders);

            EdmNavigationProperty subOrdersNavProp = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "SubOrders",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                });

            customers.AddNavigationTarget(subOrdersNavProp, orders, new EdmPathExpression("NS.VipCustomer/SubOrders"));
            me.AddNavigationTarget(subOrdersNavProp, orders, new EdmPathExpression("NS.VipCustomer/SubOrders"));


            EdmNavigationProperty subOrderNavProp = customer.AddUnidirectionalNavigation(
                new EdmNavigationPropertyInfo
                {
                    Name = "SubOrder",
                    TargetMultiplicity = EdmMultiplicity.Many,
                    Target = order
                });

            customers.AddNavigationTarget(subOrderNavProp, orders, new EdmPathExpression("NS.VipCustomer/SubOrder"));
            me.AddNavigationTarget(subOrderNavProp, orders, new EdmPathExpression("NS.VipCustomer/SubOrder"));

            return model;
        }

        private class CustomersController
        {
            public void GetOrders(int key, CancellationToken cancellation)
            { }

            public void GetSubOrdersFromVipCustomer(int key, CancellationToken cancellation)
            { }

            public void PostToOrders(int key)
            { }

            public void PutToSubOrderFromVipCustomer(int key)
            { }

            public void PostToName(int key)
            { }

            public void Get(int key)
            { }
        }

        private class MeController
        {
            public void PostToSubOrdersFromVipCustomer(CancellationToken cancellation)
            { }

            public void PutToOrder()
            { }

            public void PatchToSubOrderFromVipCustomer(CancellationToken cancellation)
            { }
        }

        private class UnknownController
        { }
    }
}