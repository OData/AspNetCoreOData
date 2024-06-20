//-----------------------------------------------------------------------------
// <copyright file="QueryBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    /// <summary>
    /// Tests to QueryBinder
    /// </summary>
    public class QueryBinderTests
    {
        [Fact]
        public void Bind_ThrowsArgumentNull_ForInputs()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            QueryNode node = new Mock<QueryNode>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => binder.Bind(null, null), "node");
            ExceptionAssert.ThrowsArgumentNull(() => binder.Bind(node, null), "context");
        }

        [Fact]
        public void Bind_ThrowsNotSupported_ForNotAcceptNode()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            Mock<QueryNode> node = new Mock<QueryNode>();
            node.Setup(c => c.Kind).Returns(QueryNodeKind.None);

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => binder.Bind(node.Object, new QueryBinderContext()),
                "Binding OData QueryNode of kind 'None' is not supported by 'QueryBinder'.");
        }

        [Fact]
        public void BindCollectionNode_ThrowsNotSupported_ForNotAcceptNode()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            Mock<CollectionNode> node = new Mock<CollectionNode>();
            node.Setup(c => c.Kind).Returns(QueryNodeKind.None);

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => binder.BindCollectionNode(node.Object, new QueryBinderContext()),
                "Binding OData QueryNode of kind 'None' is not supported by 'QueryBinder'.");
        }

        [Fact]
        public void BindSingleValueNode_ThrowsNotSupported_ForNotAcceptNode()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            Mock<SingleValueNode> node = new Mock<SingleValueNode>();
            node.Setup(c => c.Kind).Returns(QueryNodeKind.None);

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => binder.BindSingleValueNode(node.Object, new QueryBinderContext()),
                "Binding OData QueryNode of kind 'None' is not supported by 'QueryBinder'.");
        }

        [Fact]
        public void BindRangeVariable_ThrowsArgumentNull_ForInputs()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            RangeVariable variable = new Mock<RangeVariable>().Object;

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindRangeVariable(null, null), "rangeVariable");
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindRangeVariable(variable, null), "context");
        }

        [Fact]
        public void BindSingleResourceFunctionCallNode_ThrowsNotSupported_ForNotAcceptNode()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            Mock<IEdmStructuredTypeReference> typeRef = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navSource = new Mock<IEdmNavigationSource>();
            SingleResourceFunctionCallNode node = new SingleResourceFunctionCallNode("anyUnknown", null, typeRef.Object, navSource.Object);

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => binder.BindSingleResourceFunctionCallNode(node, new QueryBinderContext()),
                "Unknown function 'anyUnknown'.");
        }

        [Fact]
        public void BindSingleValueFunctionCallNode_ThrowsArgumentNull_ForInputs()
        {
            // Arrange
            QueryBinder binder = new MyQueryBinder();
            Mock<IEdmType> type = new Mock<IEdmType>();
            type.Setup(t => t.TypeKind).Returns(EdmTypeKind.Primitive);
            Mock<IEdmTypeReference> typeRef = new Mock<IEdmTypeReference>();
            typeRef.Setup(t => t.Definition).Returns(type.Object);
            SingleValueFunctionCallNode node = new SingleValueFunctionCallNode("any", null, typeRef.Object);

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindSingleValueFunctionCallNode(null, null), "node");
            ExceptionAssert.ThrowsArgumentNull(() => binder.BindSingleValueFunctionCallNode(node, null), "context");
        }

        [Fact]
        public void GetDynamicPropertyContainer_ThrowsArgumentNull_ForInputs()
        {
            // Arrange
            Mock<SingleValueNode> singleValueNode = new Mock<SingleValueNode>();
            SingleValueOpenPropertyAccessNode node = new SingleValueOpenPropertyAccessNode(singleValueNode.Object, "dynamic");

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => MyQueryBinder.Call_GetDynamicPropertyContainer(null, null), "openNode");
            ExceptionAssert.ThrowsArgumentNull(() => MyQueryBinder.Call_GetDynamicPropertyContainer(node, null), "context");
        }

        [Fact]
        public void GetDynamicPropertyContainer_ThrowsNotSupported_ForNotAcceptNode()
        {
            // Arrange
            Mock<IEdmType> type = new Mock<IEdmType>();
            type.Setup(t => t.TypeKind).Returns(EdmTypeKind.Primitive);
            Mock<IEdmTypeReference> typeRef = new Mock<IEdmTypeReference>();
            typeRef.Setup(t => t.Definition).Returns(type.Object);

            Mock<SingleValueNode> singleValueNode = new Mock<SingleValueNode>();
            singleValueNode.Setup(s => s.TypeReference).Returns(typeRef.Object);
            SingleValueOpenPropertyAccessNode node = new SingleValueOpenPropertyAccessNode(singleValueNode.Object, "dynamic");

            // Act & Assert
            ExceptionAssert.Throws<NotSupportedException>(() => MyQueryBinder.Call_GetDynamicPropertyContainer(node, new QueryBinderContext()),
                "Binding OData QueryNode of kind 'SingleValueOpenPropertyAccess' is not supported by 'QueryBinder'.");
        }

        [Theory]
        [InlineData(true, "$it.Values.Item[\"Values\"]")]
        [InlineData(false, "$it.Values")]
        public void GetPropertyExpression_Works_ForAggregateOrNonAggregate(bool isAggregated, string expected)
        {
            // Arrange
            ParameterExpression source = Expression.Parameter(typeof(GroupByWrapper), "$it");

            // Act
            var expression = QueryBinder.GetPropertyExpression(source, "Values", isAggregated);

            // Assert
            Assert.NotNull(expression);
            Assert.Equal(expected, expression.ToString());
        }

        [Fact]
        public void GetFullPropertyPath_WithSingleComplexNode()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> structuredType = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navigationSource = new Mock<IEdmNavigationSource>();
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", structuredType.Object, navigationSource.Object);
            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

            Mock<IEdmComplexTypeReference> type = new Mock<IEdmComplexTypeReference>();

            Mock<IEdmProperty> property = new Mock<IEdmProperty>();
            property.Setup(p => p.Name).Returns("Address");
            property.Setup(p => p.PropertyKind).Returns(EdmPropertyKind.Structural);
            property.Setup(p => p.Type).Returns(type.Object);

            SingleComplexNode node = new SingleComplexNode(source, property.Object);

            // Act
            string fullPropertyPath = QueryBinder.GetFullPropertyPath(node);

            // Assert
            Assert.Equal("Address", fullPropertyPath);
        }

        [Fact]
        public void GetFullPropertyPath_WithSingleValuePropertyAccessNode()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> structuredType = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navigationSource = new Mock<IEdmNavigationSource>();
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", structuredType.Object, navigationSource.Object);
            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

            Mock<IEdmTypeReference> type = new Mock<IEdmTypeReference>();

            Mock<IEdmProperty> property = new Mock<IEdmProperty>();
            property.Setup(p => p.Name).Returns("ZipCode");
            property.Setup(p => p.PropertyKind).Returns(EdmPropertyKind.Structural);
            property.Setup(p => p.Type).Returns(type.Object);

            SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(source, property.Object);

            // Act
            string fullPropertyPath = QueryBinder.GetFullPropertyPath(node);

            // Assert
            Assert.Equal("ZipCode", fullPropertyPath);
        }

        [Fact]
        public void GetFullPropertyPath_WithSingleNavigationNode()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> structuredType = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navigationSource = new Mock<IEdmNavigationSource>();
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", structuredType.Object, navigationSource.Object);
            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

            Mock<IEdmEntityTypeReference> type = new Mock<IEdmEntityTypeReference>();

            Mock<IEdmNavigationProperty> property = new Mock<IEdmNavigationProperty>();
            property.Setup(p => p.Name).Returns("Address");
            property.Setup(p => p.Type).Returns(type.Object);

            Mock<IEdmPathExpression> bindingPath = new Mock<IEdmPathExpression>();

            SingleNavigationNode node = new SingleNavigationNode(source, property.Object, bindingPath.Object);

            // Act
            string fullPropertyPath = QueryBinder.GetFullPropertyPath(node);

            // Assert
            Assert.Equal("Address", fullPropertyPath);
        }

        [Fact]
        public void GetFullPropertyPath_WithSingleValueOpenPropertyAccess()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> structuredType = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navigationSource = new Mock<IEdmNavigationSource>();
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", structuredType.Object, navigationSource.Object);
            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

            SingleValueOpenPropertyAccessNode node = new SingleValueOpenPropertyAccessNode(source, "ZipCode");

            // Act
            string fullPropertyPath = QueryBinder.GetFullPropertyPath(node);

            // Assert
            Assert.Equal("ZipCode", fullPropertyPath);
        }

        [Fact]
        public void GetFullPropertyPath_WithSingleValuePropertyAccessNodeInSingleComplexNode()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> structuredType = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navigationSource = new Mock<IEdmNavigationSource>();
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", structuredType.Object, navigationSource.Object);
            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

            Mock<IEdmComplexTypeReference> complexType = new Mock<IEdmComplexTypeReference>();

            Mock<IEdmProperty> complexProperty = new Mock<IEdmProperty>();
            complexProperty.Setup(p => p.Name).Returns("Address");
            complexProperty.Setup(p => p.PropertyKind).Returns(EdmPropertyKind.Structural);
            complexProperty.Setup(p => p.Type).Returns(complexType.Object);

            SingleComplexNode complexNode = new SingleComplexNode(source, complexProperty.Object);

            Mock<IEdmTypeReference> type = new Mock<IEdmTypeReference>();

            Mock<IEdmProperty> property = new Mock<IEdmProperty>();
            property.Setup(p => p.Name).Returns("ZipCode");
            property.Setup(p => p.PropertyKind).Returns(EdmPropertyKind.Structural);
            property.Setup(p => p.Type).Returns(type.Object);

            SingleValuePropertyAccessNode node = new SingleValuePropertyAccessNode(complexNode, property.Object);

            // Act
            string fullPropertyPath = QueryBinder.GetFullPropertyPath(node);

            // Assert
            Assert.Equal("Address\\ZipCode", fullPropertyPath);
        }

        [Fact]
        public void GetFullPropertyPath_WithSingleValueOpenPropertyAccessNodeInSingleComplexNode()
        {
            // Arrange
            Mock<IEdmStructuredTypeReference> structuredType = new Mock<IEdmStructuredTypeReference>();
            Mock<IEdmNavigationSource> navigationSource = new Mock<IEdmNavigationSource>();
            ResourceRangeVariable rangeVariable = new ResourceRangeVariable("$it", structuredType.Object, navigationSource.Object);
            ResourceRangeVariableReferenceNode source = new ResourceRangeVariableReferenceNode("$it", rangeVariable);

            Mock<IEdmComplexTypeReference> complexType = new Mock<IEdmComplexTypeReference>();

            Mock<IEdmProperty> complexProperty = new Mock<IEdmProperty>();
            complexProperty.Setup(p => p.Name).Returns("Address");
            complexProperty.Setup(p => p.PropertyKind).Returns(EdmPropertyKind.Structural);
            complexProperty.Setup(p => p.Type).Returns(complexType.Object);

            SingleComplexNode complexNode = new SingleComplexNode(source, complexProperty.Object);

            SingleValueOpenPropertyAccessNode node = new SingleValueOpenPropertyAccessNode(complexNode, "ZipCode");

            // Act
            string fullPropertyPath = QueryBinder.GetFullPropertyPath(node);

            // Assert
            Assert.Equal("Address\\ZipCode", fullPropertyPath);
        }
    }

    public class MyQueryBinder : QueryBinder
    {
        public static PropertyInfo Call_GetDynamicPropertyContainer(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
        {
            return GetDynamicPropertyContainer(openNode, context);
        }
    }
}
