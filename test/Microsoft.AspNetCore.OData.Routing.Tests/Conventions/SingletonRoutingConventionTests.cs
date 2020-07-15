// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
{
    public class SingletonRoutingConventionTests
    {
        private static SingletonRoutingConvention _convention = new SingletonRoutingConvention();

        [Fact]
        public void OrderOnSingletonAsExpected()
        {
            // Arrange & Act & Assert
            Assert.Equal(200, _convention.Order);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AppliesToControllerReturnsAsExpected(bool hasSingleton)
        {
            // Arrange & Act & Assert
            ODataControllerActionContext context = BuildContext(hasSingleton);
            Assert.Equal(hasSingleton, _convention.AppliesToController(context));
        }

        [Fact]
        public void AppliesToActionWorksAsExpected()
        {
            // Arrange
            ActionModel action = new ActionModel(
                typeof(MeController).GetMethod(nameof(MeController.GetFromVipCustomer)),
                                         new List<object>());

            ODataControllerActionContext context = BuildContext();
            context.Controller = new ControllerModel(typeof(MeController).GetTypeInfo(), new List<object>());
            context.Action = action;

            _convention.AppliesToAction(context);

            Assert.True(true);
        }

        private static ODataControllerActionContext BuildContext(bool hasSingleton = true)
        {
            ControllerModel controller = new ControllerModel(typeof(MeController).GetTypeInfo(), new List<object>());
            IEdmModel model = GetEdmModel();
            IEdmSingleton me = model.EntityContainer.FindSingleton("me");
            Assert.NotNull(me);

            if (hasSingleton)
            {
                return new ODataControllerActionContext(string.Empty, model, controller, me);
            }
            else
            {
                return new ODataControllerActionContext(string.Empty, model, controller);
            }
        }

        private static IEdmModel GetEdmModel()
        {
            var model = new EdmModel();
            var customer = new EdmEntityType("NS", "Customer");
            var vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            model.AddElement(customer);
            model.AddElement(vipCustomer);
            var entityContainer = new EdmEntityContainer("NS", "Default");
            entityContainer.AddSingleton("me", customer);
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
    }
}