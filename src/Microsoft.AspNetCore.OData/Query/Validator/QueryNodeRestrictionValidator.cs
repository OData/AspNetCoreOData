//-----------------------------------------------------------------------------
// <copyright file="QueryNodeRestrictionValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Walks the query nodes referenced by <c>$apply</c> (groupby/aggregate/compute) and top-level
/// <c>$compute</c> expressions and rejects any property that the model marks as not filterable or
/// configures as not selectable, so those properties are enforced consistently with how they are
/// enforced for <c>$filter</c> and <c>$select</c>. The same walk also enforces the operator and
/// function allow-lists and the node-count limit from <see cref="ODataValidationSettings"/>, so those
/// limits apply to the groupby/aggregate/compute/$compute expressions exactly as they do to <c>$filter</c>.
/// </summary>
internal static class QueryNodeRestrictionValidator
{
    /// <summary>
    /// Validates a single <see cref="QueryNode"/> and its descendants.
    /// </summary>
    /// <param name="node">The query node to validate. A <c>null</c> node is a no-op.</param>
    /// <param name="context">The query context used to resolve the model and query configurations.</param>
    /// <param name="validationSettings">
    /// The validation settings whose operator/function allow-lists and node-count/depth limits are enforced.
    /// </param>
    internal static void Validate(QueryNode node, ODataQueryContext context, ODataValidationSettings validationSettings)
    {
        // A FilterValidatorContext carries the same node-count / function-call-depth / lambda-depth
        // bookkeeping used by $filter, so the compute/aggregate/groupby walk enforces MaxNodeCount,
        // MaxFunctionCallDepth and MaxAnyAllExpressionDepth identically.
        FilterValidatorContext validatorContext = new FilterValidatorContext
        {
            Context = context,
            ValidationSettings = validationSettings,
            Property = context.TargetProperty,
            StructuredType = context.TargetStructuredType,
            CurrentDepth = 0
        };

        Validate(node, validatorContext);
    }

