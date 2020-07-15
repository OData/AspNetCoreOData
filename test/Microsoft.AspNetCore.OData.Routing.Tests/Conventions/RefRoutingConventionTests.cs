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
    public class RefRoutingConventionTests
    {
        private static RefRoutingConvention _ref = new RefRoutingConvention();

        [Fact]
        public void Tests()
        {
            string actionMethodName = "CreateRef";
            string method = RefRoutingConvention.SplitRefActionName(actionMethodName, out string prop, out string declaring);
            Assert.Equal("CreateRef", method);
            Assert.Null(prop);
            Assert.Null(declaring);

            actionMethodName = "GetRefToPropertyFromAbc";
            method = RefRoutingConvention.SplitRefActionName(actionMethodName, out prop, out declaring);
            Assert.Equal("GetRef", method);
            Assert.Equal("Property", prop);
            Assert.Equal("Abc", declaring);

            actionMethodName = "CreateRefFromAbcToProperty";
            method = RefRoutingConvention.SplitRefActionName(actionMethodName, out prop, out declaring);
            Assert.Null(method);
        }

        [Fact]
        public void AppliesToActionWorksAsExpected()
        {
            // Arrange
            ActionModel action = new ActionModel(
                typeof(RefTestController).GetMethod(nameof(RefTestController.CreateRef)),
                                         new List<object>());

            ODataControllerActionContext context = BuildContext(null);
            context.Controller = new ControllerModel(typeof(RefTestController).GetTypeInfo(), new List<object>());
            context.Action = action;

            _ref.AppliesToAction(context);

            Assert.True(true);
        }

        private static ODataControllerActionContext BuildContext(IEdmModel model)
        {
            ControllerModel controller = new ControllerModel(typeof(RefTestController).GetTypeInfo(), new List<object>());
            ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, model, controller);
            return context;
        }

        private class RefTestController
        {
            public void CreateRef(string navigationProperty)
            {
            }
        }
    }
}