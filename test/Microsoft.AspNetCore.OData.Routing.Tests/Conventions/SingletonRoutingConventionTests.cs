// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
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
                    { "Get", new[] { "Me" } },
                    { "GetFromVipCustomer", new[] { "Me/NS.VipCustomer" } },
                    { "Put", new[] { "Me" } },
                    { "PutFromVipCustomer", new[] { "Me/NS.VipCustomer" } },
                    { "Patch", new[] { "Me" } },
                    { "PatchFromVipCustomer", new[] { "Me/NS.VipCustomer" } },
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
        }

        private class CustomersController
        { }
    }
}