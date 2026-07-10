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

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Walks the query nodes referenced by <c>$apply</c> (groupby/aggregate/compute) and top-level
/// <c>$compute</c> expressions and rejects any property that the model marks as not filterable or
/// configures as not selectable, so those properties are enforced consistently with how they are
/// enforced for <c>$filter</c> and <c>$select</c>.
/// </summary>
internal static class QueryNodeRestrictionValidator
{
    /// <summary>
    /// Validates a single <see cref="QueryNode"/> and its descendants.
    /// </summary>
    /// <param name="node">The query node to validate. A <c>null</c> node is a no-op.</param>
    /// <param name="context">The query context used to resolve the model and query configurations.</param>
    internal static void Validate(QueryNode node, ODataQueryContext context)
    {
        if (node == null)
        {
            return;
        }

        switch (node.Kind)
        {
            case QueryNodeKind.BinaryOperator:
                BinaryOperatorNode binaryOperatorNode = (BinaryOperatorNode)node;
                Validate(binaryOperatorNode.Left, context);
                Validate(binaryOperatorNode.Right, context);
                break;

            case QueryNodeKind.UnaryOperator:
                Validate(((UnaryOperatorNode)node).Operand, context);
                break;

            case QueryNodeKind.Convert:
                Validate(((ConvertNode)node).Source, context);
                break;

            case QueryNodeKind.SingleValuePropertyAccess:
                SingleValuePropertyAccessNode singleValuePropertyAccessNode = (SingleValuePropertyAccessNode)node;
                CheckProperty(singleValuePropertyAccessNode.Property, context, checkSelectable: true);
                Validate(singleValuePropertyAccessNode.Source, context);
                break;

            case QueryNodeKind.CollectionPropertyAccess:
                CollectionPropertyAccessNode collectionPropertyAccessNode = (CollectionPropertyAccessNode)node;
                CheckProperty(collectionPropertyAccessNode.Property, context, checkSelectable: true);
                Validate(collectionPropertyAccessNode.Source, context);
                break;

            case QueryNodeKind.SingleComplexNode:
                SingleComplexNode singleComplexNode = (SingleComplexNode)node;
                CheckProperty(singleComplexNode.Property, context, checkSelectable: true);
                Validate(singleComplexNode.Source, context);
                break;

            case QueryNodeKind.CollectionComplexNode:
                CollectionComplexNode collectionComplexNode = (CollectionComplexNode)node;
                CheckProperty(collectionComplexNode.Property, context, checkSelectable: true);
                Validate(collectionComplexNode.Source, context);
                break;

            case QueryNodeKind.AggregatedCollectionPropertyNode:
                AggregatedCollectionPropertyNode aggregatedCollectionPropertyNode = (AggregatedCollectionPropertyNode)node;
                CheckProperty(aggregatedCollectionPropertyNode.Property, context, checkSelectable: true);
                Validate(aggregatedCollectionPropertyNode.Source, context);
                break;

            case QueryNodeKind.SingleNavigationNode:
                SingleNavigationNode singleNavigationNode = (SingleNavigationNode)node;
                CheckProperty(singleNavigationNode.NavigationProperty, context, checkSelectable: false);
                Validate(singleNavigationNode.Source, context);
                break;

            case QueryNodeKind.CollectionNavigationNode:
                CollectionNavigationNode collectionNavigationNode = (CollectionNavigationNode)node;
                CheckProperty(collectionNavigationNode.NavigationProperty, context, checkSelectable: false);
                Validate(collectionNavigationNode.Source, context);
                break;

            case QueryNodeKind.SingleValueFunctionCall:
                foreach (QueryNode parameter in ((SingleValueFunctionCallNode)node).Parameters)
                {
                    Validate(parameter, context);
                }
                break;

            case QueryNodeKind.CollectionFunctionCall:
                foreach (QueryNode parameter in ((CollectionFunctionCallNode)node).Parameters)
                {
                    Validate(parameter, context);
                }
                break;

            case QueryNodeKind.SingleResourceFunctionCall:
                foreach (QueryNode parameter in ((SingleResourceFunctionCallNode)node).Parameters)
                {
                    Validate(parameter, context);
                }
                break;

            case QueryNodeKind.CollectionResourceFunctionCall:
                foreach (QueryNode parameter in ((CollectionResourceFunctionCallNode)node).Parameters)
                {
                    Validate(parameter, context);
                }
                break;

            case QueryNodeKind.Any:
                AnyNode anyNode = (AnyNode)node;
                Validate(anyNode.Source, context);
                Validate(anyNode.Body, context);
                break;

            case QueryNodeKind.All:
                AllNode allNode = (AllNode)node;
                Validate(allNode.Source, context);
                Validate(allNode.Body, context);
                break;

            case QueryNodeKind.SingleResourceCast:
                Validate(((SingleResourceCastNode)node).Source, context);
                break;

            case QueryNodeKind.CollectionResourceCast:
                Validate(((CollectionResourceCastNode)node).Source, context);
                break;

            case QueryNodeKind.In:
                InNode inNode = (InNode)node;
                Validate(inNode.Left, context);
                Validate(inNode.Right, context);
                break;

            case QueryNodeKind.Count:
                // A real CountNode ($count over a collection) exposes a Source to walk. The virtual
                // $count used inside aggregate($count as ...) reports the same Count kind but has no
                // underlying property to restrict, so it is left as a no-op.
                if (node is CountNode countNode)
                {
                    Validate(countNode.Source, context);
                }
                break;

            default:
                break;
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
