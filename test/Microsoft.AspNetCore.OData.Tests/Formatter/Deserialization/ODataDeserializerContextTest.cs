//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerContextTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Reflection;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Formatter.Value;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Formatter.Deserialization
{
    public class ODataDeserializerContextTest
    {
        [Theory]
        [InlineData(typeof(Delta), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Delta<Customer>), true)]
        public void Property_IsDeltaOfT_HasRightValue(Type resourceType, bool expectedResult)
        {
            // Arrange & Act
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = resourceType };

            // Act
            Assert.Equal(expectedResult, context.IsDeltaOfT);
        }

        [Theory]
        [InlineData(typeof(Delta), false)]
        [InlineData(typeof(int), false)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(Delta<Customer>), false)]
        [InlineData(typeof(IEdmObject), true)]
        [InlineData(typeof(IEdmComplexObject), true)]
        [InlineData(typeof(IEdmEntityObject), true)]
        [InlineData(typeof(EdmComplexObject), true)]
        [InlineData(typeof(EdmEntityObject), true)]
        [InlineData(typeof(ODataUntypedActionParameters), true)]
        public void Property_IsNoClrType_HasRightValue(Type resourceType, bool expectedResult)
        {
            // Arrange & Act
            ODataDeserializerContext context = new ODataDeserializerContext { ResourceType = resourceType };

            // Assert
            Assert.Equal(expectedResult, context.IsNoClrType);
        }

        [Fact]
        public void GetContainer_Returns_InstanceAnnotationContainer()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmComplexType complex = new EdmComplexType("NS", "Complex");
            model.AddElement(complex);
            PropertyInfo propertyInfo = typeof(InstanceAnnotationClass).GetProperty("Container");
            InstanceAnnotationContainerAnnotation annotation = new InstanceAnnotationContainerAnnotation(propertyInfo);
            model.SetAnnotationValue(complex, annotation);

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = model
            };

            // Act
            InstanceAnnotationClass resource = new InstanceAnnotationClass();
            Assert.Null(resource.Container);
            IODataInstanceAnnotationContainer container = context.GetContainer(resource, complex);

            // Assert
            Assert.NotNull(container);
            Assert.Same(resource.Container, container);

            IODataInstanceAnnotationContainer container2 = context.GetContainer(resource, complex);
            Assert.Same(container, container2);
        }

        [Fact]
        public void GetContainer_Returns_InstanceAnnotationContainer_ForIDelta()
        {
            // Arrange
            EdmModel model = new EdmModel();
            EdmComplexType complex = new EdmComplexType("NS", "Complex");
            model.AddElement(complex);
            PropertyInfo propertyInfo = typeof(InstanceAnnotationClass).GetProperty("Container");
            InstanceAnnotationContainerAnnotation annotation = new InstanceAnnotationContainerAnnotation(propertyInfo);
            model.SetAnnotationValue(complex, annotation);

            ODataDeserializerContext context = new ODataDeserializerContext
            {
                Model = model
            };

            // Act
            Delta<InstanceAnnotationClass> resource = new Delta<InstanceAnnotationClass>();
            Assert.True(resource.TryGetPropertyValue("Container", out object containerOnDelta));
            Assert.Null(containerOnDelta);

            IODataInstanceAnnotationContainer container = context.GetContainer(resource, complex);

            // Assert
            Assert.NotNull(container);
            Assert.True(resource.TryGetPropertyValue("Container", out containerOnDelta));
            Assert.NotNull(containerOnDelta);
            Assert.Same(containerOnDelta, container);

            IODataInstanceAnnotationContainer container2 = context.GetContainer(resource, complex);
            Assert.Same(container, container2);
        }

        class InstanceAnnotationClass
        {
            public IODataInstanceAnnotationContainer Container { get; set; }
        }
    }
}
