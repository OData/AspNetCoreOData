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
using Moq;
using Xunit;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Tests.Commons;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Tests.Models;
using Microsoft.AspNetCore.OData.TestCommon;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions;

/// <summary>
/// Tests to QueryBinder
/// </summary>
public class QueryBinderTests
{
    #region Create the model
    private static readonly IEdmModel TestModel = BuildAndGetEdmModel();
    #endregion

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

    public static TheoryDataSet<List<QueryNode>> BindSingleResourceFunctionCallNodeForEntityTypCasting_Data
    {
        get
        {
            // Get the entity type for the Manager entity -> Manager is derived from Employee
            var managerType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Manager");

            // Create a ResourceRangeVariableReferenceNode for the Employee entity
            var employeeNode = CreateEmployeeRangeVariableReferenceNode();

            return new TheoryDataSet<List<QueryNode>>()
            {
                {
                    // Create a ConstantNode to cast Employee to Microsoft.AspNetCore.OData.Tests.Models.Manager
                    // This represents the quoted type parameter for cast function. For example: cast('Microsoft.AspNetCore.OData.Tests.Models.Manager')
                    new List<QueryNode>()
                    {
                        employeeNode,
                        new ConstantNode("Microsoft.AspNetCore.OData.Tests.Models.Manager")
                    }
                },
                {
                    // Create a SingleResourceCastNode to cast Employee to Microsoft.AspNetCore.OData.Tests.Models.Manager
                    // This represents the unquoted type parameter for cast function. For example: cast(Microsoft.AspNetCore.OData.Tests.Models.Manager)
                    new List<QueryNode>()
                    {
                        employeeNode,
                        new SingleResourceCastNode(employeeNode as SingleResourceNode, managerType)
                    }
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(BindSingleResourceFunctionCallNodeForEntityTypCasting_Data))]
    public void BindSingleResourceFunctionCallNode_CastingEntityType_ReturnsExpression(List<QueryNode> parameters)
    {
        // Arrange
        var binder = new MyQueryBinder();

        // Create the type reference and navigation source
        var employeeType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Employee");
        var collectionNode = MockCollectionResourceNode.CreateFakeNodeForEmployee();

        // Get the entity type for the Manager entity -> Manager is derived from Employee
        var managerType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Manager");

        // Create the SingleResourceFunctionCallNode
        var node = new SingleResourceFunctionCallNode("cast", parameters, collectionNode.ItemStructuredType, collectionNode.NavigationSource);

        // Create an instance of QueryBinderContext using the real model and settings
        Type clrType = TestModel.GetClrType(employeeType);
        var context = new QueryBinderContext(TestModel, new ODataQuerySettings(), clrType);

        // Act
        Expression expression = binder.BindSingleResourceFunctionCallNode(node, context);

        // Assert
        Assert.NotNull(expression);
        // cast($it, 'Microsoft.AspNetCore.OData.Tests.Models.Manager') where $it is the Employee entity
        Assert.Equal("($it As Manager)", expression.ToString());
        Assert.Equal("Microsoft.AspNetCore.OData.Tests.Models.Manager", expression.Type.ToString());
        Assert.Equal(typeof(Manager), expression.Type);
        Assert.Equal(ExpressionType.TypeAs, expression.NodeType);
        Assert.Equal(employeeType.FullName(), (expression as UnaryExpression).Operand.Type.FullName);
        Assert.Equal(managerType.FullName(), (expression as UnaryExpression).Type.FullName);
    }

    public static TheoryDataSet<List<QueryNode>> BindSingleResourceFunctionCallNodeForPropertyCasting_Data
    {
        get
        {
            var addressType = GetEdmComplexTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Address");
            var workAddressType = GetEdmComplexTypeFor("Microsoft.AspNetCore.OData.Tests.Models.WorkAddress");

            // Create a ResourceRangeVariableReferenceNode for the Employee entity
            var employeeNode = CreateEmployeeRangeVariableReferenceNode();

            // Create a SingleComplexNode for the Location property of the Person entity
            var locationProperty = GetEdmPropertyFor("Microsoft.AspNetCore.OData.Tests.Models.Employee", "Location");
            var locationNode = new SingleComplexNode(employeeNode as SingleResourceNode, locationProperty);

            return new TheoryDataSet<List<QueryNode>>()
            {
                {
                    // Create a ConstantNode to cast Location to NS.WorkAddress
                    // This represents the quoted type parameter for cast function. For example: cast($it.Location, 'Microsoft.AspNetCore.OData.Tests.Models.WorkAddress')
                    new List<QueryNode>()
                    {
                        // First parameter is the Location property
                        locationNode, 
                        // Second parameter is the ConstantNode
                        new ConstantNode("Microsoft.AspNetCore.OData.Tests.Models.WorkAddress")
                    }
                },
                {
                    // Create a SingleResourceCastNode to cast Location to Microsoft.AspNetCore.OData.Tests.Models.WorkAddress
                    // This represents the unquoted type parameter for cast function. For example: cast($it.Location, Microsoft.AspNetCore.OData.Tests.Models.WorkAddress)
                    new List<QueryNode>()
                    {
                        locationNode,
                        new SingleResourceCastNode(locationNode, workAddressType)
                    }
                }
            };
        }
    }

    [Theory]
    [MemberData(nameof(BindSingleResourceFunctionCallNodeForPropertyCasting_Data))]
    public void BindSingleResourceFunctionCallNode_PropertyCasting_ReturnsExpression(List<QueryNode> parameters)
    {
        // Arrange
        var binder = new MyQueryBinder();

        // Create the type reference and navigation source
        var employeeType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Employee");
        var employeeTypeRef = GetEntityTypeReferenceFor(employeeType);
        var collectionNode = MockCollectionResourceNode.CreateFakeNodeForEmployee();

        var addressType = GetEdmComplexTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Address");
        var workAddressType = GetEdmComplexTypeFor("Microsoft.AspNetCore.OData.Tests.Models.WorkAddress");

        // Create a ResourceRangeVariableReferenceNode for the Employee entity
        var rangeVariable = new ResourceRangeVariable("$it", employeeTypeRef, collectionNode);
        var employeeNode = new ResourceRangeVariableReferenceNode(rangeVariable.Name, rangeVariable) as SingleValueNode;

        // Create a SingleComplexNode for the Location property of the Person entity
        var locationProperty = GetEdmPropertyFor("Microsoft.AspNetCore.OData.Tests.Models.Employee", "Location");
        var locationNode = new SingleComplexNode(employeeNode as SingleResourceNode, locationProperty);

        // Create the SingleResourceFunctionCallNode
        var node = new SingleResourceFunctionCallNode("cast", parameters, collectionNode.ItemStructuredType, collectionNode.NavigationSource);

        // Create an instance of QueryBinderContext using the real model and settings
        Type clrType = TestModel.GetClrType(employeeType);
        var context = new QueryBinderContext(TestModel, new ODataQuerySettings(), clrType);

        // Act
        Expression expression = binder.BindSingleResourceFunctionCallNode(node, context);

        // Assert
        Assert.NotNull(expression);
        // cast($it.Location, 'Microsoft.AspNetCore.OData.Tests.Models.WorkAddress') where $it is the Employee entity
        Assert.Equal("($it.Location As WorkAddress)", expression.ToString());
        Assert.Equal("Microsoft.AspNetCore.OData.Tests.Models.WorkAddress", expression.Type.ToString());
        Assert.Equal("Location", ((expression as UnaryExpression).Operand as MemberExpression).Member.Name);
        Assert.Equal(typeof(WorkAddress), expression.Type);
        Assert.Equal(ExpressionType.TypeAs, expression.NodeType);
        Assert.Equal(workAddressType.FullName(), (expression as UnaryExpression).Type.FullName);
        Assert.Equal(addressType.FullName(), (expression as UnaryExpression).Operand.Type.FullName);
    }

    [Fact]
    public void BindSingleResourceFunctionCallNode_ThrowsNotSupported_ForNotAcceptParameterQueryNode()
    {
        // Arrange
        var binder = new MyQueryBinder();

        // Create the type reference and navigation source
        var employeeType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Employee");
        var employeeTypeRef = GetEntityTypeReferenceFor(employeeType);
        var collectionNode = MockCollectionResourceNode.CreateFakeNodeForEmployee();

        // Get the entity type for the Manager entity -> Manager is derived from Employee
        var managerType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Manager");

        // Create a ResourceRangeVariableReferenceNode for the Employee entity
        var rangeVariable = new ResourceRangeVariable("$it", employeeTypeRef, collectionNode);
        var employeeNode = new ResourceRangeVariableReferenceNode(rangeVariable.Name, rangeVariable) as SingleValueNode;

        // Create a SingleValuePropertyAccessNode for the EmployeeID property of the Person entity
        var employeeIDProperty = GetEdmPropertyFor("Microsoft.AspNetCore.OData.Tests.Models.Employee", "EmployeeID");
        var employeeIDNode = new SingleValuePropertyAccessNode(employeeNode, employeeIDProperty);

        // Create ConvertNode that is not SingleResourceNode or ConstantNode
        var edmTypeReference = EdmCoreModel.Instance.FindDeclaredType("Edm.Int32").ToEdmTypeReference(false);
        var convertNode = new ConvertNode(employeeIDNode, edmTypeReference);

        // Create the parameters list
        int capacity = 2;
        var parameters = new List<QueryNode>(capacity)
        {
            employeeNode, // First parameter is the Person entity,
            convertNode // Second parameter is the ConvertNode
        };

        // Create the SingleResourceFunctionCallNode
        var node = new SingleResourceFunctionCallNode("cast", parameters, collectionNode.ItemStructuredType, collectionNode.NavigationSource);

        // Create an instance of QueryBinderContext using the real model and settings
        Type clrType = TestModel.GetClrType(employeeType);
        var context = new QueryBinderContext(TestModel, new ODataQuerySettings(), clrType);

        // Act & Assert
        ExceptionAssert.Throws<NotSupportedException>(() => binder.BindSingleResourceFunctionCallNode(node, context),
            "Binding OData QueryNode of kind 'Convert' is not supported by 'BindSingleResourceCastFunctionCall'.");
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

    private static SingleValueNode CreateEmployeeRangeVariableReferenceNode()
    {
        // Create the type reference and navigation source
        var employeeType = GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Employee");
        var employeeTypeRef = GetEntityTypeReferenceFor(employeeType);
        var collectionNode = MockCollectionResourceNode.CreateFakeNodeForEmployee();

        // Create a ResourceRangeVariableReferenceNode for the Employee entity
        var rangeVariable = new ResourceRangeVariable("$it", employeeTypeRef, collectionNode);
        return new ResourceRangeVariableReferenceNode(rangeVariable.Name, rangeVariable);
    }

    #region Helper methods
    public static IEdmEntityType GetEntityTypeFor(string entityQualifiedName)
    {
        return TestModel.FindDeclaredType(entityQualifiedName) as IEdmEntityType;
    }

    public static IEdmComplexType GetEdmComplexTypeFor(string complexTypeQualifiedName)
    {
        return TestModel.FindDeclaredType(complexTypeQualifiedName) as IEdmComplexType;
    }

    public static IEdmEntityTypeReference GetEntityTypeReferenceFor(IEdmEntityType entityType)
    {
        return new EdmEntityTypeReference(entityType, false);
    }

    public static IEdmProperty GetEdmPropertyFor(string entityQualifiedName, string propertyName)
    {
        return GetEntityTypeFor(entityQualifiedName).FindProperty(propertyName);
    }

    public static IEdmEntitySet GetEntitySetFor(string entityName)
    {
        return TestModel.EntityContainer.FindEntitySet(entityName);
    }

    private static IEdmModel BuildAndGetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();
        builder.Namespace = "Microsoft.AspNetCore.OData.Tests.Models";
        builder.EntitySet<Employee>("Employees");
        builder.ComplexType<Address>();
        builder.ComplexType<WorkAddress>();
        builder.EntitySet<Manager>("Managers");

        return builder.GetEdmModel();
    }
    #endregion
}

public class MyQueryBinder : QueryBinder
{
    public static PropertyInfo Call_GetDynamicPropertyContainer(SingleValueOpenPropertyAccessNode openNode, QueryBinderContext context)
    {
        return GetDynamicPropertyContainer(openNode, context);
    }
}

#region Mock Single Entity Node
public class MockSingleEntityNode : SingleEntityNode
{
    private readonly IEdmEntityTypeReference typeReference;
    private readonly IEdmEntitySetBase set;

    public MockSingleEntityNode(IEdmEntityTypeReference type, IEdmEntitySetBase set)
    {
        this.typeReference = type;
        this.set = set;
    }

    public override IEdmTypeReference TypeReference
    {
        get { return this.typeReference; }
    }

    public override IEdmNavigationSource NavigationSource
    {
        get { return this.set; }
    }

    public override IEdmStructuredTypeReference StructuredTypeReference
    {
        get { return this.typeReference; }
    }

    public override IEdmEntityTypeReference EntityTypeReference
    {
        get { return this.typeReference; }
    }

    public static MockSingleEntityNode CreateFakeNodeForEmployee()
    {
        var employeeType = QueryBinderTests.GetEntityTypeFor("Microsoft.AspNetCore.OData.Tests.Models.Employee");
        return new MockSingleEntityNode(QueryBinderTests.GetEntityTypeReferenceFor(employeeType), QueryBinderTests.GetEntitySetFor("Employees"));
    }
}
#endregion

#region Mock Collection Resource Node
public class MockCollectionResourceNode : CollectionResourceNode
{
    private readonly IEdmStructuredTypeReference _typeReference;
    private readonly IEdmNavigationSource _source;
    private readonly IEdmTypeReference _itemType;
    private readonly IEdmCollectionTypeReference _collectionType;

    public MockCollectionResourceNode(IEdmStructuredTypeReference type, IEdmNavigationSource source, IEdmTypeReference itemType, IEdmCollectionTypeReference collectionType)
    {
        _typeReference = type;
        _source = source;
        _itemType = itemType;
        _collectionType = collectionType;
    }

    public override IEdmStructuredTypeReference ItemStructuredType => _typeReference;

    public override IEdmNavigationSource NavigationSource => _source;

    public override IEdmTypeReference ItemType => _itemType;

    public override IEdmCollectionTypeReference CollectionType => _collectionType;

    public static MockCollectionResourceNode CreateFakeNodeForEmployee()
    {
        var singleEntityNode = MockSingleEntityNode.CreateFakeNodeForEmployee();
        return new MockCollectionResourceNode(
            singleEntityNode.EntityTypeReference, singleEntityNode.NavigationSource, singleEntityNode.EntityTypeReference, singleEntityNode.EntityTypeReference.AsCollection());
    }
}
#endregion
