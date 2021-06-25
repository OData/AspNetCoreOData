// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Serialization
{
    public class ODataSerializerContextTest
    {
        private static IEdmModel _model = GetEdmModel();

        [Fact]
        public void EmptyCtor_DoesnotThrow()
        {
            // Arrange & Act & Asserts
            ExceptionAssert.DoesNotThrow(() => new ODataSerializerContext());
        }

        [Fact]
        public void Ctor_ForNestedContext_ThrowsArgumentNull_Resource()
        {
            // Arrange
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = new Mock<IEdmNavigationProperty>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataSerializerContext(resource: null, selectExpandClause: selectExpand, edmProperty: navProp), "resource");
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_CopiesProperties()
        {
            // Arrange
            IEdmModel model = new Mock<IEdmModel>().Object;
            HttpRequest request = new Mock<HttpRequest>().Object;
            IEdmNavigationProperty navProp = new Mock<IEdmNavigationProperty>().Object;
            IEdmNavigationSource navigationSource = new Mock<IEdmNavigationSource>().Object;
            ODataSerializerContext context = new ODataSerializerContext
            {
                NavigationSource = navigationSource,
                MetadataLevel = ODataMetadataLevel.Full,
                Model = model,
                Path = new ODataPath(),
                Request = request,
                RootElementName = "somename",
                SelectExpandClause = new SelectExpandClause(new SelectItem[0], allSelected: true),
                SkipExpensiveAvailabilityChecks = true,
            };
            ResourceContext resource = new ResourceContext { SerializerContext = context };
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(resource, selectExpand, navProp);

            // Assert
            Assert.Equal(context.MetadataLevel, nestedContext.MetadataLevel);
            Assert.Same(context.Model, nestedContext.Model);
            Assert.Same(context.Path, nestedContext.Path);
            Assert.Same(context.Request, nestedContext.Request);
            Assert.Equal(context.RootElementName, nestedContext.RootElementName);
            Assert.Equal(context.SkipExpensiveAvailabilityChecks, nestedContext.SkipExpensiveAvailabilityChecks);
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_InitializesRightValues()
        {
            // Arrange
            IEdmEntitySet orders = _model.EntityContainer.FindEntitySet("Orders");
            IEdmEntitySet customers = _model.EntityContainer.FindEntitySet("Customers");
            IEdmEntityType customer = _model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(c => c.Name == "SerializerCustomer");
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmNavigationProperty navProp = customer.NavigationProperties().First();
            ODataSerializerContext context = new ODataSerializerContext { NavigationSource = customers, Model = _model };
            ResourceContext resource = new ResourceContext { SerializerContext = context };

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(resource, selectExpand, navProp);

            // Assert
            Assert.Same(resource, nestedContext.ExpandedResource);
            Assert.Same(navProp, nestedContext.NavigationProperty);
            Assert.Same(selectExpand, nestedContext.SelectExpandClause);
            Assert.Same(orders, nestedContext.NavigationSource);
            Assert.Same(navProp, nestedContext.EdmProperty);
        }

        [Fact]
        public void Ctor_ThatBuildsNestedContext_InitializesRightValues_ForComplex()
        {
            // Arrange
            SelectExpandClause selectExpand = new SelectExpandClause(new SelectItem[0], allSelected: true);
            IEdmEntitySet customers = _model.EntityContainer.FindEntitySet("Customers");
            IEdmEntityType customer = _model.SchemaElements.OfType<IEdmEntityType>().FirstOrDefault(c => c.Name == "SerializerCustomer");
            IEdmProperty complexProperty = customer.Properties().First(p => p.Name == "Address");
            ODataSerializerContext context = new ODataSerializerContext { NavigationSource = customers, Model = _model };
            ResourceContext resource = new ResourceContext { SerializerContext = context };

            // Act
            ODataSerializerContext nestedContext = new ODataSerializerContext(resource, selectExpand, complexProperty);

            // Assert
            Assert.Same(resource, nestedContext.ExpandedResource);
            Assert.Same(selectExpand, nestedContext.SelectExpandClause);
            Assert.Same(complexProperty, nestedContext.EdmProperty);
        }

        [Fact]
        public void Property_Items_IsInitialized()
        {
            ODataSerializerContext context = new ODataSerializerContext();
            Assert.NotNull(context.Items);
        }

        [Fact]
        public void GetEdmType_ThrowsInvalidOperation_IfEdmObjectGetEdmTypeReturnsNull()
        {
            // Arrange (this code path does not use ODataSerializerContext fields or properties)
            var context = new ODataSerializerContext();
            NullEdmType edmObject = new NullEdmType();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(() => context.GetEdmType(edmObject, null),
                exceptionMessage: "The EDM type of the object of type 'Microsoft.AspNetCore.OData.Tests.Formatter.Serialization.ODataSerializerContextTest+NullEdmType'" +
                " is null. The EDM type of an 'IEdmObject' cannot be null.");
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SerializerCustomer>("Customers");
            builder.EntitySet<SerializerOrder>("Orders");
            return builder.GetEdmModel();
        }

        /// <summary>
        /// An instance of IEdmObject with no EdmType.
        /// </summary>
        private class NullEdmType : IEdmObject
        {
            public IEdmTypeReference GetEdmType()
            {
                return null;
            }
        }

        private class SerializerCustomer
        {
            public int Id { get; set; }

            public SerializerAddress Address { get; set; }

            public SerializerOrder Order { get; set; }
        }

        private class SerializerOrder
        {
            public int Id { get; set; }
        }

        private class SerializerAddress
        {
            public string City { get; set; }
        }
    }
}
