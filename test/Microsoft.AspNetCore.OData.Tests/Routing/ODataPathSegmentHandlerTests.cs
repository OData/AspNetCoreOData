// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.AspNetCore.OData.Tests.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing
{
    public class ODataPathSegmentHandlerTests
    {
        [Fact]
        public void ODataPathSegmentHandler_Handles_MetadataSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            // Act
            handler.Handle(MetadataSegment.Instance);

            // Assert
            Assert.Equal("$metadata", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_ValueSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            IEdmType intType = EdmCoreModel.Instance.GetInt32(false).Definition;
            ValueSegment segment = new ValueSegment(intType);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("$value", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_NavigationPropertyLinkSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityType customer = new EdmEntityType("NS", "customer");
            EdmEntityType order = new EdmEntityType("NS", "order");
            IEdmNavigationProperty ordersNavProperty = customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Orders",
                Target = order,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            NavigationPropertyLinkSegment segment = new NavigationPropertyLinkSegment(ordersNavProperty, null);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("Orders/$ref", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_CountSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            // Act
            handler.Handle(CountSegment.Instance);

            // Assert
            Assert.Equal("$count", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_DynamicPathSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            DynamicPathSegment segment = new DynamicPathSegment("any");

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("any", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_ActionSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);
            IEdmAction action = new EdmAction("NS", "action", intType);
            OperationSegment segment = new OperationSegment(action, null);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("NS.action", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_FunctionSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            IEdmFunction function = new EdmFunction("NS", "function", intType);
            OperationSegment segment = new OperationSegment(function, null);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("NS.function()", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_ActionImportSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            IEdmAction action = new EdmAction("NS", "action", intType);
            IEdmActionImport actionImport = new EdmActionImport(entityContainer, "action", action);
            OperationImportSegment segment = new OperationImportSegment(actionImport, null);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("action", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_FunctionImportSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            IEdmFunction function = new EdmFunction("NS", "function", intType);
            IEdmFunctionImport functionImport = new EdmFunctionImport(entityContainer, "function", function);
            OperationImportSegment segment = new OperationImportSegment(functionImport, null);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("function()", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_PropertySegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            PropertySegment segment = new PropertySegment(property);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("Name", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_SingletonSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmSingleton me = entityContainer.AddSingleton("me", customer);
            SingletonSegment segment = new SingletonSegment(me);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("me", handler.PathLiteral);
            Assert.Same(me, handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_EntitySetSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            EntitySetSegment segment = new EntitySetSegment(customers);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("Customers", handler.PathLiteral);
            Assert.Same(customers, handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_NavigationPropertySegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityType customer = new EdmEntityType("NS", "customer");
            EdmEntityType order = new EdmEntityType("NS", "order");
            IEdmNavigationProperty ordersNavProperty = customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Orders",
                Target = order,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            NavigationPropertySegment segment = new NavigationPropertySegment(ordersNavProperty, null);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("Orders", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_TypeCastSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            TypeSegment segment = new TypeSegment(vipCustomer, customer, customers);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("NS.VipCustomer", handler.PathLiteral);
            Assert.Same(customers, handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_PathTemplateSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();
            PathTemplateSegment segment = new PathTemplateSegment("{any}");

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("{any}", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_BatchSegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            // Act
            handler.Handle(BatchSegment.Instance);

            // Assert
            Assert.Equal("$batch", handler.PathLiteral);
            Assert.Null(handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_KeySegment()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.String));

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "Id", "abc" }
            };

            KeySegment segment = new KeySegment(keys, customer, customers);

            // Act
            handler.Handle(segment);

            // Assert
            Assert.Equal("('abc')", handler.PathLiteral);
            Assert.Same(customers, handler.NavigationSource);
        }

        [Fact]
        public void ODataPathSegmentHandler_Handles_KeySegment_AfterNavigationProperty()
        {
            // Arrange
            ODataPathSegmentHandler handler = new ODataPathSegmentHandler();

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            EdmEntityType order = new EdmEntityType("NS", "order");
            order.AddKeys(order.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            IEdmNavigationProperty ordersNavProperty = customer.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Orders",
                Target = order,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            NavigationPropertyLinkSegment segment1 = new NavigationPropertyLinkSegment(ordersNavProperty, null);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet orders = entityContainer.AddEntitySet("Orders", order);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "Id", 42 }
            };

            KeySegment segment2 = new KeySegment(keys, order, orders);

            // Act
            handler.Handle(segment1);
            handler.Handle(segment2);

            // Assert
            Assert.Equal("Orders(42)/$ref", handler.PathLiteral);
            Assert.Same(orders, handler.NavigationSource);
        }

        [Fact]
        public void ConvertKeysToString_ConvertKeysValues()
        {
            // Arrange
            EdmEntityType entityType = new EdmEntityType("NS", "Entity");
            IEdmStructuralProperty key1 = entityType.AddStructuralProperty("Id", EdmCoreModel.Instance.GetInt32(false));
            IEdmStructuralProperty key2 = entityType.AddStructuralProperty("Id", EdmCoreModel.Instance.GetString(false));

            entityType.AddKeys(key1, key2);
            IEnumerable<KeyValuePair<string, object>> keys = new KeyValuePair<string, object>[]
            {
                KeyValuePair.Create("Id", (object)4),
                KeyValuePair.Create("Name", (object)"abc")
            };

            // Act
            string actual = ODataPathSegmentHandler.ConvertKeysToString(keys, entityType);

            // Assert
            Assert.Equal("Id=4,Name='abc'", actual);
        }

        [Fact]
        public void TranslateNode_TranslatesValue()
        {
            // Arrange & Act & Assert
            UriTemplateExpression expression = KeySegmentTemplateTests.BuildExpression("{key}");
            ConstantNode node = new ConstantNode(expression);
            Assert.Equal("{key}", ODataPathSegmentHandler.TranslateNode(node));

            // Arrange & Act & Assert
            EdmEnumType enumType = new EdmEnumType("NS", "Color");
            enumType.AddMember(new EdmEnumMember(enumType, "Red", new EdmEnumMemberValue(1)));
            ODataEnumValue enumValue = new ODataEnumValue("Red", "NS.Color");
            node = new ConstantNode(enumValue);
            Assert.Equal("NS.Color'Red'", ODataPathSegmentHandler.TranslateNode(node));
        }
    }
}
