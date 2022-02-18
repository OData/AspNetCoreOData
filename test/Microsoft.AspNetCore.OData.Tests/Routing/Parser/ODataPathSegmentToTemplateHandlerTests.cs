//-----------------------------------------------------------------------------
// <copyright file="ODataPathSegmentToTemplateHandlerTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Routing.Parser
{
    public class ODataPathSegmentToTemplateHandlerTests
    {
        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Metadata()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            // Act
            handler.Handle(MetadataSegment.Instance);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<MetadataSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Value()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            IEdmType intType = EdmCoreModel.Instance.GetInt32(false).Definition;
            ValueSegment segment = new ValueSegment(intType);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<ValueSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_NavigationPropertyLink()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

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
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<NavigationLinkSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Count()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            // Act
            handler.Handle(CountSegment.Instance);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<CountSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_DynamicPath()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            DynamicPathSegment segment = new DynamicPathSegment("any");

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<DynamicSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Action()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);
            IEdmAction action = new EdmAction("NS", "action", intType);
            OperationSegment segment = new OperationSegment(action, null);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<ActionSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Function()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            IEdmFunction function = new EdmFunction("NS", "function", intType);
            OperationSegment segment = new OperationSegment(function, null);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<FunctionSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_ActionImport()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            IEdmAction action = new EdmAction("NS", "action", intType);
            IEdmActionImport actionImport = new EdmActionImport(entityContainer, "action", action);
            OperationImportSegment segment = new OperationImportSegment(actionImport, null);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<ActionImportSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_FunctionImport()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            IEdmTypeReference intType = EdmCoreModel.Instance.GetInt32(false);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            IEdmFunction function = new EdmFunction("NS", "function", intType);
            IEdmFunctionImport functionImport = new EdmFunctionImport(entityContainer, "function", function);
            OperationImportSegment segment = new OperationImportSegment(functionImport, null);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<FunctionImportSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Property()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            IEdmStructuralProperty property = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            PropertySegment segment = new PropertySegment(property);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<PropertySegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Singleton()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmSingleton me = entityContainer.AddSingleton("me", customer);
            SingletonSegment segment = new SingletonSegment(me);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<SingletonSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_EntitySet()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            EntitySetSegment segment = new EntitySetSegment(customers);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<EntitySetSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_NavigationProperty()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

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
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<NavigationSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_TypeCast()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmEntityType vipCustomer = new EdmEntityType("NS", "VipCustomer", customer);
            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            TypeSegment segment = new TypeSegment(vipCustomer, customer, customers);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<CastSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_PathTemplate()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);
            PathTemplateSegment segment = new PathTemplateSegment("{any}");

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<PathTemplateSegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Throws_BatchSegment()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            // Act
            Action test = () => handler.Handle(BatchSegment.Instance);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "'ODataPathSegment' of kind 'BatchSegment' is not implemented.");
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Throws_BatchReferenceSegment()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            BatchReferenceSegment segment = new BatchReferenceSegment("$4", customer, customers);

            // Act
            Action test = () => handler.Handle(segment);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "'ODataPathSegment' of kind 'BatchReferenceSegment' is not implemented.");
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Key()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "Id", "{key}" }
            };

            KeySegment segment = new KeySegment(keys, customer, customers);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<KeySegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_Key_AfterNavigationProperty()
        {
            // Arrange
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(null);

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
                { "Id", "{relatedKey}" }
            };

            KeySegment segment2 = new KeySegment(keys, order, orders);

            // Act
            handler.Handle(segment1);
            handler.Handle(segment2);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            NavigationLinkSegmentTemplate nlTemplate = Assert.IsType<NavigationLinkSegmentTemplate>(segmentTemplate);
            Assert.NotNull(nlTemplate.Key);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Handles_AlternateKey()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            IEdmStructuralProperty code = customer.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Int32);
            model.AddElement(customer);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            model.AddElement(entityContainer);

            IDictionary<string, IEdmProperty> alternateKeys = new Dictionary<string, IEdmProperty>
            {
                { "Code", code }
            };
            model.AddAlternateKeyAnnotation(customer, alternateKeys);

            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "Code", "{Code}" }
            };

            KeySegment segment = new KeySegment(keys, customer, customers);
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(model);

            // Act
            handler.Handle(segment);

            // Assert
            ODataSegmentTemplate segmentTemplate = Assert.Single(handler.Templates);
            Assert.IsType<KeySegmentTemplate>(segmentTemplate);
        }

        [Fact]
        public void ODataPathSegmentToTemplateHandler_Throws_WithoutAlternateKey()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Code", EdmPrimitiveTypeKind.Int32);
            model.AddElement(customer);

            EdmEntityContainer entityContainer = new EdmEntityContainer("NS", "Default");
            EdmEntitySet customers = entityContainer.AddEntitySet("Customers", customer);
            model.AddElement(entityContainer);
            IDictionary<string, object> keys = new Dictionary<string, object>
            {
                { "Code", "{Code}" }
            };

            KeySegment segment = new KeySegment(keys, customer, customers);
            ODataPathSegmentToTemplateHandler handler = new ODataPathSegmentToTemplateHandler(model);

            // Act
            Action test = () => handler.Handle(segment);

            // Assert
            ExceptionAssert.Throws<ODataException>(test, "Cannot find key 'Code' in the 'NS.Customer' type.");
        }
    }
}
