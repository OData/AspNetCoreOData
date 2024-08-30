//-----------------------------------------------------------------------------
// <copyright file="AttributeRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Conventions;

public class AttributeRoutingConventionTests
{
    private static IEdmModel _edmModel;
    private static ODataOptions _options;
    private static AttributeRoutingConvention _attributeConvention;

    static AttributeRoutingConventionTests()
    {
        _edmModel = GetEdmModel();
        _options = new ODataOptions();
        _options.AddRouteComponents(_edmModel);
        _attributeConvention = CreateConvention();
    }

    [Fact]
    public void AppliesToControllerOnAttributeRoutingConvention_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Assert.True(_attributeConvention.AppliesToController(null));
    }

    [Fact]
    public void AppliesToActionOnAttributeRoutingConvention_Throws_Context()
    {
        // Arrange & Act & Assert
        ExceptionAssert.ThrowsArgumentNull(() => _attributeConvention.AppliesToAction(null), "context");
    }

    [Fact]
    public void AppliesToActionWithoutRoutePrefixWorksAsExpected()
    {
        // Arrange
        ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithoutPrefixController>("MyAction");
        ActionModel action = controller.Actions.First();
        Assert.Collection(action.Selectors, // Guard
            e =>
            {
                Assert.Equal("Customers({key})", e.AttributeRouteModel.Template);
                Assert.DoesNotContain(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("Customers/{key}/Name", e.AttributeRouteModel.Template);
                Assert.DoesNotContain(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            });

        ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, _edmModel, controller);
        context.Action = action;
        context.Options = _options;

        // Act
        bool ok = _attributeConvention.AppliesToAction(context);
        Assert.False(ok);

        Assert.Equal(2, action.Selectors.Count);
        Assert.Collection(action.Selectors,
            e =>
            {
                Assert.Equal("/Customers({key})", e.AttributeRouteModel.Template);
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Customers/{key}/Name", e.AttributeRouteModel.Template);
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            });
    }

    [Fact]
    public void AppliesToActionWithLongTemplateWorksAsExpected()
    {
        // Arrange
        ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithoutPrefixController>("LongAction");
        ActionModel action = controller.Actions.First();

        SelectorModel selectorModel = Assert.Single(action.Selectors); // Guard
        Assert.Equal("Customers({key})/Orders({relatedKey})/NS.MyOrder/Title", selectorModel.AttributeRouteModel.Template);
        Assert.DoesNotContain(selectorModel.EndpointMetadata, a => a is ODataRoutingMetadata);

        ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, _edmModel, controller)
        {
            Action = action,
            Options = _options,
        };

        // Act
        bool ok = _attributeConvention.AppliesToAction(context);
        Assert.False(ok);

        // Assert
        SelectorModel actualSelectorModel = Assert.Single(action.Selectors);
        Assert.Equal("/Customers({key})/Orders({relatedKey})/NS.MyOrder/Title", actualSelectorModel.AttributeRouteModel.Template);
        Assert.Null(actualSelectorModel.AttributeRouteModel.Order);
        Assert.Contains(actualSelectorModel.EndpointMetadata, a => a is ODataRoutingMetadata);
    }

    [Fact]
    public void AppliesToActionWithRoutePrefixWorksAsExpected()
    {
        // Arrange
        ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithPrefixController>("List");
        ActionModel action = controller.Actions.First();
        Assert.Equal(2, action.Selectors.Count);

        ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, _edmModel, controller)
        {
            Action = action,
            Options = _options,
        };

        AttributeRoutingConvention attributeConvention = CreateConvention();

        // Act
        bool ok = _attributeConvention.AppliesToAction(context);
        Assert.False(ok);

        // Assert
        Assert.Equal(4, action.Selectors.Count);
        Assert.Collection(action.Selectors,
            e =>
            {
                Assert.Equal("/Customers/{key}", e.AttributeRouteModel.Template);
                Assert.Equal(9, e.AttributeRouteModel.Order);
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Orders/{key}", e.AttributeRouteModel.Template);
                Assert.Equal(9, e.AttributeRouteModel.Order);
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Customers", e.AttributeRouteModel.Template);
                Assert.Equal(3, e.AttributeRouteModel.Order);
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Orders", e.AttributeRouteModel.Template);
                Assert.Equal(3, e.AttributeRouteModel.Order);
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            });
    }

    [Fact]
    public void AppliesToActionWithOrderOnControllerRoutePrefixWorksAsExpected()
    {
        // Arrange
        ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithPrefixController2>("List");
        ActionModel action = controller.Actions.First();
        Assert.Equal(2, action.Selectors.Count);

        ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, _edmModel, controller)
        {
            Action = action,
            Options = _options,
        };

        AttributeRoutingConvention attributeConvention = CreateConvention();

        // Act
        bool ok = _attributeConvention.AppliesToAction(context);
        Assert.False(ok);

        // Assert
        Assert.Equal(4, action.Selectors.Count);
        Assert.Collection(action.Selectors,
            e =>
            {
                Assert.Equal("/Customers/{key}", e.AttributeRouteModel.Template);
                Assert.Equal(9, e.AttributeRouteModel.Order); // Order from controller
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Orders/{key}", e.AttributeRouteModel.Template);
                Assert.Equal(8, e.AttributeRouteModel.Order); // Order from controller
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Customers", e.AttributeRouteModel.Template);
                Assert.Equal(3, e.AttributeRouteModel.Order); // Order from action
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            },
            e =>
            {
                Assert.Equal("/Orders", e.AttributeRouteModel.Template);
                Assert.Equal(3, e.AttributeRouteModel.Order); // Order from action
                Assert.Contains(e.EndpointMetadata, a => a is ODataRoutingMetadata);
            });
    }

    [Theory]
    [InlineData("GetVipCustomerWithPrefix", "/VipCustomer")]
    [InlineData("GetVipCustomerOrdersWithPrefix", "/VipCustomer/Orders")]
    [InlineData("GetVipCustomerNameWithPrefix", "/VipCustomer/Name")]
    public void AppliesToActionForSingletonWorksAsExpected(string actionName, string expectedTemplate)
    {
        // Arrange
        ControllerModel controller = ControllerModelHelpers.BuildControllerModel<SingletonTestControllerWithPrefix>(actionName);
        ActionModel action = controller.Actions.First();

        ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, _edmModel, controller);
        context.Action = action;
        context.Options = _options;
        AttributeRoutingConvention attributeConvention = CreateConvention();

        // Act
        bool ok = _attributeConvention.AppliesToAction(context);
        Assert.False(ok);

        // Assert
        SelectorModel selector = Assert.Single(action.Selectors);
        Assert.NotNull(selector.AttributeRouteModel);
        Assert.Equal(expectedTemplate, selector.AttributeRouteModel.Template);
        Assert.Contains(selector.EndpointMetadata, a => a is ODataRoutingMetadata);
    }

    [Fact]
    public void AppliesToActionForSingletonDoesnotWorksAsExpected()
    {
        // Arrange
        ControllerModel controller = ControllerModelHelpers.BuildControllerModel<SingletonTestControllerWithPrefix>("GetVipCustomerAliasWithPrefix");
        ActionModel action = controller.Actions.First();

        ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, _edmModel, controller);
        context.Action = action;
        context.Options = _options;

        // Act
        bool ok = _attributeConvention.AppliesToAction(context);
        Assert.False(ok);

        // Assert
        SelectorModel selector = Assert.Single(action.Selectors);
        Assert.NotNull(selector.AttributeRouteModel);
        Assert.Equal("Alias", selector.AttributeRouteModel.Template);
        Assert.DoesNotContain(selector.EndpointMetadata, a => a is ODataRoutingMetadata);
    }

    private static AttributeRoutingConvention CreateConvention()
    {
        var services = new ServiceCollection()
            .AddLogging();

        services.AddSingleton<IODataPathTemplateParser, DefaultODataPathTemplateParser>();
        services.AddSingleton<AttributeRoutingConvention>();

        return services.BuildServiceProvider().GetRequiredService<AttributeRoutingConvention>();
    }

    private static IEdmModel GetEdmModel()
    {
        EdmModel model = new EdmModel();

        // Customer
        EdmEntityType customer = new EdmEntityType("NS", "Customer");
        customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.String));
        customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
        customer.AddStructuralProperty("Alias", EdmPrimitiveTypeKind.String);
        model.AddElement(customer);

        // Order
        EdmEntityType order = new EdmEntityType("NS", "Order", null, false, true);
        order.AddKeys(order.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        model.AddElement(order);

        // MyOrder
        EdmEntityType myOrder = new EdmEntityType("NS", "MyOrder", order);
        myOrder.AddKeys(myOrder.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
        myOrder.AddStructuralProperty("Title", EdmPrimitiveTypeKind.String);
        model.AddElement(myOrder);

        EdmNavigationProperty ordersNavProp = customer.AddUnidirectionalNavigation(
            new EdmNavigationPropertyInfo
            {
                Name = "Orders",
                TargetMultiplicity = EdmMultiplicity.Many,
                Target = order
            });

        EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
        EdmEntitySet orders = container.AddEntitySet("Orders", order);
        EdmEntitySet customers = container.AddEntitySet("Customers", customer);
        customers.AddNavigationTarget(ordersNavProp, orders);
        EdmSingleton vipCustomer = container.AddSingleton("VipCustomer", customer);
        vipCustomer.AddNavigationTarget(ordersNavProp, orders);

        model.AddElement(container);
        return model;
    }

    private class WithoutPrefixController : ODataController
    {
        [HttpGet("Customers({key})")]
        [Route("Customers/{key}/Name")]
        [HttpPost]
        [HttpPatch]
        public void MyAction(int key)
        {
        }

        [HttpGet("Customers({key})/Orders({relatedKey})/NS.MyOrder/Title")]
        public void LongAction()
        {
        }
    }

    [ODataAttributeRouting] // using this attribute if not derived from ODataController
    [Route("Customers")]
    [Route("Orders")]
    private class WithPrefixController
    {
        [HttpGet("{key}", Order = 9)]
        [HttpPost("", Order = 3)]
        public void List(int key)
        {
        }
    }

    [ODataAttributeRouting] // using this attribute if not derived from ODataController
    [Route("Customers", Order = 9)]
    [Route("Orders", Order = 8)]
    private class WithPrefixController2
    {
        [HttpGet("{key}")]
        [HttpPost("", Order = 3)] // 3 should override 8 on controller
        public void List()
        {
        }
    }

    [Route("VipCustomer")]
    public class SingletonTestControllerWithPrefix
    {
        [ODataAttributeRouting]
        [HttpGet("")]
        public void GetVipCustomerWithPrefix()
        {
        }

        [ODataAttributeRouting]
        [HttpPost("Orders")]
        public void GetVipCustomerOrdersWithPrefix()
        {
        }

        [ODataAttributeRouting]
        [HttpGet("Name")]
        public void GetVipCustomerNameWithPrefix()
        {
        }

        [HttpGet("Alias")]
        public void GetVipCustomerAliasWithPrefix()
        {
        }
    }

    private class AttributeRoutingConventionTestPathTemplateParser : IODataPathTemplateParser
    {
        public ODataPathTemplate Parse(IEdmModel model, string odataPath, IServiceProvider requestProvider)
        {
            return new ODataPathTemplate();
        }
    }
}
