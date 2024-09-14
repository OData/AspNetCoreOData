//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingApplicationModelProviderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing;

public class ODataRoutingApplicationModelProviderTests
{
    private static IEdmModel _model = GetEdmModel();

    [Fact]
    public void OnProvidersExecuting_DoesNothing()
    {
        // Arrange
        ODataOptions options = new ODataOptions();
        options.AddRouteComponents("odata", _model);

        IODataControllerActionConvention[] conventions = new IODataControllerActionConvention[]
        {
            new EntitySetRoutingConvention()
        };

        ODataRoutingApplicationModelProvider provider = CreateProvider(options, conventions);

        Type controllerType = typeof(CustomersController);
        ApplicationModelProviderContext providerContext = CreateProviderContext(controllerType);

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
                Assert.Single(e.Selectors);
            },
            e =>
            {
                // Get(int key)
                Assert.Equal("Get", e.ActionMethod.Name);
                Assert.Single(e.Parameters);
                Assert.Single(e.Selectors);
            });
    }

    [Fact]
    public void OnProvidersExecuted_AddODataRoutingSelector_WhenEntitySetRoutingConvention()
    {
        // Arrange
        ODataOptions options = new ODataOptions();
        options.AddRouteComponents("odata", _model);

        IODataControllerActionConvention[] conventions = new IODataControllerActionConvention[]
        {
            new EntitySetRoutingConvention()
        };

        ODataRoutingApplicationModelProvider provider = CreateProvider(options, conventions);

        Type controllerType = typeof(CustomersController);
        ApplicationModelProviderContext providerContext = CreateProviderContext(controllerType);

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
                Assert.Equal(new[] { "/odata/Customers", "/odata/Customers/$count" }, e.Selectors.Select(s => s.AttributeRouteModel.Template));
            },
            e =>
            {
                // Get(int key)
                Assert.Equal("Get", e.ActionMethod.Name);
                Assert.Single(e.Parameters);
                Assert.Single(e.Selectors);
            });
    }

    [Fact]
    public void OnProvidersExecuted_AddODataRoutingSelector_WhenEntityRoutingConvention()
    {
        // Arrange
        ODataOptions options = new ODataOptions();
        options.AddRouteComponents("odata", _model);

        IODataControllerActionConvention[] conventions = new IODataControllerActionConvention[]
        {
            new EntityRoutingConvention()
        };

        ODataRoutingApplicationModelProvider provider = CreateProvider(options, conventions);

        Type controllerType = typeof(CustomersController);
        ApplicationModelProviderContext providerContext = CreateProviderContext(controllerType);

        // Act
        provider.OnProvidersExecuted(providerContext);

        // Assert
        ControllerModel controller = Assert.Single(providerContext.Result.Controllers);

        Assert.Equal(2, controller.Actions.Count);
        Assert.Collection(controller.Actions,
            e =>
            {
                // Get()
                Assert.Equal("Get", e.ActionMethod.Name);
                Assert.Empty(e.Parameters);
                var selector = Assert.Single(e.Selectors);
                Assert.Null(selector.AttributeRouteModel);
            },
            e =>
            {
                // Get(int key)
                Assert.Equal("Get", e.ActionMethod.Name);
                Assert.Single(e.Parameters);
                Assert.Equal(2, e.Selectors.Count);
                Assert.Equal(new[] { "/odata/Customers({key})", "/odata/Customers/{key}" }, e.Selectors.Select(s => s.AttributeRouteModel.Template));
            });
    }

    [Fact]
    public void OnProvidersExecuted_AddODataRoutingSelector_WhenAttributeRoutingConvention()
    {
        // Arrange
        ODataOptions options = new ODataOptions();
        options.AddRouteComponents("odata", _model);

        LoggerFactory loggerFactory = new LoggerFactory();
        var logger = new Logger<AttributeRoutingConvention>(loggerFactory);
        IODataPathTemplateParser parser = new DefaultODataPathTemplateParser();
        IODataControllerActionConvention[] conventions = new IODataControllerActionConvention[]
        {
            new AttributeRoutingConvention(logger, parser)
        };

        ODataRoutingApplicationModelProvider provider = CreateProvider(options, conventions);
        ApplicationModelProviderContext providerContext = CreateProviderContext(typeof(AttributeRoutingController));

        // Act
        provider.OnProvidersExecuted(providerContext);

        // Assert
        ControllerModel controller = Assert.Single(providerContext.Result.Controllers);

        ActionModel action = Assert.Single(controller.Actions);
        Assert.Equal("AnyMethodNameHere", action.ActionMethod.Name);
        Assert.Single(action.Parameters);
        SelectorModel selectorModel = Assert.Single(action.Selectors);
        Assert.Equal("/odata/Customers({key})/Name", selectorModel.AttributeRouteModel.Template);
        Assert.Contains(selectorModel.EndpointMetadata, a => a is ODataRoutingMetadata);
    }

    private static ODataRoutingApplicationModelProvider CreateProvider(ODataOptions options,
        IEnumerable<IODataControllerActionConvention> conventions)
    {
        foreach (var c in conventions)
        {
            options.Conventions.Add(c);
        }
        IOptions<ODataOptions> odataOptions = Options.Create(options);
        return new ODataRoutingApplicationModelProvider(/*conventions, */odataOptions);
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
        EdmModel model = new EdmModel();
        EdmEntityType customer = new EdmEntityType("NS", "Customer");
        customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
        customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        model.AddElement(customer);
        EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
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

    public class AttributeRoutingController : ODataController
    {
        [HttpGet("odata/Customers({key})/Name")]
        public void AnyMethodNameHere(int key)
        {
        }
    }
}