    private static void Validate(QueryNode node, FilterValidatorContext validatorContext)
    {
        if (node == null)
        {
            return;
        }

        ODataQueryContext context = validatorContext.Context;
        ODataValidationSettings validationSettings = validatorContext.ValidationSettings;

        // Count every visited node against MaxNodeCount, consistent with $filter.
        validatorContext.IncrementNodeCount();

        switch (node.Kind)
        {
            case QueryNodeKind.BinaryOperator:
                BinaryOperatorNode binaryOperatorNode = (BinaryOperatorNode)node;
                FilterQueryValidator.ValidateBinaryOperatorAllowed(binaryOperatorNode, validationSettings);
                Validate(binaryOperatorNode.Left, validatorContext);
                Validate(binaryOperatorNode.Right, validatorContext);
                break;

            case QueryNodeKind.UnaryOperator:
                UnaryOperatorNode unaryOperatorNode = (UnaryOperatorNode)node;
                FilterQueryValidator.ValidateUnaryOperatorAllowed(unaryOperatorNode, validationSettings);
                Validate(unaryOperatorNode.Operand, validatorContext);
                break;

            case QueryNodeKind.Convert:
                Validate(((ConvertNode)node).Source, validatorContext);
                break;

            case QueryNodeKind.SingleValuePropertyAccess:
                SingleValuePropertyAccessNode singleValuePropertyAccessNode = (SingleValuePropertyAccessNode)node;
                CheckProperty(singleValuePropertyAccessNode.Property, context, checkSelectable: true);
                Validate(singleValuePropertyAccessNode.Source, validatorContext);
                break;

            case QueryNodeKind.CollectionPropertyAccess:
                CollectionPropertyAccessNode collectionPropertyAccessNode = (CollectionPropertyAccessNode)node;
                CheckProperty(collectionPropertyAccessNode.Property, context, checkSelectable: true);
                Validate(collectionPropertyAccessNode.Source, validatorContext);
                break;

            case QueryNodeKind.SingleComplexNode:
                SingleComplexNode singleComplexNode = (SingleComplexNode)node;
                CheckProperty(singleComplexNode.Property, context, checkSelectable: true);
                Validate(singleComplexNode.Source, validatorContext);
                break;

            case QueryNodeKind.CollectionComplexNode:
                CollectionComplexNode collectionComplexNode = (CollectionComplexNode)node;
                CheckProperty(collectionComplexNode.Property, context, checkSelectable: true);
                Validate(collectionComplexNode.Source, validatorContext);
                break;

            case QueryNodeKind.AggregatedCollectionPropertyNode:
                AggregatedCollectionPropertyNode aggregatedCollectionPropertyNode = (AggregatedCollectionPropertyNode)node;
                CheckProperty(aggregatedCollectionPropertyNode.Property, context, checkSelectable: true);
                Validate(aggregatedCollectionPropertyNode.Source, validatorContext);
                break;

            case QueryNodeKind.SingleNavigationNode:
                SingleNavigationNode singleNavigationNode = (SingleNavigationNode)node;
                CheckProperty(singleNavigationNode.NavigationProperty, context, checkSelectable: false);
                Validate(singleNavigationNode.Source, validatorContext);
                break;

            case QueryNodeKind.CollectionNavigationNode:
                CollectionNavigationNode collectionNavigationNode = (CollectionNavigationNode)node;
                CheckProperty(collectionNavigationNode.NavigationProperty, context, checkSelectable: false);
                Validate(collectionNavigationNode.Source, validatorContext);
                break;

            case QueryNodeKind.SingleValueFunctionCall:
                ValidateFunctionCall(((SingleValueFunctionCallNode)node).Name, ((SingleValueFunctionCallNode)node).Parameters, validatorContext);
                break;

            case QueryNodeKind.CollectionFunctionCall:
                ValidateFunctionCall(((CollectionFunctionCallNode)node).Name, ((CollectionFunctionCallNode)node).Parameters, validatorContext);
                break;

            case QueryNodeKind.SingleResourceFunctionCall:
                ValidateFunctionCall(((SingleResourceFunctionCallNode)node).Name, ((SingleResourceFunctionCallNode)node).Parameters, validatorContext);
                break;

            case QueryNodeKind.CollectionResourceFunctionCall:
                ValidateFunctionCall(((CollectionResourceFunctionCallNode)node).Name, ((CollectionResourceFunctionCallNode)node).Parameters, validatorContext);
                break;

            case QueryNodeKind.Any:
                AnyNode anyNode = (AnyNode)node;
                FilterQueryValidator.ValidateFunctionAllowed("any", validationSettings);
                validatorContext.EnterLambda();
                try
                {
                    Validate(anyNode.Source, validatorContext);
                    Validate(anyNode.Body, validatorContext);
                }
                finally
                {
                    validatorContext.ExitLambda();
                }

                break;

            case QueryNodeKind.All:
                AllNode allNode = (AllNode)node;
                FilterQueryValidator.ValidateFunctionAllowed("all", validationSettings);
                validatorContext.EnterLambda();
                try
                {
                    Validate(allNode.Source, validatorContext);
                    Validate(allNode.Body, validatorContext);
                }
                finally
                {
                    validatorContext.ExitLambda();
                }

                break;

            case QueryNodeKind.SingleResourceCast:
                Validate(((SingleResourceCastNode)node).Source, validatorContext);
                break;

            case QueryNodeKind.CollectionResourceCast:
                Validate(((CollectionResourceCastNode)node).Source, validatorContext);
                break;

            case QueryNodeKind.In:
                InNode inNode = (InNode)node;
                Validate(inNode.Left, validatorContext);
                Validate(inNode.Right, validatorContext);
                break;

            case QueryNodeKind.Count:
                // A real CountNode ($count over a collection) exposes a Source to walk. The virtual
                // $count used inside aggregate($count as ...) reports the same Count kind but has no
                // underlying property to restrict, so it is left as a no-op.
                if (node is CountNode countNode)
                {
                    Validate(countNode.Source, validatorContext);
                }

                break;

            default:
                break;
        }
    }

    private static void ValidateFunctionCall(string functionName, IEnumerable<QueryNode> parameters, FilterValidatorContext validatorContext)
    {
        // Enforce the function allow-list and the function-call depth limit, consistent with $filter.
        FilterQueryValidator.ValidateFunctionAllowed(functionName, validatorContext.ValidationSettings);
        validatorContext.EnterFunctionCall();
        try
        {
            foreach (QueryNode parameter in parameters)
            {
                Validate(parameter, validatorContext);
            }
        }
        finally
        {
            validatorContext.ExitFunctionCall();
        }
    }

    private static void CheckProperty(IEdmProperty property, ODataQueryContext context, bool checkSelectable)
    {
        if (property == null)
        {
            return;
        }

        IEdmModel model = context.Model;

        // Enforce only the explicit, per-property restrictions ([NotFilterable]/[NotSelectable] and
        // model-bound disabled configurations) by passing the enable flags as true. This keeps the
        // groupby/aggregate/compute walk selective: a property that is not explicitly restricted stays
        // available regardless of the global EnableFilter/EnableSelect switches, so enabling this
        // validation is a no-op for unconfigured properties.
        //
        // The two path arguments are passed as null so each property is evaluated against the
        // restrictions declared on its own declaring type. That resolves the attribute-based
        // restrictions and the type/property-level model-bound configurations this walk targets;
        // a restriction scoped to a specific traversal path is intentionally not resolved here, so
        // the outcome stays consistent regardless of how the property was reached.
        if (EdmHelpers.IsNotFilterable(property, null, null, model, enableFilter: true))
        {
            throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter, property.Name));
        }

        if (checkSelectable && EdmHelpers.IsNotSelectable(property, null, null, model, enableSelect: true))
        {
            throw new ODataException(Error.Format(SRResources.NotSelectablePropertyUsedInSelect, property.Name));
        }
    }
}
