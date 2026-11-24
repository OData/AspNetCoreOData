//-----------------------------------------------------------------------------
// <copyright file="QueryValidatorHelpersTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;
using Xunit;

namespace Microsoft.AspNetCore.OData.Tests.Query.Validator;

public class QueryValidatorHelpersTests
{
    [Fact]
    public void AnyAllExpressionDepth_AllowsAbsentAndEmptyClauses()
    {
        var settings = new ODataValidationSettings();

        QueryValidatorHelpers.ValidateAnyAllExpressionDepth((QueryNode)null, settings);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth((OrderByClause)null, settings);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth((ComputeClause)null, settings);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(new ComputeClause(Array.Empty<ComputeExpression>()), settings);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AnyAllExpressionDepth_ParsedCollectionsDoNotAllocateDuringTraversal(bool compute)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var parser = new ODataQueryOptionParser(context.Model, context.ElementType, context.NavigationSource, new Dictionary<string, string> { { "$orderby", "concat(tolower(Name),toupper(Name))" }, { "$compute", "tolower(Name) as Lower,toupper(Name) as Upper" } });
        var orderByClause = parser.ParseOrderBy();
        var computeClause = parser.ParseCompute();
        var settings = new ODataValidationSettings();
        for (int i = 0; i < 64; i++)
        {
            if (compute)
            {
                QueryValidatorHelpers.ValidateAnyAllExpressionDepth(computeClause, settings);
            }
            else
            {
                QueryValidatorHelpers.ValidateAnyAllExpressionDepth(orderByClause, settings);
            }
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 64; i++)
        {
            if (compute)
            {
                QueryValidatorHelpers.ValidateAnyAllExpressionDepth(computeClause, settings);
            }
            else
            {
                QueryValidatorHelpers.ValidateAnyAllExpressionDepth(orderByClause, settings);
            }
        }
        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void AnyAllExpressionDepth_DeferredFunctionArgumentsRemainIndependentAndReEnumerable()
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var oneLevel = new FilterQueryOption("Contacts/any(a:a/Name eq 'x')", context).FilterClause.Expression;
        var nested = new FilterQueryOption("Contacts/any(a:a/Contacts/all(b:b/Name eq 'x'))", context).FilterClause.Expression;
        int visited = 0;
        var arguments = new QueryNode[] { oneLevel, oneLevel, new NamedFunctionParameterNode("value", nested) }.Select(argument => { visited++; return argument; });
        var node = new SingleValueFunctionCallNode("test", Array.Empty<IEdmFunction>(), arguments, EdmCoreModel.Instance.GetBoolean(false), null);
        var settings = new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 };

        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings);
        Assert.Equal(3, visited);
        settings.MaxAnyAllExpressionDepth = 1;
        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings));
        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        Assert.Equal(6, visited);
        settings.MaxAnyAllExpressionDepth = 2;
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings);
        Assert.Equal(9, visited);
    }

    [Fact]
    public void AnyAllExpressionDepth_CountFilterChecksOnlyDepth()
    {
        var context = ValidationTestHelper.CreateDerivedProductsContext();
        var settings = new ODataValidationSettings { AllowedFunctions = AllowedFunctions.None, AllowedLogicalOperators = AllowedLogicalOperators.None, MaxNodeCount = 1 };
        var withinLimit = new ComputeQueryOption("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any(a:a/ProductName eq 'x')) as Total", context);
        var overLimit = new ComputeQueryOption("Category/QueryableProducts/$count($filter=Category/EnumerableProducts/any(a:a/Category/EnumerableProducts/all(b:b/ProductName eq 'x'))) as Total", context);

        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(withinLimit.ComputeClause, settings);
        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(overLimit.ComputeClause, settings));
        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        settings.MaxAnyAllExpressionDepth = 2;
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(overLimit.ComputeClause, settings);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(int.MaxValue, int.MaxValue - 1)]
    public void AnyAllExpressionDepth_EmptyAnyHonorsInheritedDepthAndIntegerBoundary(int maximum, int currentDepth)
    {
        var node = new AnyNode(new Collection<RangeVariable>()) { Body = new ConstantNode(true) };
        var settings = new ODataValidationSettings { MaxAnyAllExpressionDepth = maximum };

        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings, currentDepth);
        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings, currentDepth + 1));

        Assert.Contains($"The Any/All nesting limit of '{maximum}' has been exceeded.", error.Message);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings, currentDepth);
    }

    [Theory]
    [InlineData("not Contacts/any(a:a/Contacts/all(b:b/Name eq 'x'))")]
    [InlineData("Contacts/all(a:a/Contacts/any(b:b/Name eq 'x')) in (true)")]
    [InlineData("Contacts/all(a:a/Contacts/all(b:b/Name eq 'x')) eq true")]
    [InlineData("cast(Contacts/any(a:a/Contacts/any(b:b/Name eq 'x')),Edm.Boolean)")]
    [InlineData("Contacts/$count($filter=Contacts/any(a:a/Contacts/any(b:b/Name eq 'x'))) gt 0")]
    [InlineData("Contacts/any(a:a/Contacts/$count($filter=Contacts/any(b:b/Name eq 'x')) gt 0)")]
    public void AnyAllExpressionDepth_TraversesWrappedExpressionsAtInheritedDepth(string expression)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var node = new FilterQueryOption(expression, context).FilterClause.Expression;
        var settings = new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 };

        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings);
        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings, 1));

        Assert.Contains("The Any/All nesting limit of '2' has been exceeded.", error.Message);
    }

    [Theory]
    [InlineData("singleValue", false)]
    [InlineData("singleValue", true)]
    [InlineData("collection", false)]
    [InlineData("collection", true)]
    [InlineData("resource", false)]
    [InlineData("resource", true)]
    [InlineData("resources", false)]
    [InlineData("resources", true)]
    public void AnyAllExpressionDepth_TraversesFunctionSourcesAndNamedArguments(string kind, bool useSource)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var lambda = new FilterQueryOption("Contacts/any(a:a/Contacts/any(b:b/Name eq 'x'))", context).FilterClause.Expression;
        var entityType = context.Model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "QueryCompositionCustomer");
        var entityReference = new EdmEntityTypeReference(entityType, false);
        var primitiveCollection = new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetBoolean(false)));
        var entityCollection = new EdmCollectionTypeReference(new EdmCollectionType(entityReference));
        QueryNode source = useSource ? lambda : null;
        QueryNode[] arguments = useSource ? null : new QueryNode[] { new ConstantNode(true), new NamedFunctionParameterNode("value", lambda) };
        QueryNode node = kind switch
        {
            "singleValue" => new SingleValueFunctionCallNode("test", Array.Empty<IEdmFunction>(), arguments, EdmCoreModel.Instance.GetBoolean(false), source),
            "collection" => new CollectionFunctionCallNode("test", Array.Empty<IEdmFunction>(), arguments, primitiveCollection, source),
            "resource" => new SingleResourceFunctionCallNode("test", Array.Empty<IEdmFunction>(), arguments, entityReference, null, source),
            _ => new CollectionResourceFunctionCallNode("test", Array.Empty<IEdmFunction>(), arguments, entityCollection, null, source)
        };

        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, new ODataValidationSettings()));

        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AnyAllExpressionDepth_TraversesOpenPropertySources(bool collection)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var lambda = new FilterQueryOption("Contacts/any(a:a/Contacts/any(b:b/Name eq 'x'))", context).FilterClause.Expression;
        QueryNode node = collection ? new CollectionOpenPropertyAccessNode(lambda, "Values") : new SingleValueOpenPropertyAccessNode(lambda, "Value");

        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, new ODataValidationSettings()));

        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 });
    }

    [Theory]
    [InlineData(nameof(SingleValuePropertyAccessNode))]
    [InlineData(nameof(CollectionPropertyAccessNode))]
    [InlineData(nameof(SingleComplexNode))]
    [InlineData(nameof(CollectionComplexNode))]
    [InlineData(nameof(SingleNavigationNode))]
    [InlineData(nameof(CollectionNavigationNode))]
    [InlineData(nameof(SingleResourceCastNode))]
    [InlineData(nameof(CollectionResourceCastNode))]
    [InlineData(nameof(AggregatedCollectionPropertyNode))]
    public void AnyAllExpressionDepth_TraversesTypedSourcesAtInheritedDepth(string kind)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var lambda = new FilterQueryOption("Contacts/any(a:a/Contacts/any(b:b/Name eq 'x'))", context).FilterClause.Expression;
        var customer = Assert.IsType<EdmEntityType>(context.Model.SchemaElements.OfType<IEdmEntityType>().Single(t => t.Name == "QueryCompositionCustomer"));
        var entityReference = new EdmEntityTypeReference(customer, false);
        var source = new SingleResourceFunctionCallNode("test", new QueryNode[] { lambda }, entityReference, null);
        var contacts = new CollectionNavigationNode(source, (IEdmNavigationProperty)customer.FindProperty("Contacts"), null);
        var address = customer.FindProperty("Address");
        var addresses = customer.AddStructuralProperty("Addresses", new EdmCollectionTypeReference(new EdmCollectionType(address.Type)));
        QueryNode node = kind switch
        {
            nameof(SingleValuePropertyAccessNode) => new SingleValuePropertyAccessNode(source, customer.FindProperty("Name")),
            nameof(CollectionPropertyAccessNode) => new CollectionPropertyAccessNode(source, customer.FindProperty("Tags")),
            nameof(SingleComplexNode) => new SingleComplexNode(source, address),
            nameof(CollectionComplexNode) => new CollectionComplexNode(source, addresses),
            nameof(SingleNavigationNode) => new SingleNavigationNode(source, (IEdmNavigationProperty)customer.FindProperty("RelationshipManager"), null),
            nameof(CollectionNavigationNode) => contacts,
            nameof(SingleResourceCastNode) => new SingleResourceCastNode(source, customer),
            nameof(CollectionResourceCastNode) => new CollectionResourceCastNode(contacts, customer),
            _ => new AggregatedCollectionPropertyNode(contacts, customer.FindProperty("AmountSpent"))
        };
        var settings = new ODataValidationSettings();

        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings));

        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        settings.MaxAnyAllExpressionDepth = 2;
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings);
        var inheritedError = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, settings, 1));
        Assert.Contains("The Any/All nesting limit of '2' has been exceeded.", inheritedError.Message);
    }

    [Theory]
    [InlineData("binaryRight")]
    [InlineData("inRight")]
    [InlineData("countSource")]
    [InlineData("lambdaSource")]
    public void AnyAllExpressionDepth_TraversesEveryChildPosition(string position)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var oneLevel = new FilterQueryOption("Contacts/any(a:a/Name eq 'x')", context).FilterClause.Expression;
        var nested = new FilterQueryOption("Contacts/any(a:a/Contacts/any(b:b/Name eq 'x'))", context).FilterClause.Expression;
        var collectionType = new EdmCollectionTypeReference(new EdmCollectionType(EdmCoreModel.Instance.GetBoolean(false)));
        var source = new CollectionFunctionCallNode("test", Array.Empty<IEdmFunction>(), new QueryNode[] { position == "lambdaSource" ? oneLevel : nested }, collectionType, null);
        QueryNode node = position switch
        {
            "binaryRight" => new BinaryOperatorNode(BinaryOperatorKind.Equal, new ConstantNode(true), nested),
            "inRight" => new InNode(new ConstantNode(true), source),
            "countSource" => new CountNode(source),
            _ => new AnyNode(new Collection<RangeVariable>()) { Source = source, Body = new ConstantNode(true) }
        };

        var error = Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, new ODataValidationSettings()));

        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(node, new ODataValidationSettings { MaxAnyAllExpressionDepth = 2 });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AnyAllExpressionDepth_TraversesEveryClauseWithoutAccumulatingSiblingDepth(bool compute)
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var node = new FilterQueryOption("Contacts/any(a:a/Name eq 'x')", context).FilterClause.Expression;
        var rangeVariable = new FilterQueryOption("true", context).FilterClause.RangeVariable;
        var settings = new ODataValidationSettings();
        var nested = new AnyNode(new Collection<RangeVariable>()) { Body = node };
        var computeClause = new ComputeClause(new[] { new ComputeExpression(node, "First", node.TypeReference), new ComputeExpression(node, "Second", node.TypeReference), new ComputeExpression(nested, "Third", nested.TypeReference) });
        var orderByClause = new OrderByClause(new OrderByClause(null, nested, OrderByDirection.Ascending, rangeVariable), node, OrderByDirection.Ascending, rangeVariable);
        var error = compute
            ? Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(computeClause, settings))
            : Assert.Throws<ODataException>(() => QueryValidatorHelpers.ValidateAnyAllExpressionDepth(orderByClause, settings));

        Assert.Contains("The Any/All nesting limit of '1' has been exceeded.", error.Message);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(new ComputeClause(new[] { new ComputeExpression(node, "First", node.TypeReference), new ComputeExpression(node, "Second", node.TypeReference) }), settings);
        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(new OrderByClause(new OrderByClause(null, node, OrderByDirection.Ascending, rangeVariable), node, OrderByDirection.Ascending, rangeVariable), settings);
    }

    [Fact]
    public void AnyAllExpressionDepth_AllowsVirtualAggregateCount()
    {
        var context = ValidationTestHelper.CreateCustomerContext();
        var option = new ApplyQueryOption("aggregate($count as Total)", context);
        var transformation = Assert.IsType<AggregateTransformationNode>(option.ApplyClause.Transformations.Single());
        var expression = Assert.IsType<AggregateExpression>(transformation.AggregateExpressions.Single());
        Assert.Equal(QueryNodeKind.Count, expression.Expression.Kind);
        Assert.False(expression.Expression is CountNode);

        QueryValidatorHelpers.ValidateAnyAllExpressionDepth(expression.Expression, new ODataValidationSettings());
    }
}
