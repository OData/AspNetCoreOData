//-----------------------------------------------------------------------------
// <copyright file="LinkGenerationHelpersTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Edm;
using Microsoft.AspNetCore.OData.Tests.Extensions;
using Microsoft.AspNetCore.OData.Tests.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter
{
    public class LinkGenerationHelpersTest
    {
        private static IEdmModel _model;
        private static IEdmEntitySet _customers;
        private static IEdmEntityType _customer;
        private static IEdmEntityType _specialCustomer;
        private static IEdmModel _myOrderModel = GetEdmModel2();

        static LinkGenerationHelpersTest()
        {
            _model = GetEdmModel();
            _customers = _model.EntityContainer.FindEntitySet("Customers");
            _customer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            _specialCustomer = _model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "SpecialCustomer");
        }

        [Theory]
        [InlineData(false, "http://localhost/Customers(42)")]
        [InlineData(true, "http://localhost/Customers(42)/NS.SpecialCustomer")]
        public void GenerateSelfLink_WorksToGenerateExpectedSelfLink_ForEntitySet(bool includeCast, string expectedIdLink)
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, _customers, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });

            // Act
            var idLink = entityContext.GenerateSelfLink(includeCast);

            // Assert
            Assert.Equal(expectedIdLink, idLink.ToString());
        }

        [Theory]
        [InlineData(false, "http://localhost/Customers(42)/Orders")]
        [InlineData(true, "http://localhost/Customers(42)/NS.SpecialCustomer/Orders")]
        public void GenerateNavigationLink_WorksToGenerateExpectedNavigationLink_ForEntitySet(bool includeCast, string expectedNavigationLink)
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, _customers, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmNavigationProperty ordersProperty = _customer.NavigationProperties().Single();

            // Act
            Uri uri = entityContext.GenerateNavigationPropertyLink(ordersProperty, includeCast);

            // Assert
            Assert.Equal(expectedNavigationLink, uri.AbsoluteUri);
        }

        [Theory]
        [InlineData(false, "http://localhost/Mary")]
        [InlineData(true, "http://localhost/Mary/NS.SpecialCustomer")]
        public void GenerateSelfLink_WorksToGenerateExpectedSelfLink_ForSingleton(bool includeCast, string expectedIdLink)
        {
            // Arrange
            IEdmSingleton mary = new EdmSingleton(_model.EntityContainer, "Mary", _customer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, mary, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });

            // Act
            var idLink = entityContext.GenerateSelfLink(includeCast);

            // Assert
            Assert.Equal(expectedIdLink, idLink.ToString());
        }

        [Theory]
        [InlineData(false, "http://localhost/Mary/Orders")]
        [InlineData(true, "http://localhost/Mary/NS.SpecialCustomer/Orders")]
        public void GenerateNavigationLink_WorksToGenerateExpectedNavigationLink_ForSingleton(bool includeCast, string expectedNavigationLink)
        {
            // Arrange
            IEdmSingleton mary = new EdmSingleton(_model.EntityContainer, "Mary", _customer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, mary, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmNavigationProperty ordersProperty = _customer.NavigationProperties().Single();

            // Act
            Uri uri = entityContext.GenerateNavigationPropertyLink(ordersProperty, includeCast);

            // Assert
            Assert.Equal(expectedNavigationLink, uri.AbsoluteUri);
        }

        private ResourceContext GetOrderLineResourceForNewSingletonContainer()
        {
            // Arrange
            IEdmSingleton myVipOrder = _myOrderModel.FindDeclaredSingleton("VipOrder");
            IEdmEntityType vipOrderType = (IEdmEntityType)myVipOrder.Type;
            IEdmNavigationProperty orderLinesProperty = vipOrderType.NavigationProperties().Single(x => x.ContainsTarget && x.Name == "OrderLines");
            IEdmContainedEntitySet orderLines = (IEdmContainedEntitySet)myVipOrder.FindNavigationTarget(orderLinesProperty);
            IEdmEntityType orderLine = _myOrderModel.SchemaElements.OfType<IEdmEntityType>().First(e => e.Name == "OrderLine");
            IEdmNavigationProperty orderLineDetailsNav = orderLine.NavigationProperties().First();

            HttpRequest request = RequestFactory.Create(_myOrderModel);

            ODataPath path = new ODataPath(
                    new SingletonSegment(myVipOrder),
                    new NavigationPropertySegment(orderLinesProperty, orderLines));

            ODataSerializerContext orderLineSerializerContext = ODataSerializerContextFactory.Create(_myOrderModel, orderLines, path, request);
            orderLineSerializerContext.EdmProperty = orderLineDetailsNav;
            ResourceContext orderLineResource = new ResourceContext(orderLineSerializerContext, orderLine.AsReference(), new { ID = 21 });
            orderLineSerializerContext.ExpandedResource = orderLineResource;

            return orderLineResource;
        }

        [Fact]
        public void GenerateBaseODataPathSegments_WorksToGenerateExpectedPath_ForSingletonContainer()
        {
            // Arrange
            ResourceContext orderLineResource = GetOrderLineResourceForNewSingletonContainer();

            // Act
            IList<ODataPathSegment> newPaths = orderLineResource.GenerateBaseODataPathSegments();

            // Assert
            Assert.Equal(3, newPaths.Count());
            Assert.Equal("Microsoft.OData.UriParser.SingletonSegment", newPaths[0].GetType().FullName); // VipOrder
            Assert.Equal("Microsoft.OData.UriParser.NavigationPropertySegment", newPaths[1].GetType().FullName); // OrderLines
            Assert.Equal("Microsoft.OData.UriParser.KeySegment", newPaths[2].GetType().FullName); // 21
        }

        [Fact]
        public void GenerateSelfLink_WorksToGenerateExpectedSelfLink_ForSingletonContainer()
        {
            // Arrange
            ResourceContext orderLineResource = GetOrderLineResourceForNewSingletonContainer();

            // Act
            Uri selfLink = orderLineResource.GenerateSelfLink(false);

            // Assert
            Assert.Equal("http://localhost/VipOrder/OrderLines(21)", selfLink.AbsoluteUri);
        }

        [Theory]
        [InlineData(false, "http://localhost/MyOrders(42)/OrderLines(21)/OrderLines")]
        [InlineData(true, "http://localhost/MyOrders(42)/OrderLines(21)/NS.OrderLine/OrderLines")]
        public void GenerateNavigationLink_WorksToGenerateExpectedNavigationLink_ForContainedNavigation(
            bool includeCast,
            string expectedNavigationLink)
        {
            // NOTE: This test is generating a link that does not technically correspond to a valid model (specifically
            //       the extra OrderLines navigation), but it allows us to validate the nested navigation scenario
            //       without twisting the model unnecessarily.

            // Arrange
            IEdmEntityType myOrder = (IEdmEntityType)_myOrderModel.FindDeclaredType("NS.MyOrder");
            IEdmEntityType orderLine = (IEdmEntityType)_myOrderModel.FindDeclaredType("NS.OrderLine");

            IEdmNavigationProperty orderLinesProperty = myOrder.NavigationProperties().Single(x => x.ContainsTarget && x.Name == "OrderLines");
            
            IEdmEntitySet entitySet = _myOrderModel.FindDeclaredEntitySet("MyOrders");
            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"ID", 42}
            };

            IDictionary<string, object> parameters2 = new Dictionary<string, object>
            {
                {"ID", 21}
            };

            // containment
            IEdmContainedEntitySet orderLines = (IEdmContainedEntitySet)entitySet.FindNavigationTarget(orderLinesProperty);

            ODataPath path = new ODataPath(
                    new EntitySetSegment(entitySet),
                    new KeySegment(parameters.ToArray(), myOrder, entitySet),
                    new NavigationPropertySegment(orderLinesProperty, orderLines),
                    new KeySegment(parameters2.ToArray(), orderLine, orderLines));

            var request = RequestFactory.Create(_myOrderModel);
            var serializerContext = ODataSerializerContextFactory.Create(_myOrderModel, orderLines, path, request);
            var entityContext = new ResourceContext(serializerContext, orderLine.AsReference(), new { ID = 21 });

            // Act
            Uri uri = entityContext.GenerateNavigationPropertyLink(orderLinesProperty, includeCast);

            // Assert
            Assert.Equal(expectedNavigationLink, uri.AbsoluteUri);
        }

        [Fact]
        public void GenerateNavigationLink_WorksToGenerateExpectedNavigationLink_ForNonContainedNavigation()
        {
            // Arrange
            IEdmEntityType myOrder = (IEdmEntityType)_myOrderModel.FindDeclaredType("NS.MyOrder");
            IEdmEntityType orderLine = (IEdmEntityType)_myOrderModel.FindDeclaredType("NS.OrderLine");
            IEdmNavigationProperty nonOrderLinesProperty = myOrder.NavigationProperties().Single(x => x.Name.Equals("NonContainedOrderLines"));

            IEdmEntitySet entitySet = _myOrderModel.FindDeclaredEntitySet("MyOrders");
            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"ID", 42}
            };

            IDictionary<string, object> parameters2 = new Dictionary<string, object>
            {
                {"ID", 21}
            };

            IEdmNavigationSource nonContainedOrderLines = entitySet.FindNavigationTarget(nonOrderLinesProperty);
            ODataPath path = new ODataPath(
                    new EntitySetSegment(entitySet),
                    new KeySegment(parameters.ToArray(), myOrder, entitySet),
                    new NavigationPropertySegment(nonOrderLinesProperty, nonContainedOrderLines),
                    new KeySegment(parameters2.ToArray(), orderLine, nonContainedOrderLines));

            IEdmNavigationProperty orderLinesProperty = myOrder.NavigationProperties().Single(x => x.ContainsTarget);
            IEdmContainedEntitySet orderLines = (IEdmContainedEntitySet)entitySet.FindNavigationTarget(orderLinesProperty);

            var request = RequestFactory.Create(_myOrderModel);
            var serializerContext = ODataSerializerContextFactory.Create(_myOrderModel, orderLines, path, request);
            var entityContext = new ResourceContext(serializerContext, orderLine.AsReference(), new { ID = 21 });

            // Act
            Uri uri = entityContext.GenerateSelfLink(false);

            // Assert
            Assert.Equal("http://localhost/OrderLines(21)", uri.AbsoluteUri);
        }

        [Fact]
        public void GenerateSelfLink_ThrowsArgumentNull_EntityContext()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => LinkGenerationHelpers.GenerateSelfLink(resourceContext: null, includeCast: false),
                "resourceContext");
        }

        //[Fact]
        //public void GenerateSelfLink_ThrowsArgument_IfUrlHelperIsNull()
        //{
        //    ResourceContext context = new ResourceContext();

        //    ExceptionAssert.ThrowsArgument(
        //        () => LinkGenerationHelpers.GenerateSelfLink(context, includeCast: false),
        //        "resourceContext",
        //        "The property 'Url' of ResourceContext cannot be null.");
        //}

        [Fact]
        public void GenerateNavigationPropertyLink_ThrowsArgumentNull_EntityContext()
        {
            IEdmNavigationProperty navigationProperty = new Mock<IEdmNavigationProperty>().Object;

            ExceptionAssert.ThrowsArgumentNull(
                () => LinkGenerationHelpers.GenerateNavigationPropertyLink(resourceContext: null, navigationProperty: navigationProperty, includeCast: false),
                "resourceContext");
        }

        //[Fact]
        //public void GenerateNavigationPropertyLink_ThrowsArgument_IfUrlHelperIsNull()
        //{
        //    IEdmNavigationProperty navigationProperty = new Mock<IEdmNavigationProperty>().Object;
        //    ResourceContext context = new ResourceContext();

        //    ExceptionAssert.ThrowsArgument(
        //        () => LinkGenerationHelpers.GenerateNavigationPropertyLink(context, navigationProperty, includeCast: false),
        //        "resourceContext",
        //        "The property 'Url' of ResourceContext cannot be null.");
        //}

        [Fact]
        public void GenerateActionLinkForFeed_ThrowsArgumentNull_FeedContext()
        {
            // Arrange
            ResourceSetContext feedContext = null;
            IEdmAction action = new Mock<IEdmAction>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => feedContext.GenerateActionLink(action), "resourceSetContext");
        }

        [Fact]
        public void GenerateActionLinkForFeed_ThrowsArgumentNull_Action()
        {
            // Arrange
            ResourceSetContext resourceSetContext = new ResourceSetContext();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => resourceSetContext.GenerateActionLink(action: null), "action");
        }

        [Fact]
        public void GenerateActionLinkForFeed_ThrowsActionNotBoundToCollectionOfEntity_IfActionHasNoParameters()
        {
            // Arrange
            ResourceSetContext context = new ResourceSetContext();
            Mock<IEdmAction> action = new Mock<IEdmAction>();
            action.Setup(a => a.Parameters).Returns(Enumerable.Empty<IEdmOperationParameter>());
            action.Setup(a => a.Name).Returns("SomeAction");

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => context.GenerateActionLink(action.Object),
                "action",
                "The action 'SomeAction' is not bound to the collection of entity. Only actions that are bound to entities can have action links.");
        }

        [Fact]
        public void GenerateActionLinkForFeed_GeneratesLinkWithoutCast_IfEntitySetTypeMatchesActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            IEdmAction action = _model.SchemaElements.OfType<IEdmAction>().First(a => a.Name == "UpgradeAll");
            Assert.NotNull(action); // Guard

            ResourceSetContext context = new ResourceSetContext { EntitySetBase = _customers, Request = request };

            // Act
            Uri link = context.GenerateActionLink(action);

            Assert.Equal("http://localhost/Customers/NS.UpgradeAll", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLinkForFeed_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            IEdmAction action = _model.SchemaElements.OfType<IEdmAction>().First(a => a.Name == "UpgradeSpecialAll");
            Assert.NotNull(action); // Guard
            ResourceSetContext context = new ResourceSetContext { EntitySetBase = _customers, Request = request };

            // Act
            Uri link = context.GenerateActionLink(action);

            // Assert
            Assert.Equal("http://localhost/Customers/NS.SpecialCustomer/NS.UpgradeSpecialAll", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLinkForFeed_GeneratesLinkWithDownCast_IfElementTypeDerivesFromBindingParameterType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            IEdmAction action = _model.SchemaElements.OfType<IEdmAction>().First(a => a.Name == "UpgradeAll");
            Assert.NotNull(action); // Guard
            IEdmEntitySet specialCustomers = new EdmEntitySet(_model.EntityContainer, "SpecialCustomers", _specialCustomer);

            ResourceSetContext context = new ResourceSetContext { EntitySetBase = specialCustomers, Request = request };

            // Act
            Uri link = context.GenerateActionLink(action);

            // Assert
            Assert.Equal("http://localhost/SpecialCustomers/NS.Customer/NS.UpgradeAll", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_ThrowsArgumentNull_EntityContext()
        {
            ResourceContext entityContext = null;
            IEdmActionImport action = new Mock<IEdmActionImport>().Object;

            ExceptionAssert.ThrowsArgumentNull(() => entityContext.GenerateActionLink(action.Action), "resourceContext");
        }

        [Fact]
        public void GenerateActionLink_ThrowsArgumentNull_Action()
        {
            ResourceContext entityContext = new ResourceContext();

            ExceptionAssert.ThrowsArgumentNull(() => entityContext.GenerateActionLink(action: null), "action");
        }

        [Fact]
        public void GenerateActionLink_ThrowsActionNotBoundToEntity_IfActionHasNoParameters()
        {
            ResourceContext entityContext = new ResourceContext();
            Mock<IEdmAction> action = new Mock<IEdmAction>();
            action.Setup(a => a.Parameters).Returns(Enumerable.Empty<IEdmOperationParameter>());
            action.Setup(a => a.Name).Returns("SomeAction");

            ExceptionAssert.ThrowsArgument(
                () => entityContext.GenerateActionLink(action.Object),
                "action",
                "The action 'SomeAction' is not bound to an entity. Only actions that are bound to entities can have action links.");
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithoutCast_IfEntitySetTypeMatchesActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, _customers, request);
            var entityContext = new ResourceContext(serializerContext, _customer.AsReference(), new { ID = 42 });
            IEdmAction upgradeCustomer = _model.SchemaElements.OfType<IEdmAction>().FirstOrDefault(c => c.Name == "upgrade");
            Assert.NotNull(upgradeCustomer);

            // Act
            Uri link = entityContext.GenerateActionLink(upgradeCustomer);

            Assert.Equal("http://localhost/Customers(42)/NS.upgrade", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, _customers, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmAction specialUpgrade = _model.SchemaElements.OfType<IEdmAction>().FirstOrDefault(c => c.Name == "specialUpgrade");
            Assert.NotNull(specialUpgrade);

            // Act
            Uri link = entityContext.GenerateActionLink(specialUpgrade);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.SpecialCustomer/NS.specialUpgrade", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithDownCast_IfElementTypeDerivesFromBindingParameterType()
        {
            // Arrange
            IEdmEntitySet specialCustomers = new EdmEntitySet(_model.EntityContainer, "SpecialCustomers", _specialCustomer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, specialCustomers, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmAction upgradeCustomer = _model.SchemaElements.OfType<IEdmAction>().FirstOrDefault(c => c.Name == "upgrade");
            Assert.NotNull(upgradeCustomer);

            // Act
            Uri link = entityContext.GenerateActionLink(upgradeCustomer);

            // Assert
            Assert.Equal("http://localhost/SpecialCustomers(42)/NS.Customer/NS.upgrade", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType_ForSingleton()
        {
            // Arrange
            IEdmSingleton mary = new EdmSingleton(_model.EntityContainer, "Mary", _customer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, mary, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmAction specialUpgrade = _model.SchemaElements.OfType<IEdmAction>().FirstOrDefault(c => c.Name == "specialUpgrade");
            Assert.NotNull(specialUpgrade);

            // Act
            Uri link = entityContext.GenerateActionLink(specialUpgrade);

            // Assert
            Assert.Equal("http://localhost/Mary/NS.SpecialCustomer/NS.specialUpgrade", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateActionLink_GeneratesLinkWithDownCast_IfElementTypeDerivesFromBindingParameterType_ForSingleton()
        {
            // Arrange
            IEdmSingleton me = new EdmSingleton(_model.EntityContainer, "Me", _specialCustomer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, me, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmAction upgradeCustomer = _model.SchemaElements.OfType<IEdmAction>().FirstOrDefault(c => c.Name == "upgrade");
            Assert.NotNull(upgradeCustomer);

            // Act
            Uri link = entityContext.GenerateActionLink(upgradeCustomer);

            // Assert
            Assert.Equal("http://localhost/Me/NS.Customer/NS.upgrade", link.AbsoluteUri);
        }

        //[Fact]
        //public void GenerateActionLink_ReturnsNull_ForContainment()
        //{
        //    // Arrange
        //    var request = RequestFactory.Create(_model);
        //    var serializerContext = ODataSerializerContextFactory.Create(_model, _model.OrderLines, request);
        //    var entityContext = new ResourceContext(serializerContext, _model.OrderLine.AsReference(), new { ID = 42 });

        //    // Act
        //    Uri link = entityContext.GenerateActionLink(_model.Tag);

        //    // Assert
        //    Assert.Null(link);
        //}

        [Fact]
        public void GenerateFunctionLinkForFeed_ThrowsArgumentNull_ResourceSetContext()
        {
            // Arrange
            ResourceSetContext resourceSetContext = null;
            IEdmFunction function = new Mock<IEdmFunction>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => resourceSetContext.GenerateFunctionLink(function), "resourceSetContext");
        }

        [Fact]
        public void GenerateFunctionLinkForFeed_ThrowsArgumentNull_Function()
        {
            // Arrange
            ResourceSetContext feedContext = new ResourceSetContext();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => feedContext.GenerateFunctionLink(function: null), "function");
        }

        [Fact]
        public void GenerateFunctionLinkForFeed_ThrowsFunctionNotBoundToCollectionOfEntity_IfFunctionHasNoParameters()
        {
            // Arrange
            ResourceSetContext context = new ResourceSetContext();
            Mock<IEdmFunction> function = new Mock<IEdmFunction>();
            function.Setup(a => a.Parameters).Returns(Enumerable.Empty<IEdmOperationParameter>());
            function.Setup(a => a.Name).Returns("SomeFunction");

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => context.GenerateFunctionLink(function.Object),
                "function",
                "The function 'SomeFunction' is not bound to the collection of entity. Only functions that are bound to entities can have function links.");
        }

        [Fact]
        public void GenerateFunctionLinkForFeed_GeneratesLinkWithoutCast_IfEntitySetTypeMatchesActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            IEdmFunction function = _model.SchemaElements.OfType<IEdmFunction>().First(a => a.Name == "IsAllUpgraded");
            Assert.NotNull(function); // Guard
            ResourceSetContext context = new ResourceSetContext { EntitySetBase = _customers, Request = request };

            // Act
            Uri link = context.GenerateFunctionLink(function);

            Assert.Equal("http://localhost/Customers/NS.IsAllUpgraded(param=@param)", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLinkForFeed_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            IEdmFunction function = _model.SchemaElements.OfType<IEdmFunction>().First(a => a.Name == "IsSpecialAllUpgraded");
            Assert.NotNull(function); // Guard

            ResourceSetContext context = new ResourceSetContext { EntitySetBase = _customers, Request = request };
            // Act
            Uri link = context.GenerateFunctionLink(function);

            // Assert
            Assert.Equal("http://localhost/Customers/NS.SpecialCustomer/NS.IsSpecialAllUpgraded(param=@param)", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLinkForFeed_GeneratesLinkWithDownCast_IfElementTypeDerivesFromBindingParameterType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            IEdmFunction function = _model.SchemaElements.OfType<IEdmFunction>().First(a => a.Name == "IsAllUpgraded");
            Assert.NotNull(function); // Guard
            IEdmEntitySet specialCustomers = new EdmEntitySet(_model.EntityContainer, "SpecialCustomers", _specialCustomer);

            ResourceSetContext context = new ResourceSetContext { EntitySetBase = specialCustomers, Request = request };

            // Act
            Uri link = context.GenerateFunctionLink(function);

            // Assert
            Assert.Equal("http://localhost/SpecialCustomers/NS.Customer/NS.IsAllUpgraded(param=@param)", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLink_GeneratesLinkWithoutCast_IfEntitySetTypeMatchesActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, _customers, request);
            var entityContext = new ResourceContext(serializerContext, _customer.AsReference(), new { ID = 42 });
            IEdmFunction isCustomerUpgraded = _model.SchemaElements.OfType<IEdmFunction>().First(c => c.Name == "IsUpgradedWithParam");
            Assert.NotNull(isCustomerUpgraded);

            // Act
            Uri link = entityContext.GenerateFunctionLink(isCustomerUpgraded);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.IsUpgradedWithParam(city=@city)", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLink_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType()
        {
            // Arrange
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, _customers, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmFunction isSpecialUpgraded = _model.SchemaElements.OfType<IEdmFunction>().First(c => c.Name == "IsSpecialUpgraded");
            Assert.NotNull(isSpecialUpgraded);

            // Act
            Uri link = entityContext.GenerateFunctionLink(isSpecialUpgraded);

            // Assert
            Assert.Equal("http://localhost/Customers(42)/NS.SpecialCustomer/NS.IsSpecialUpgraded()", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLink_GeneratesLinkWithDownCast_IfElementTypeDerivesFromBindingParameterType()
        {
            // Arrange
            IEdmEntitySet specialCustomers = new EdmEntitySet(_model.EntityContainer, "SpecialCustomers", _specialCustomer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, specialCustomers, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmFunction isCustomerUpgraded = _model.SchemaElements.OfType<IEdmFunction>().First(c => c.Name == "IsUpgradedWithParam");
            Assert.NotNull(isCustomerUpgraded);

            // Act
            Uri link = entityContext.GenerateFunctionLink(isCustomerUpgraded);

            // Assert
            Assert.Equal("http://localhost/SpecialCustomers(42)/NS.Customer/NS.IsUpgradedWithParam(city=@city)",
                link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLink_GeneratesLinkWithCast_IfEntitySetTypeDoesnotMatchActionEntityType_ForSingleton()
        {
            // Arrange
            IEdmSingleton mary = new EdmSingleton(_model.EntityContainer, "Mary", _customer);
            var request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, mary, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmFunction isSpecialUpgraded = _model.SchemaElements.OfType<IEdmFunction>().First(c => c.Name == "IsSpecialUpgraded");
            Assert.NotNull(isSpecialUpgraded);

            // Act
            Uri link = entityContext.GenerateFunctionLink(isSpecialUpgraded);

            // Assert
            Assert.Equal("http://localhost/Mary/NS.SpecialCustomer/NS.IsSpecialUpgraded()", link.AbsoluteUri);
        }

        [Fact]
        public void GenerateFunctionLink_GeneratesLinkWithDownCast_IfElementTypeDerivesFromBindingParameterType_ForSingleton()
        {
            // Arrange
            IEdmSingleton me = new EdmSingleton(_model.EntityContainer, "Me", _specialCustomer);
            HttpRequest request = RequestFactory.Create(_model);
            var serializerContext = ODataSerializerContextFactory.Create(_model, me, request);
            var entityContext = new ResourceContext(serializerContext, _specialCustomer.AsReference(), new { ID = 42 });
            IEdmFunction isCustomerUpgraded = _model.SchemaElements.OfType<IEdmFunction>().First(c => c.Name == "IsUpgradedWithParam");
            Assert.NotNull(isCustomerUpgraded);

            // Act
            Uri link = entityContext.GenerateFunctionLink(isCustomerUpgraded);

            // Assert
            Assert.Equal("http://localhost/Me/NS.Customer/NS.IsUpgradedWithParam(city=@city)", link.AbsoluteUri);
        }

        


        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "NS";
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Order>("Orders");
            var specialCustomer = builder.EntityType<SpecialCustomer>();
            var customer = builder.EntityType<Customer>();

            FunctionConfiguration function = customer.Function("IsUpgradedWithParam").Returns<bool>();
            function.Parameter<string>("city");

            specialCustomer.Function("IsSpecialUpgraded").Returns<bool>();

            // bound to collection
            function = customer.Collection.Function("IsAllUpgraded").Returns<bool>();
            function.Parameter<int>("param");

            function = specialCustomer.Collection.Function("IsSpecialAllUpgraded").Returns<bool>();
            function.Parameter<int>("param");

            // actions
            customer.Action("upgrade");
            specialCustomer.Action("specialUpgrade");

            // actions bound to collection
            customer.Collection.Action("UpgradeAll");
            specialCustomer.Collection.Action("UpgradeSpecialAll");

            return builder.GetEdmModel();
        }

        private class Customer
        {
            public int ID { get; set; }

            public IList<Order> Orders { get; set; }
        }

        private class SpecialCustomer : Customer
        { }

        private class Order
        {
            public int ID { get; set; }
        }

        private static IEdmModel GetEdmModel2()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "NS";
            builder.EntitySet<MyOrder>("MyOrders");
            builder.Singleton<MyOrder>("VipOrder");
            return builder.GetEdmModel();
        }

        private class MyOrder
        {
            public int ID { get; set; }

            [Contained]
            public IList<OrderLine> OrderLines { get; set; }

            public IList<OrderLine> NonContainedOrderLines { get; set; }
        }

        private class OrderLine
        {
            public int ID { get; set; }

            [Contained]
            public IList<OrderLineDetail> OrderLineDetails { get; set; }
        }

        private class OrderLineDetail
        {
            public int ID { get; set; }
        }
    }
}
