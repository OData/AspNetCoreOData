// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Routing.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Routing.Tests.Conventions
{
    public class AttributeRoutingConventionTests
    {
        private static IEdmModel EdmModel = GetEdmModel();

        [Fact]
        public void AppliesToControllerWithoutRoutePrefixWorksAsExpected()
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithoutPrefixController>("MyAction");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, EdmModel, controller);
            AttributeRoutingConvention attributeConvention = CreateConvention();

            // Act
            bool ok = attributeConvention.AppliesToController(context);
            Assert.False(ok);

            Assert.Equal(4, action.Selectors.Count);
            Assert.Equal(new[]
                {
                    "Customers({key})",
                    "Customers/{key}",
                    "Customers({key})/Name",
                    "Customers/{key}/Name"
                },
                action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Fact]
        public void AppliesToControllerWithRoutePrefixWorksAsExpected()
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithPrefixController>("List");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, EdmModel, controller);
            AttributeRoutingConvention attributeConvention = CreateConvention();

            // Act
            bool ok = attributeConvention.AppliesToController(context);
            Assert.False(ok);

            // Assert
            Assert.Equal(6, action.Selectors.Count);
            Assert.Equal(new[]
                {
                    "Customers({key})",
                    "Customers/{key}",
                    "Customers",
                    "Orders({key})",
                    "Orders/{key}",
                    "Orders",
                },
                action.Selectors.Select(s => s.AttributeRouteModel.Template));
        }

        [Fact]
        public void AppliesToControllerWithLongTemplateWorksAsExpected()
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<WithoutPrefixController>("LongAction");
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, EdmModel, controller);
            AttributeRoutingConvention attributeConvention = CreateConvention();

            // Act
            bool ok = attributeConvention.AppliesToController(context);
            Assert.False(ok);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.NotNull(selector.AttributeRouteModel);
            Assert.Equal("Customers({key})/Orders({relatedKey})/NS.MyOrder/Title", selector.AttributeRouteModel.Template);
        }

        [Theory]
        [InlineData("GetVipCustomerWithPrefix", "VipCustomer")]
        [InlineData("GetVipCustomerOrdersWithPrefix", "VipCustomer/Orders")]
        [InlineData("GetVipCustomerNameWithPrefix", "VipCustomer/Name")]
        public void AppliesToControllerForSingletonWorksAsExpected(string actionName, string expectedTemplate)
        {
            // Arrange
            ControllerModel controller = ControllerModelHelpers.BuildControllerModel<SingletonTestControllerWithPrefix>(actionName);
            ActionModel action = controller.Actions.First();

            ODataControllerActionContext context = new ODataControllerActionContext(string.Empty, EdmModel, controller);
            AttributeRoutingConvention attributeConvention = CreateConvention();

            // Act
            bool ok = attributeConvention.AppliesToController(context);
            Assert.False(ok);

            // Assert
            SelectorModel selector = Assert.Single(action.Selectors);
            Assert.NotNull(selector.AttributeRouteModel);
            Assert.Equal(expectedTemplate, selector.AttributeRouteModel.Template);
        }

        private AttributeRoutingConvention CreateConvention()
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
            model.AddElement(customer);

            // Order
            EdmEntityType order = new EdmEntityType("NS", "Order", null, false, true);
            order.AddKeys(order.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            model.AddElement(order);

            // MyOrder
            EdmEntityType myOrder = new EdmEntityType("NS", "MyOrder");
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

        private class WithoutPrefixController
        {
            [ODataRoute("Customers({key})")]
            [ODataRoute("Customers/{key}/Name")]
            public void MyAction(int key)
            {
            }

            [ODataRoute("Customers({key})/Orders({relatedKey})/NS.MyOrder/Title")]
            public void LongAction()
            {

            }
        }

        [ODataRoutePrefix("Customers")]
        [ODataRoutePrefix("Orders")]
        private class WithPrefixController
        {
            [ODataRoute("({key})")]
            [ODataRoute("")]
            public void List(int key)
            {
            }
        }

        [ODataRoutePrefix("VipCustomer")]
        public class SingletonTestControllerWithPrefix
        {
            [ODataRoute("")]
            public void GetVipCustomerWithPrefix()
            {
            }

            [ODataRoute("Orders")]
            public void GetVipCustomerOrdersWithPrefix()
            {
            }

            [ODataRoute("Name")]
            public void GetVipCustomerNameWithPrefix()
            {
            }
        }

        private class AttributeRoutingConventionTestPathTemplateParser : IODataPathTemplateParser
        {
            public ODataPathTemplate Parse(IEdmModel model, string odataPath)
            {
                return new ODataPathTemplate();
            }
        }
    }
}