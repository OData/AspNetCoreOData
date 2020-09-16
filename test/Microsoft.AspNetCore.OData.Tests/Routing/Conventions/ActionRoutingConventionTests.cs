// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class ActionRoutingConventionTests
    {
        private static ActionRoutingConvention ActionConvention = ConventionHelpers.CreateConvention<ActionRoutingConvention>();
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
            bool actual = ActionConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryDataSet<Type, string, string[]> ActionRoutingConventionTestData
        {
            get
            {
                return new TheoryDataSet<Type, string, string[]>()
                {
                    // Bound to single
                    {
                        typeof(CustomersController),
                        "IsBaseUpgraded",
                        new[]
                        {
                            "Customers({key})/NS.IsBaseUpgraded",
                            "Customers/{key}/NS.IsBaseUpgraded",
                            "Customers({key})/IsBaseUpgraded",
                            "Customers/{key}/IsBaseUpgraded",
                        }
                    },
                    {
                        typeof(MeController),
                        "IsBaseUpgraded",
                        new[]
                        {
                            "Me/NS.IsBaseUpgraded",
                            "Me/IsBaseUpgraded"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsUpgraded",
                        new[]
                        {
                            "Customers({key})/NS.IsUpgraded",
                            "Customers/{key}/NS.IsUpgraded",
                            "Customers({key})/IsUpgraded",
                            "Customers/{key}/IsUpgraded"
                        }
                    },
                    {
                        typeof(MeController),
                        "IsUpgraded",
                        new[]
                        {
                            "Me/NS.IsUpgraded",
                            "Me/IsUpgraded"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsVipUpgraded",
                        new[]
                        {
                            "Customers({key})/NS.VipCustomer/NS.IsVipUpgraded",
                            "Customers/{key}/NS.VipCustomer/NS.IsVipUpgraded",
                            "Customers({key})/NS.VipCustomer/IsVipUpgraded",
                            "Customers/{key}/NS.VipCustomer/IsVipUpgraded"
                        }
                    },
                    {
                        typeof(MeController),
                        "IsVipUpgraded",
                        new[]
                        {
                            "Me/NS.VipCustomer/NS.IsVipUpgraded",
                            "Me/NS.VipCustomer/IsVipUpgraded"
                        }
                    },
                    // bound to collection
                    {
                        typeof(CustomersController),
                        "IsBaseAllUpgraded",
                        new[]
                        {
                            "Customers/NS.IsBaseAllUpgraded",
                            "Customers/IsBaseAllUpgraded"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsAllCustomersUpgraded",
                        new[]
                        {
                            "Customers/NS.IsAllCustomersUpgraded",
                            "Customers/IsAllCustomersUpgraded"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsVipAllUpgraded",
                        new[]
                        {
                            "Customers/NS.VipCustomer/NS.IsVipAllUpgraded",
                            "Customers/NS.VipCustomer/IsVipAllUpgraded"
                        }
                    },
                    // overload
                    {
                        typeof(CustomersController),
                        "UpgradedAllOnCustomer",
                        new[]
                        {
                            "Customers({key})/NS.UpgradedAll",
                            "Customers/{key}/NS.UpgradedAll",
                            "Customers({key})/UpgradedAll",
                            "Customers/{key}/UpgradedAll"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "UpgradedAllOnCollectionOfCustomer",
                        new[]
                        {
                            "Customers/NS.UpgradedAll",
                            "Customers/UpgradedAll"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "UpgradedAllOnCollectionOfVipCustomer",
                        new[]
                        {
                            "Customers/NS.VipCustomer/NS.UpgradedAll",
                            "Customers/NS.VipCustomer/UpgradedAll"
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ActionRoutingConventionTestData))]
        public void ActionRoutingConventionTestDataRunsAsExpected(Type controllerType, string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType, actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            ActionConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("Post")]
        [InlineData("UnknownAction")]
        public void PropertyRoutingConventionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            // Act
            bool returnValue = ActionConvention.AppliesToAction(context);
            Assert.False(returnValue);

            // Assert
            Assert.Empty(action.Selectors);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmTypeReference stringType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, isNullable: false);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: false);

            // Entity
            EdmEntityType entity = new EdmEntityType("NS", "Entity", null, true, false);
            model.AddElement(entity);

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer", entity);
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            model.AddElement(customer);

            // VipCustomer
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(vipCustomer);

            // action bound to single
            EdmAction isBaseUpgraded = new EdmAction("NS", "IsBaseUpgraded", returnType, true, entitySetPathExpression: null);
            isBaseUpgraded.AddParameter("entity", new EdmEntityTypeReference(entity, false));
            isBaseUpgraded.AddParameter("param", stringType);
            model.AddElement(isBaseUpgraded);

            EdmAction isUpgraded = new EdmAction("NS", "IsUpgraded", returnType, true, entitySetPathExpression: null);
            isUpgraded.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(isUpgraded);

            EdmAction isVipUpgraded = new EdmAction("NS", "IsVipUpgraded", returnType, true, entitySetPathExpression: null);
            isVipUpgraded.AddParameter("entity", new EdmEntityTypeReference(vipCustomer, false));
            isVipUpgraded.AddParameter("param", stringType);
            model.AddElement(isVipUpgraded);

            // actions bound to collection
            EdmAction isBaseAllUpgraded = new EdmAction("NS", "IsBaseAllUpgraded", returnType, true, entitySetPathExpression: null);
            isBaseAllUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(entity, false))));
            isBaseAllUpgraded.AddParameter("param", intType);
            model.AddElement(isBaseAllUpgraded);

            EdmAction isAllUpgraded = new EdmAction("NS", "IsAllCustomersUpgraded", returnType, true, entitySetPathExpression: null);
            isAllUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            isAllUpgraded.AddParameter("param", intType);
            model.AddElement(isAllUpgraded);

            EdmAction isVipAllUpgraded = new EdmAction("NS", "IsVipAllUpgraded", returnType, true, entitySetPathExpression: null);
            isVipAllUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(vipCustomer, false))));
            isVipAllUpgraded.AddParameter("param", intType);
            model.AddElement(isVipAllUpgraded);

            // overloads
            EdmAction upgradeAll1 = new EdmAction("NS", "UpgradedAll", returnType, true, entitySetPathExpression: null);
            upgradeAll1.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            upgradeAll1.AddParameter("age", intType);
            upgradeAll1.AddParameter("name", stringType);
            model.AddElement(upgradeAll1);

            EdmAction upgradeAll2 = new EdmAction("NS", "UpgradedAll", returnType, true, entitySetPathExpression: null);
            upgradeAll2.AddParameter("entityset", new EdmEntityTypeReference(customer, false));
            upgradeAll2.AddParameter("age", intType);
            upgradeAll2.AddParameter("name", stringType);
            upgradeAll2.AddParameter("gender", stringType);
            model.AddElement(upgradeAll2);

            EdmAction upgradeAll3 = new EdmAction("NS", "UpgradedAll", returnType, true, entitySetPathExpression: null);
            upgradeAll3.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(vipCustomer, false))));
            upgradeAll3.AddParameter("age", intType);
            upgradeAll3.AddParameter("name", stringType);
            model.AddElement(upgradeAll3);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            container.AddEntitySet("Customers", customer);
            container.AddSingleton("Me", customer);
            model.AddElement(container);
            return model;
        }

        private class CustomersController
        {
            [HttpPost]
            public void Post()
            { }

            [HttpPost]
            public void IsBaseUpgraded(int key, CancellationToken cancellation, ODataActionParameters parameters)
            { }

            [HttpPost]
            public void IsUpgraded(int key, CancellationToken cancellation)
            { }

            [HttpPost]
            public void IsVipUpgraded(int key, ODataActionParameters parameters)
            { }

            [HttpPost]
            public void IsBaseAllUpgraded(ODataActionParameters parameters)
            { }

            [HttpPost]
            public void IsAllCustomersUpgraded(ODataActionParameters parameters)
            { }

            [HttpPost]
            public void IsVipAllUpgraded(CancellationToken cancellation, ODataUntypedActionParameters parameters)
            { }

            [HttpPost]
            public void UpgradedAllOnCustomer(int key, ODataActionParameters parameters)
            { }

            [HttpPost]
            public void UpgradedAllOnCollectionOfCustomer(ODataActionParameters parameters)
            { }

            [HttpPost]
            public void UpgradedAllOnCollectionOfVipCustomer(ODataActionParameters parameters)
            { }

            [HttpPost]
            public void UnknownAction()
            { }
        }

        private class MeController
        {
            [HttpPost]
            public void IsBaseUpgraded(CancellationToken cancellation, ODataActionParameters parameters)
            { }

            [HttpPost]
            public void IsUpgraded(CancellationToken cancellation)
            { }

            [HttpPost]
            public void IsVipUpgraded(ODataActionParameters parameters, string param)
            { }
        }

        private class AnotherCustomersController
        { }

        private class UnknownController
        { }
    }
}