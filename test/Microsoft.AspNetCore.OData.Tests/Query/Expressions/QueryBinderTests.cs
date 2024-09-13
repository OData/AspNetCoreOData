//-----------------------------------------------------------------------------
// <copyright file="QueryBinderTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.FullyQualified.NS;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

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

    [Theory]
    [InlineData(typeof(ConstantNode))]
    [InlineData(typeof(SingleResourceCastNode))]
    public void BindSingleResourceFunctionCallNode_CastingEntityType_ReturnsExpression(Type queryNodeType)
    {
        // Arrange
        var binder = new MyQueryBinder();

        var model = HardCodedTestModel.TestModel;

        // Create the type reference and navigation source
        var personType = HardCodedTestModel.GetEntityType("Microsoft.FullyQualified.NS.Person");
        var personTypeRef = HardCodedTestModel.GetEntityTypeReference(personType);
        var collectionNode = FakeCollectionResourceNode.CreateFakeNodeForPerson();

        var employeeType = HardCodedTestModel.GetEntityType("Microsoft.FullyQualified.NS.Employee");

        // Create a ResourceRangeVariableReferenceNode for the Person entity
        var rangeVariable = new ResourceRangeVariable("$it", personTypeRef, collectionNode);
        var personNode = new ResourceRangeVariableReferenceNode(rangeVariable.Name, rangeVariable) as SingleValueNode;

        // Create the parameters list
        int capacity = 2;
        var parameters = new List<QueryNode>(capacity)
        {
            personNode // First parameter is the Person entity
        };

        if (queryNodeType == typeof(SingleResourceCastNode))
        {
            // Create a SingleResourceCastNode to cast Person to NS.Employee
            var singleResourceCastNode = new SingleResourceCastNode(personNode as SingleResourceNode, employeeType);
            parameters.Add(singleResourceCastNode); // Second parameter is the SingleResourceCastNode
        }
        else if (queryNodeType == typeof(ConstantNode))
        {
            // Create a ConstantNode to cast Person to NS.Employee
            var constantNode = new ConstantNode("Microsoft.FullyQualified.NS.Employee");
            parameters.Add(constantNode); // Second parameter is the ConstantNode
        }

        // Create the SingleResourceFunctionCallNode
        var node = new SingleResourceFunctionCallNode("cast", parameters, collectionNode.ItemStructuredType, collectionNode.NavigationSource);

        // Create an instance of QueryBinderContext using the real model and settings
        Type clrType = model.GetClrType(personType);
        var context = new QueryBinderContext(model, new ODataQuerySettings(), clrType);

        // Act
        Expression expression = binder.BindSingleResourceFunctionCallNode(node, context);

        // Assert
        Assert.NotNull(expression);
        Assert.Equal("($it As Employee)", expression.ToString());
        Assert.Equal("Microsoft.FullyQualified.NS.Employee", expression.Type.ToString());
        Assert.Equal(typeof(Employee), expression.Type);
        Assert.Equal(ExpressionType.TypeAs, expression.NodeType);
        Assert.Equal(personType.FullName(), (expression as UnaryExpression).Operand.Type.FullName);
        Assert.Equal(employeeType.FullName(), (expression as UnaryExpression).Type.FullName);
    }

    [Theory]
    [InlineData(typeof(ConstantNode))]
    [InlineData(typeof(SingleResourceCastNode))]
    public void BindSingleResourceFunctionCallNode_PropertyCasting_ReturnsExpression(Type queryNodeType)
    {
        // Arrange
        var binder = new MyQueryBinder();

        var model = HardCodedTestModel.TestModel;

        // Create the type reference and navigation source
        var personType = HardCodedTestModel.GetEntityType("Microsoft.FullyQualified.NS.Person");
        var personTypeRef = HardCodedTestModel.GetEntityTypeReference(personType);
        var collectionNode = FakeCollectionResourceNode.CreateFakeNodeForPerson();

        var myAddressType = HardCodedTestModel.GetEdmComplexType("Microsoft.FullyQualified.NS.MyAddress");
        var workAddressType = HardCodedTestModel.GetEdmComplexType("Microsoft.FullyQualified.NS.WorkAddress");

        // Create a ResourceRangeVariableReferenceNode for the Person entity
        var rangeVariable = new ResourceRangeVariable("$it", personTypeRef, collectionNode);
        var personNode = new ResourceRangeVariableReferenceNode(rangeVariable.Name, rangeVariable) as SingleValueNode;

        // Create a SingleComplexNode for the Location property of the Person entity
        var locationProperty = HardCodedTestModel.GetPersonLocationProperty();
        var locationNode = new SingleComplexNode(personNode as SingleResourceNode, locationProperty);

        // Create the parameters list
        int capacity = 2;
        var parameters = new List<QueryNode>(capacity)
        {
            locationNode // First parameter is the Location property
        };

        if(queryNodeType == typeof(SingleResourceCastNode))
        {
            // Create a SingleResourceCastNode to cast Location to NS.WorkAddress
            var singleResourceCastNode = new SingleResourceCastNode(locationNode, workAddressType);
            parameters.Add(singleResourceCastNode); // Second parameter is the SingleResourceCastNode
        }
        else if (queryNodeType == typeof(ConstantNode))
        {
            // Create a ConstantNode to cast Location to NS.WorkAddress
            var constantNode = new ConstantNode("Microsoft.FullyQualified.NS.WorkAddress");
            parameters.Add(constantNode); // Second parameter is the ConstantNode
        }

        // Create the SingleResourceFunctionCallNode
        var node = new SingleResourceFunctionCallNode("cast", parameters, collectionNode.ItemStructuredType, collectionNode.NavigationSource);

        // Create an instance of QueryBinderContext using the real model and settings
        Type clrType = model.GetClrType(personType);
        var context = new QueryBinderContext(model, new ODataQuerySettings(), clrType);

        // Act
        Expression expression = binder.BindSingleResourceFunctionCallNode(node, context);

        // Assert
        Assert.NotNull(expression);
        Assert.Equal("($it.Location As WorkAddress)", expression.ToString());
        Assert.Equal("Microsoft.FullyQualified.NS.WorkAddress", expression.Type.ToString());
        Assert.Equal("Location", ((expression as UnaryExpression).Operand as MemberExpression).Member.Name);
        Assert.Equal(typeof(WorkAddress), expression.Type);
        Assert.Equal(ExpressionType.TypeAs, expression.NodeType);
        Assert.Equal(workAddressType.FullName(), (expression as UnaryExpression).Type.FullName);
        Assert.Equal(myAddressType.FullName(), (expression as UnaryExpression).Operand.Type.FullName);
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
}

public class MyQueryBinder : QueryBinder
{
    public static PropertyInfo Call_GetDynamicPropertyContainer(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
    {
        return GetDynamicPropertyContainer(openNode, context);
    }
}
