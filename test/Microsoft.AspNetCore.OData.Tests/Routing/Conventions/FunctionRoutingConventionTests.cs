//-----------------------------------------------------------------------------
// <copyright file="FunctionRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class FunctionRoutingConventionTests
    {
        private static FunctionRoutingConvention FunctionConvention = ConventionHelpers.CreateConvention<FunctionRoutingConvention>();
        private static IEdmModel EdmModel = GetEdmModel();

        [Fact]
        public void AppliesToActionOnFunctionRoutingConvention_Throws_Context()
        {
            // Arrange & Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => FunctionConvention.AppliesToController(null), "context");
            ExceptionAssert.ThrowsArgumentNull(() => FunctionConvention.AppliesToAction(null), "context");
        }

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
            bool actual = FunctionConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryDataSet<Type, string, string[]> FunctionRoutingConventionTestData
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
                            "/Customers({key})/NS.IsBaseUpgraded()",
                            "/Customers({key})/IsBaseUpgraded()",
                            "/Customers/{key}/NS.IsBaseUpgraded()",
                            "/Customers/{key}/IsBaseUpgraded()"
                        }
                    },
                    {
                        typeof(MeController),
                        "IsBaseUpgraded",
                        new[]
                        {
                            "/Me/NS.IsBaseUpgraded()",
                            "/Me/IsBaseUpgraded()"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsUpgraded",
                        new[]
                        {
                            "/Customers({key})/NS.IsUpgraded()",
                            "/Customers({key})/IsUpgraded()",
                            "/Customers/{key}/NS.IsUpgraded()",
                            "/Customers/{key}/IsUpgraded()"
                        }
                    },
                    {
                        typeof(MeController),
                        "IsUpgraded",
                        new[]
                        {
                            "/Me/NS.IsUpgraded()",
                            "/Me/IsUpgraded()"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsVipUpgraded",
                        new[]
                        {
                            "/Customers({key})/NS.VipCustomer/NS.IsVipUpgraded(param={param})",
                            "/Customers({key})/NS.VipCustomer/IsVipUpgraded(param={param})",
                            "/Customers/{key}/NS.VipCustomer/NS.IsVipUpgraded(param={param})",
                            "/Customers/{key}/NS.VipCustomer/IsVipUpgraded(param={param})"
                        }
                    },
                    {
                        typeof(MeController),
                        "IsVipUpgraded",
                        new[]
                        {
                            "/Me/NS.VipCustomer/NS.IsVipUpgraded(param={param})",
                            "/Me/NS.VipCustomer/IsVipUpgraded(param={param})"
                        }
                    },
                    // bound to collection
                    {
                        typeof(CustomersController),
                        "IsBaseAllUpgraded",
                        new[]
                        {
                            "/Customers/NS.IsBaseAllUpgraded(param={param})",
                            "/Customers/IsBaseAllUpgraded(param={param})"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsAllCustomersUpgraded",
                        new[]
                        {
                            "/Customers/NS.IsAllCustomersUpgraded(param={param})",
                            "/Customers/IsAllCustomersUpgraded(param={param})"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "IsVipAllUpgraded",
                        new[]
                        {
                            "/Customers/NS.VipCustomer/NS.IsVipAllUpgraded(param={param})",
                            "/Customers/NS.VipCustomer/IsVipAllUpgraded(param={param})"
                        }
                    },
                    // optional parameter
                    {
                        typeof(CustomersController),
                        "GetWholeSalary",
                        new[]
                        {
                            "/Customers/NS.GetWholeSalary(minSalary={minSalary},maxSalary={maxSalary},aveSalary={aveSalary})",
                            "/Customers/GetWholeSalary(minSalary={minSalary},maxSalary={maxSalary},aveSalary={aveSalary})"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "GetStatusOnLineOfflineUser",
                        new[]
                        {
                            "/Customers/NS.GetStatusOnLineOfflineUser()",
                            "/Customers/GetStatusOnLineOfflineUser()"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "GetStatusOnLineOfflineUser",
                        new[]
                        {
                            "/Customers/NS.GetStatusOnLineOfflineUser()",
                            "/Customers/GetStatusOnLineOfflineUser()"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "StatusLineOfflineUserOn",
                        new[]
                        {
                            "/Customers/NS.StatusLineOfflineUserOn()",
                            "/Customers/StatusLineOfflineUserOn()"
                        }
                    },
                    {
                        typeof(CustomersController),
                        "GetStatusOnLineOfflineUserOnVipCustomer",
                        new[]
                        {
                            "/Customers/NS.VipCustomer/NS.GetStatusOnLineOfflineUser(param={param})",
                            "/Customers/NS.VipCustomer/GetStatusOnLineOfflineUser(param={param})"
                        }
                    }
                };
            }
        }

        public static TheoryDataSet<Type, string, string[]> FunctionRoutingConventionCaseInsensitiveTestData
        {
            get
            {
                return new TheoryDataSet<Type, string, string[]>()
                {
                    // Bound to single
                    {
                        typeof(CustomersCaseInsensitiveController),
                        "ISBASEUPGRADED",
                        new[]
                        {
                            "/CustomersCaseInsensitive({key})/NS.IsBaseUpgraded()",
                            "/CustomersCaseInsensitive({key})/IsBaseUpgraded()",
                            "/CustomersCaseInsensitive/{key}/NS.IsBaseUpgraded()",
                            "/CustomersCaseInsensitive/{key}/IsBaseUpgraded()"
                        }
                    },
                    {
                        typeof(CustomersCaseInsensitiveController),
                        "ISUPGRADED",
                        new[]
                        {
                            "/CustomersCaseInsensitive({key})/NS.IsUpgraded()",
                            "/CustomersCaseInsensitive({key})/IsUpgraded()",
                            "/CustomersCaseInsensitive/{key}/NS.IsUpgraded()",
                            "/CustomersCaseInsensitive/{key}/IsUpgraded()"
                        }
                    },
                    {
                        typeof(CustomersCaseInsensitiveController),
                        "ISVIPUPGRADED",
                        new[]
                        {
                            "/CustomersCaseInsensitive({key})/NS.VipCustomer/NS.IsVipUpgraded(param={param})",
                            "/CustomersCaseInsensitive({key})/NS.VipCustomer/IsVipUpgraded(param={param})",
                            "/CustomersCaseInsensitive/{key}/NS.VipCustomer/NS.IsVipUpgraded(param={param})",
                            "/CustomersCaseInsensitive/{key}/NS.VipCustomer/IsVipUpgraded(param={param})"
                        }
                    },
                    // bound to collection
                    {
                        typeof(CustomersCaseInsensitiveController),
                        "ISBASEALLUPGRADED",
                        new[]
                        {
                            "/CustomersCaseInsensitive/NS.IsBaseAllUpgraded(param={param})",
                            "/CustomersCaseInsensitive/IsBaseAllUpgraded(param={param})"
                        }
                    },
                    {
                        typeof(CustomersCaseInsensitiveController),
                        "ISALLCUSTOMERSUPGRADED",
                        new[]
                        {
                            "/CustomersCaseInsensitive/NS.IsAllCustomersUpgraded(param={param})",
                            "/CustomersCaseInsensitive/IsAllCustomersUpgraded(param={param})"
                        }
                    },
                    {
                        typeof(CustomersCaseInsensitiveController),
                        "ISVIPALLUPGRADED",
                        new[]
                        {
                            "/CustomersCaseInsensitive/NS.VipCustomer/NS.IsVipAllUpgraded(param={param})",
                            "/CustomersCaseInsensitive/NS.VipCustomer/IsVipAllUpgraded(param={param})"
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(FunctionRoutingConventionTestData))]
        public void FunctionRoutingConventionTestDataRunsAsExpected(Type controllerType, string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType, actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            FunctionConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }
        
        [Theory]
        [MemberData(nameof(FunctionRoutingConventionTestData))]
        [MemberData(nameof(FunctionRoutingConventionCaseInsensitiveTestData))]
        public void FunctionRoutingConventionCaseInsensitiveTestDataRunsAsExpected(Type controllerType, string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType, actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;
            context.Options.RouteOptions.EnableActionNameCaseInsensitive = true;

            // Act
            FunctionConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }
        
        [Theory]
        [MemberData(nameof(FunctionRoutingConventionCaseInsensitiveTestData))]
        public void FunctionRoutingConventionCaseSensitiveByDefault(Type controllerType, string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType, actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            FunctionConvention.AppliesToAction(context);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Null(selector.AttributeRouteModel);
            Assert.NotEmpty(templates);
        }        

        public static TheoryDataSet<MethodInfo, string[]> OverloadFunctionTestData
        {
            get
            {
                Type controller = typeof(CustomersController);
                MethodInfo method1 = controller.GetMethod("UpgradedAll", new Type[] { typeof(int), typeof(string) });
                MethodInfo method2 = controller.GetMethod("UpgradedAll", new Type[] { typeof(int), typeof(string), typeof(string) });
                return new TheoryDataSet<MethodInfo, string[]>()
                {
                    {
                        method1,
                        new[]
                        {
                            "/Customers/NS.UpgradedAll(age={age},name={name})",
                            "/Customers/UpgradedAll(age={age},name={name})",
                            "/Customers/NS.VipCustomer/NS.UpgradedAll(age={age},name={name})",
                            "/Customers/NS.VipCustomer/UpgradedAll(age={age},name={name})"
                        }
                    },
                    {
                        // TODO: this overload result looks not good, let's improve it later.
                        method2,
                        new[]
                        {
                            "/Customers/NS.UpgradedAll(age={age},name={name})",
                            "/Customers/UpgradedAll(age={age},name={name})",
                            "/Customers/NS.UpgradedAll(age={age},name={name},gender={gender})",
                            "/Customers/UpgradedAll(age={age},name={name},gender={gender})",
                            "/Customers/NS.VipCustomer/NS.UpgradedAll(age={age},name={name})",
                            "/Customers/NS.VipCustomer/UpgradedAll(age={age},name={name})"
                        }
                    }
                };
            }
        }

        [Theory]
        [MemberData(nameof(OverloadFunctionTestData))]
        public void FunctionRoutingConventionResolveFunctionOverloadAsExpected(MethodInfo method, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModelByMethodInfo<CustomersController>(method);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            FunctionConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("Get")]
        [InlineData("UnknownFunction")]
        [InlineData("NonSupportedOn")]
        [InlineData("NonSupportedOnCollectionOf")]
        public void PropertyRoutingConventionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CustomersController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            // Act
            bool returnValue = FunctionConvention.AppliesToAction(context);
            Assert.False(returnValue);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Null(selector.AttributeRouteModel);
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

            // functions bound to single
            EdmFunction isBaseUpgraded = new EdmFunction("NS", "IsBaseUpgraded", returnType, true, entitySetPathExpression: null, isComposable: false);
            isBaseUpgraded.AddParameter("entity", new EdmEntityTypeReference(entity, false));
            model.AddElement(isBaseUpgraded);

            EdmFunction isUpgraded = new EdmFunction("NS", "IsUpgraded", returnType, true, entitySetPathExpression: null, isComposable: false);
            isUpgraded.AddParameter("entity", new EdmEntityTypeReference(customer, false));
            model.AddElement(isUpgraded);

            EdmFunction isVipUpgraded = new EdmFunction("NS", "IsVipUpgraded", returnType, true, entitySetPathExpression: null, isComposable: false);
            isVipUpgraded.AddParameter("entity", new EdmEntityTypeReference(vipCustomer, false));
            isVipUpgraded.AddParameter("param", stringType);
            model.AddElement(isVipUpgraded);

            // functions bound to collection
            EdmFunction isBaseAllUpgraded = new EdmFunction("NS", "IsBaseAllUpgraded", returnType, true, entitySetPathExpression: null, isComposable: false);
            isBaseAllUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(entity, false))));
            isBaseAllUpgraded.AddParameter("param", intType);
            model.AddElement(isBaseAllUpgraded);

            EdmFunction isAllUpgraded = new EdmFunction("NS", "IsAllCustomersUpgraded", returnType, true, entitySetPathExpression: null, isComposable: false);
            isAllUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            isAllUpgraded.AddParameter("param", intType);
            model.AddElement(isAllUpgraded);

            EdmFunction isVipAllUpgraded = new EdmFunction("NS", "IsVipAllUpgraded", returnType, true, entitySetPathExpression: null, isComposable: false);
            isVipAllUpgraded.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(vipCustomer, false))));
            isVipAllUpgraded.AddParameter("param", intType);
            model.AddElement(isVipAllUpgraded);

            // overloads
            EdmFunction upgradeAll1 = new EdmFunction("NS", "UpgradedAll", returnType, true, entitySetPathExpression: null, isComposable: false);
            upgradeAll1.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            upgradeAll1.AddParameter("age", intType);
            upgradeAll1.AddParameter("name", stringType);
            model.AddElement(upgradeAll1);

            EdmFunction upgradeAll2 = new EdmFunction("NS", "UpgradedAll", returnType, true, entitySetPathExpression: null, isComposable: false);
            upgradeAll2.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            upgradeAll2.AddParameter("age", intType);
            upgradeAll2.AddParameter("name", stringType);
            upgradeAll2.AddParameter("gender", stringType);
            model.AddElement(upgradeAll2);

            EdmFunction upgradeAll3 = new EdmFunction("NS", "UpgradedAll", returnType, true, entitySetPathExpression: null, isComposable: false);
            upgradeAll3.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(vipCustomer, false))));
            upgradeAll3.AddParameter("age", intType);
            upgradeAll3.AddParameter("name", stringType);
            model.AddElement(upgradeAll3);

            // function with optional parameters
            EdmFunction getSalaray = new EdmFunction("NS", "GetWholeSalary", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            getSalaray.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            getSalaray.AddParameter("minSalary", intType);
            getSalaray.AddOptionalParameter("maxSalary", intType);
            getSalaray.AddOptionalParameter("aveSalary", intType, "129");
            model.AddElement(getSalaray);

            EdmFunction f = new EdmFunction("NS", "GetStatusOnLineOfflineUser", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            f.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            model.AddElement(f);

            EdmFunction f2 = new EdmFunction("NS", "StatusLineOfflineUserOn", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            f2.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(customer, false))));
            model.AddElement(f2);

            EdmFunction f3 = new EdmFunction("NS", "GetStatusOnLineOfflineUser", intType, isBound: true, entitySetPathExpression: null, isComposable: false);
            f3.AddParameter("entityset", new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(vipCustomer, false))));
            f3.AddParameter("param", intType);
            model.AddElement(f3);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            container.AddEntitySet("Customers", customer);
            container.AddEntitySet("CustomersCaseInsensitive", customer);
            container.AddSingleton("Me", customer);
            model.AddElement(container);
            return model;
        }

        private class CustomersController
        {
            public void Get()
            { }

            [HttpGet]
            public void GetStatusOnLineOfflineUser() { }

            [HttpGet]
            public void GetStatusOnLineOfflineUserOnVipCustomer(int param) { }

            [HttpGet]
            public void StatusLineOfflineUserOn() { }

            [HttpGet]
            public void IsBaseUpgraded(int key, CancellationToken cancellation)
            { }

            [HttpGet]
            public void IsUpgraded(int key, CancellationToken cancellation)
            { }

            [HttpGet]
            public void IsVipUpgraded(int key, string param)
            { }

            [HttpGet]
            public void IsBaseAllUpgraded(int param)
            { }

            [HttpGet]
            public void IsAllCustomersUpgraded(int param)
            { }

            [HttpGet]
            public void IsVipAllUpgraded(CancellationToken cancellation, int param)
            { }

            [HttpGet]
            public void UpgradedAll(int age, string name)
            {
                // this one will match two
            }

            [HttpGet]
            public void UpgradedAll(int age, string name, string gender)
            { }

            [HttpGet]
            public void GetWholeSalary(int minSalary, int maxSalary, int aveSalary)
            { }

            [HttpGet]
            public void UnknownFunction()
            { }

            [HttpGet]
            public void NonSupportedOn()
            {
            }

            [HttpGet]
            public void NonSupportedOnCollectionOf()
            {
            }
        }

        private class MeController
        {
            [HttpGet]
            public void IsBaseUpgraded(CancellationToken cancellation)
            { }

            [HttpGet]
            public void IsUpgraded(CancellationToken cancellation)
            { }

            [HttpGet]
            public void IsVipUpgraded(string param)
            { }
        }
        
        private class CustomersCaseInsensitiveController
        {
            public void GET()
            { }

            [HttpGet]
            public void ISBASEUPGRADED(int key, CancellationToken cancellation)
            { }

            [HttpGet]
            public void ISUPGRADED(int key, CancellationToken cancellation)
            { }

            [HttpGet]
            public void ISVIPUPGRADED(int key, string param)
            { }

            [HttpGet]
            public void ISBASEALLUPGRADED(int param)
            { }

            [HttpGet]
            public void ISALLCUSTOMERSUPGRADED(int param)
            { }

            [HttpGet]
            public void ISVIPALLUPGRADED(CancellationToken cancellation, int param)
            { }
        }
        

        private class AnotherCustomersController
        { }

        private class UnknownController
        { }
    }
}
