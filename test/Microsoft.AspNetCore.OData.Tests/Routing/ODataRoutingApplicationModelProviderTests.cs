// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing
{
    public class ODataRoutingApplicationModelProviderTests
    {
        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void OnProvidersExecuting_DoesNothing()
        {
            // Arrange
            var options = new ODataOptions();
            options.AddModel("odata", _model);

            var conventions = new IODataControllerActionConvention[]
            {
                new EntitySetRoutingConvention()
            };

            var provider = CreateProvider(options, conventions);

            var controllerType = typeof(CustomersController);
            var providerContext = CreateProviderContext(controllerType);

            // Act
            provider.OnProvidersExecuting(providerContext);

            // Assert
            var controller = Assert.Single(providerContext.Result.Controllers);
            Assert.Collection(controller.Actions,
                e =>
                {
                    // Get()
                    Assert.Equal("Get", e.ActionMethod.Name);
                    Assert.Empty(e.Parameters);
                    Assert.Empty(e.Selectors);
                },
                e =>
                {
                    // Get(int key)
                    Assert.Equal("Get", e.ActionMethod.Name);
                    Assert.Single(e.Parameters);
                    Assert.Empty(e.Selectors);
                });
        }

        [Fact]
        public void OnProvidersExecuted_AddODataRoutingSelector_WhenEntitySetRoutingConvention()
        {
            // Arrange
            var options = new ODataOptions();
            options.AddModel("odata", _model);

            var conventions = new IODataControllerActionConvention[]
            {
                new EntitySetRoutingConvention()
            };

            var provider = CreateProvider(options, conventions);

            var controllerType = typeof(CustomersController);
            var providerContext = CreateProviderContext(controllerType);

            // Act
            provider.OnProvidersExecuted(providerContext);

            // Assert
            var controller = Assert.Single(providerContext.Result.Controllers);

            Assert.Equal(2, controller.Actions.Count);
            Assert.Collection(controller.Actions,
                e =>
                {
                    // Get()
                    Assert.Equal("Get", e.ActionMethod.Name);
                    Assert.Empty(e.Parameters);
                    Assert.Equal(2, e.Selectors.Count);
                    Assert.Equal(new[] { "odata/Customers", "odata/Customers/$count" }, e.Selectors.Select(s => s.AttributeRouteModel.Template));
                },
                e =>
                {
                    // Get(int key)
                    Assert.Equal("Get", e.ActionMethod.Name);
                    Assert.Single(e.Parameters);
                    Assert.Empty(e.Selectors);
                });
        }

        [Fact]
        public void OnProvidersExecuted_AddODataRoutingSelector_WhenEntityRoutingConvention()
        {
            // Arrange
            var options = new ODataOptions();
            options.AddModel("odata", _model);

            var conventions = new IODataControllerActionConvention[]
            {
                new EntityRoutingConvention()
            };

            var provider = CreateProvider(options, conventions);

            var controllerType = typeof(CustomersController);
            var providerContext = CreateProviderContext(controllerType);

            // Act
            provider.OnProvidersExecuted(providerContext);

            // Assert
            var controller = Assert.Single(providerContext.Result.Controllers);

            Assert.Equal(2, controller.Actions.Count);
            Assert.Collection(controller.Actions,
                e =>
                {
                    // Get()
                    Assert.Equal("Get", e.ActionMethod.Name);
                    Assert.Empty(e.Parameters);
                    Assert.Empty(e.Selectors);
                },
                e =>
                {
                    // Get(int key)
                    Assert.Equal("Get", e.ActionMethod.Name);
                    Assert.Single(e.Parameters);
                    Assert.Equal(2, e.Selectors.Count);
                    Assert.Equal(new[] { "odata/Customers({key})", "odata/Customers/{key}" }, e.Selectors.Select(s => s.AttributeRouteModel.Template));
                });
        }

        [Fact]
        public void OnProvidersExecuted_AddODataRoutingSelector_WhenAttributeRoutingConvention()
        {
            // Arrange
            var options = new ODataOptions();
            options.AddModel("odata", _model);

            LoggerFactory loggerFactory = new LoggerFactory();
            var logger = new Logger<AttributeRoutingConvention>(loggerFactory);
            IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();
            var conventions = new IODataControllerActionConvention[]
            {
                new AttributeRoutingConvention(logger, parser)
            };

            var provider = CreateProvider(options, conventions);
            var providerContext = CreateProviderContext(typeof(AttributeRoutingController));

            // Act
            provider.OnProvidersExecuted(providerContext);

            // Assert
            var controller = Assert.Single(providerContext.Result.Controllers);

            ActionModel action = Assert.Single(controller.Actions);
            Assert.Equal("AnyMethodNameHere", action.ActionMethod.Name);
            Assert.Empty(action.Parameters);
            Assert.Equal(2, action.Selectors.Count);
            Assert.Equal(new[] { "odata/Customers({key})/Name", "odata/Customers/{key}/Name" }, action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        private static ODataRoutingApplicationModelProvider CreateProvider(ODataOptions options,
            IEnumerable<IODataControllerActionConvention> conventions)
        {
            IOptions<ODataOptions> odataOptions = Options.Create(options);
            return new ODataRoutingApplicationModelProvider(conventions, odataOptions);
        }

        private static ApplicationModelProviderContext CreateProviderContext(Type controllerType)
        {
            var context = new ApplicationModelProviderContext(new[] { controllerType.GetTypeInfo() });
            ControllerModel controllerModel = ControllerModelHelpers.BuildControllerModelWithAllActions(controllerType);
            context.Result.Controllers.Add(controllerModel);
            return context;
        }

        private static IEdmModel GetEdmModel()
        {
            var model = new EdmModel();
            var customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);
            var container = new EdmEntityContainer("NS", "Default");
            container.AddEntitySet("Customers", customer);
            model.AddElement(container);
            return model;
        }

        public class CustomersController
        {
            public void Get()
            {
            }

            public void Get(int key)
            {
            }
        }

        public class AttributeRoutingController
        {
            [ODataRoute("Customers({key})/Name")]
            public void AnyMethodNameHere(int key)
            {
            }
        }
    }
}
