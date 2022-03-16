//-----------------------------------------------------------------------------
// <copyright file="SingletonRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.TestCommon;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class SingletonRoutingConventionTests
    {
        private static SingletonRoutingConvention SingletonConvention = ConventionHelpers.CreateConvention<SingletonRoutingConvention>();
        private static IEdmModel EdmModel = GetEdmModel();

        [Fact]
        public void OrderOnSingletonAsExpected()
        {
            // Arrange & Act & Assert
            Assert.Equal(200, SingletonConvention.Order);
        }

        [Fact]
        public void AppliesToActionOnSingletonRoutingConvention_Throws_Context()
        {
            // Arrange
            SingletonRoutingConvention convention = new SingletonRoutingConvention();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => convention.AppliesToAction(null), "context");
        }

        [Theory]
        [InlineData(typeof(CustomersController), false)]
        [InlineData(typeof(MeController), true)]
        public void AppliesToControllerReturnsExpectedForController(Type controllerType, bool expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType);
            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);

            // Act
            bool actual = SingletonConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        public static TheoryDataSet<string, string[]> SingletonConventionTestData
        {
            get
            {
                return new TheoryDataSet<string, string[]>()
                {
                    // Bound to single
                    { "Get", new[] { "/Me" } },
                    { "GetFromVipCustomer", new[] { "/Me/NS.VipCustomer" } },
                    { "Put", new[] { "/Me" } },
                    { "PutFromVipCustomer", new[] { "/Me/NS.VipCustomer" } },
                    { "Patch", new[] { "/Me" } },
                    { "PatchFromVipCustomer", new[] { "/Me/NS.VipCustomer" } },
                };
            }
        }
        
        public static TheoryDataSet<string, string[]> SingletonConventionCaseInsensitiveTestData
        {
            get
            {
                return new TheoryDataSet<string, string[]>()
                {
                    // Bound to single
                    { "GET", new[] { "/CaseInsensitiveMe" } },
                    { "GETFromVIPCUSTOMER", new[] { "/CaseInsensitiveMe/NS.VipCustomer" } },
                    { "PUT", new[] { "/CaseInsensitiveMe" } },
                    { "PUTFromVIPCUSTOMER", new[] { "/CaseInsensitiveMe/NS.VipCustomer" } },
                    { "PATCH", new[] { "/CaseInsensitiveMe" } },
                    { "PATCHFromVIPCUSTOMER", new[] { "/CaseInsensitiveMe/NS.VipCustomer" } },
                };
            }
        }

        [Theory]
        [MemberData(nameof(SingletonConventionTestData))]
        public void SingletonRoutingConventionTestDataRunsAsExpected(string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<MeController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;

            // Act
            SingletonConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }
        
        [Theory]
        [MemberData(nameof(SingletonConventionCaseInsensitiveTestData))]
        public void SingletonRoutingConventionCaseInsensitiveTestDataRunsAsExpected(string actionName, string[] templates)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<CaseInsensitiveMeController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = action;
            context.Options.RouteOptions.EnableActionNameCaseInsensitive = true;

            // Act
            SingletonConvention.AppliesToAction(context);

            // Assert
            Assert.Equal(templates.Length, action.Selectors.Count);
            Assert.Equal(templates, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Theory]
        [InlineData("GetFrom")]
        [InlineData("PutFrom")]
        [InlineData("PatchFrom")]
        public void SingletonRoutingConventionDoesNothingForNotSupportedAction(string actionName)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<MeController>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmModel, controller);
            context.Action = controller.Actions.First();

            // Act
            bool returnValue = SingletonConvention.AppliesToAction(context);
            Assert.False(returnValue);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Null(selector.AttributeRouteModel);
        }

        private static IEdmModel GetEdmModel()
        {
            EdmModel model = new EdmModel();

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(customer);
            model.AddElement(vipCustomer);
            var entityContainer = new EdmEntityContainer("NS", "Default");
            entityContainer.AddSingleton("Me", customer);
            entityContainer.AddSingleton("CaseInsensitiveMe", customer);
            model.AddElement(entityContainer);
            return model;
        }

        private class MeController
        {
            public void Get()
            {
            }

            public void GetFromVipCustomer()
            {
            }

            public void Put()
            {
            }

            public void PutFromVipCustomer()
            {
            }

            public void Patch()
            {
            }

            public void PatchFromVipCustomer()
            {
            }

            public void GetFrom()
            {
            }

            public void PutFrom()
            {
            }

            public void PatchFrom()
            {
            }
        }        
        
        private class CaseInsensitiveMeController
        {
            public void GET()
            {
            }

            public void GETFromVIPCUSTOMER()
            {
            }

            public void PUT()
            {
            }

            public void PUTFromVIPCUSTOMER()
            {
            }

            public void PATCH()
            {
            }

            public void PATCHFromVIPCUSTOMER()
            {
            }
        }

        private class CustomersController
        { }
    }
}
