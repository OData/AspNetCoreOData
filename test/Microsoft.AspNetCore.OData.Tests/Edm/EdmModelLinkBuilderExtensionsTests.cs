// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.OData.Edm;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Edm
{
    public class EdmModelLinkBuilderExtensionsTests
    {
        private static IEdmModel Model;
        private static IEdmEntitySet Customers;
        private static IEdmNavigationProperty OrdersNavigationProperty;

        static EdmModelLinkBuilderExtensionsTests()
        {
            EdmModel model = new EdmModel();

            // Order
            EdmEntityType order = new EdmEntityType("NS", "Order", null, false, true);
            model.AddElement(order);

            // Customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            model.AddElement(customer);

            OrdersNavigationProperty = customer.AddUnidirectionalNavigation(
               new EdmNavigationPropertyInfo
               {
                   Name = "Orders",
                   TargetMultiplicity = EdmMultiplicity.Many,
                   Target = order
               });

            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            Customers = container.AddEntitySet("Customers", customer);

            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);
            function.AddParameter("entity", new EdmEntityTypeReference(customer, true));
            model.AddElement(function);

            Model = model;
        }

        [Fact]
        public void HasIdLink_SetsIdLinkBuilder()
        {
            // Arrange
            EdmModel model = new EdmModel();
            SelfLinkBuilder<Uri> linkBuilder = new SelfLinkBuilder<Uri>((ResourceContext a) => new Uri("http://id"), followsConventions: false);

            // Act
            model.HasIdLink(Customers, linkBuilder);

            // Assert
            SelfLinkBuilder<Uri> actualLinkBuilder = model.GetNavigationSourceLinkBuilder(Customers).IdLinkBuilder;
            Assert.Same(linkBuilder, actualLinkBuilder);
        }

        [Fact]
        public void HasEditLink_SetsIdLinkBuilder()
        {
            // Arrange
            EdmModel model = new EdmModel();

            SelfLinkBuilder<Uri> linkBuilder = new SelfLinkBuilder<Uri>((ResourceContext a) => new Uri("http://id"), followsConventions: false);

            // Act
            model.HasEditLink(Customers, linkBuilder);

            // Assert
            SelfLinkBuilder<Uri> actualLinkBuilder = model.GetNavigationSourceLinkBuilder(Customers).EditLinkBuilder;
            Assert.Same(linkBuilder, actualLinkBuilder);
        }

        [Fact]
        public void HasReadLink_SetsIdLinkBuilder()
        {
            // Arrange
            EdmModel model = new EdmModel();

            SelfLinkBuilder<Uri> linkBuilder = new SelfLinkBuilder<Uri>((ResourceContext a) => new Uri("http://id"), followsConventions: false);

            // Act
            model.HasReadLink(Customers, linkBuilder);

            // Assert
            SelfLinkBuilder<Uri> actualLinkBuilder = model.GetNavigationSourceLinkBuilder(Customers).ReadLinkBuilder;
            Assert.Same(linkBuilder, actualLinkBuilder);
        }

        [Fact]
        public void HasNavigationPropertyLink_SetsIdLinkBuilder()
        {
            // Arrange
            EdmModel model = new EdmModel();

            NavigationLinkBuilder linkBuilder = new NavigationLinkBuilder(
                (ResourceContext a, IEdmNavigationProperty b) => new Uri("http://orders"), followsConventions: false);

            // Act
            model.HasNavigationPropertyLink(Customers, OrdersNavigationProperty, linkBuilder);

            // Assert
            NavigationSourceLinkBuilderAnnotation annotation = model.GetNavigationSourceLinkBuilder(Customers);
            Uri uri = annotation.BuildNavigationLink(new ResourceContext(), OrdersNavigationProperty);
            Assert.Equal(uri, new Uri("http://orders"));
        }

        [Fact]
        public void SetOperationLinkBuilder_SetsOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            OperationLinkBuilder linkBuilder = new OperationLinkBuilder((ResourceContext a) => new Uri("http://localhost"), followsConventions: false);

            // Act
            model.SetOperationLinkBuilder(function, linkBuilder);

            // Assert
            OperationLinkBuilder actualLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(function);

            Assert.Same(linkBuilder, actualLinkBuilder);
            Assert.Equal(new Uri("http://localhost"), actualLinkBuilder.BuildLink((ResourceContext)null));
        }

        [Fact]
        public void SetOperationLinkBuilder_ResetOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            IEdmFunction function = new EdmFunction("NS", "Function", returnType, isBound: true, entitySetPathExpression: null, isComposable: false);

            // Act
            OperationLinkBuilder linkBuilder1 = new OperationLinkBuilder((ResourceContext a) => new Uri("http://localhost1"), followsConventions: false);
            model.SetOperationLinkBuilder(function, linkBuilder1);

            OperationLinkBuilder linkBuilder2 = new OperationLinkBuilder((ResourceContext a) => new Uri("http://localhost2"), followsConventions: false);
            model.SetOperationLinkBuilder(function, linkBuilder2);

            // Assert
            OperationLinkBuilder actualLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(function);

            Assert.Same(linkBuilder2, actualLinkBuilder);
            Assert.Equal(new Uri("http://localhost2"), actualLinkBuilder.BuildLink((ResourceContext)null));
        }

        [Fact]
        public void SetOperationLinkBuilder_ResetOperationLinkBuilder_AfterCallGetOperationLinkBuilder()
        {
            // Arrange
            IEdmModel model = new EdmModel();
            IEdmEntityType entity = new EdmEntityType("NS", "entity");
            IEdmTypeReference returnType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Boolean, isNullable: false);
            EdmFunction function = new EdmFunction("NS", "Function", returnType);
            function.AddParameter("entity", new EdmEntityTypeReference(entity, true));

            // Act
            OperationLinkBuilder linkBuilder = model.GetOperationLinkBuilder(function);
            Assert.NotNull(linkBuilder);

            OperationLinkBuilder linkBuilder2 = new OperationLinkBuilder((ResourceContext a) => new Uri("http://localhost2"), followsConventions: false);
            model.SetOperationLinkBuilder(function, linkBuilder2);

            // Assert
            OperationLinkBuilder actualLinkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(function);

            Assert.Same(linkBuilder2, actualLinkBuilder);
            Assert.Equal(new Uri("http://localhost2"), actualLinkBuilder.BuildLink((ResourceContext)null));
        }
    }
}
