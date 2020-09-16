// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions
{
    public class MetadataRoutingConventionTests
    {
        private static MetadataRoutingConvention _metadataConvention = ConventionHelpers.CreateConvention<MetadataRoutingConvention>();

        [Theory]
        [InlineData(typeof(MetadataController), true)]
        [InlineData(typeof(UnknownController), false)]
        public void AppliesToControllerReturnsExpectedForController(Type controllerType, bool expected)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel(controllerType);
            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(string.Empty, EdmCoreModel.Instance, controller);

            // Act
            bool actual = _metadataConvention.AppliesToController(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("odata")]
        [InlineData("odata{data}")]
        public void AppliesToActionAddTemplateForMetadataWithPrefix(string prefix)
        {
            // Arrange
            string expected = string.IsNullOrEmpty(prefix) ? "$metadata" : $"{prefix}/$metadata";
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<MetadataController>("GetMetadata");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(prefix, EdmCoreModel.Instance, controller);
            context.Action = action;

            // Act
            _metadataConvention.AppliesToAction(context);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Equal(expected, selector.AttributeRouteModel.Template);
        }

        [Theory]
        [InlineData("")]
        [InlineData("odata")]
        [InlineData("odata{data}")]
        public void AppliesToActionAddTemplateForServiceDocumentWithPrefix(string prefix)
        {
            // Arrange
            string expected = string.IsNullOrEmpty(prefix) ? "" : $"{prefix}/";
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<MetadataController>("GetServiceDocument");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = ODataControllerActionContextHelpers.BuildContext(prefix, EdmCoreModel.Instance, controller);
            context.Action = action;

            // Act
            _metadataConvention.AppliesToAction(context);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.Equal(expected, selector.AttributeRouteModel.Template);
        }

        private class UnknownController
        { }
    }
}