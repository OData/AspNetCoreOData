//-----------------------------------------------------------------------------
// <copyright file="OrderByQueryValidator.cs" company=".NET Foundation">
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
/// Represents a validator used to validate an <see cref="OrderByQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class OrderByQueryValidator : IOrderByQueryValidator
{
    /// <summary>
    /// Validates an <see cref="OrderByQueryOption" />.
    /// </summary>
    /// <param name="orderByOption">The $orderby query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(OrderByQueryOption orderByOption, ODataValidationSettings validationSettings)
    {
        if (orderByOption == null)
        {
            throw Error.ArgumentNull("orderByOption");
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull("validationSettings");
        }

        OrderByValidatorContext validatorContext = new OrderByValidatorContext
        {
            OrderBy = orderByOption,
            Context = orderByOption.Context,
            ValidationSettings = validationSettings,
            Property = orderByOption.Context.TargetProperty,
            StructuredType = orderByOption.Context.TargetStructuredType,
            CurrentDepth = 0
        };

        OrderByClause clause = orderByOption.OrderByClause;
        while (clause != null)
        {
            validatorContext.IncrementNodeCount();

            ValidateOrderBy(clause, validatorContext);

            clause = clause.ThenBy;
        }
    }

    /// <summary>
    /// Validates a <see cref="OrderByClause" />.
    /// </summary>
    /// <param name="orderByClause">The <see cref="OrderByClause" />.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateOrderBy(OrderByClause orderByClause, OrderByValidatorContext validatorContext)
    {
        if (orderByClause != null)
        {
            ValidateSingleValueNode(orderByClause.Expression, validatorContext, skipRangeVariable: false);
        }
    }

    /// <summary>
    /// Override this method if you want to visit each query node.
    /// </summary>
    /// <param name="node">The query node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateQueryNode(QueryNode node, OrderByValidatorContext validatorContext)
    {
        if (node is SingleValueNode singleNode)
        {
            ValidateSingleValueNode(singleNode, validatorContext, skipRangeVariable: true);
        }
        else if (node is CollectionNode collectionNode)
        {
            ValidateCollectionNode(collectionNode, validatorContext);
        }
    }

    /// <summary>
    /// The recursive method that validate most of the SingleValueNode type.
    /// </summary>
    /// <param name="node">The single value node.</param>
    /// <param name="validatorContext">The validator context.</param>
    /// <param name="skipRangeVariable">The boolean value indicating skip the range variable validation. Typically, if it's 'Source' of a query node, we should skip it.</param>
    protected virtual void ValidateSingleValueNode(SingleValueNode node, OrderByValidatorContext validatorContext, bool skipRangeVariable)
    {
        if (node == null)
        {
            return;
        }

        switch (node.Kind)
        {
            case QueryNodeKind.BinaryOperator:
                ValidateBinaryOperatorNode(node as BinaryOperatorNode, validatorContext);
                break;

            case QueryNodeKind.Constant:
                ValidateConstantNode(node as ConstantNode, validatorContext);
                break;

            case QueryNodeKind.Convert:
                ValidateConvertNode(node as ConvertNode, validatorContext);
                break;

            case QueryNodeKind.Count:
                ValidateCountNode(node as CountNode, validatorContext);
                break;

            case QueryNodeKind.ResourceRangeVariableReference:
                if (!skipRangeVariable)
                {
                    ValidateRangeVariable((node as ResourceRangeVariableReferenceNode).RangeVariable, validatorContext);
                }
                break;

            case QueryNodeKind.NonResourceRangeVariableReference:
                if (!skipRangeVariable)
                {
                    ValidateRangeVariable((node as NonResourceRangeVariableReferenceNode).RangeVariable, validatorContext);
                }
                break;

            case QueryNodeKind.SingleValuePropertyAccess:
                ValidateSingleValuePropertyAccessNode(node as SingleValuePropertyAccessNode, validatorContext);
                break;

            case QueryNodeKind.SingleComplexNode:
                ValidateSingleComplexNode(node as SingleComplexNode, validatorContext);
                break;

            case QueryNodeKind.UnaryOperator:
                ValidateUnaryOperatorNode(node as UnaryOperatorNode, validatorContext);
                break;

            case QueryNodeKind.SingleValueFunctionCall:
                ValidateSingleValueFunctionCallNode(node as SingleValueFunctionCallNode, validatorContext);
                break;

            case QueryNodeKind.SingleResourceFunctionCall:
                ValidateSingleResourceFunctionCallNode((SingleResourceFunctionCallNode)node, validatorContext);
                break;

            case QueryNodeKind.SingleNavigationNode:
                ValidateNavigationPropertyNode((SingleNavigationNode)node, validatorContext);
                break;

            case QueryNodeKind.SingleResourceCast:
                ValidateSingleResourceCastNode(node as SingleResourceCastNode, validatorContext);
                break;

            case QueryNodeKind.Any:
                ValidateAnyNode(node as AnyNode, validatorContext);
                break;

            case QueryNodeKind.All:
                ValidateAllNode(node as AllNode, validatorContext);
                break;

            case QueryNodeKind.In:
                ValidateInNode(node as InNode, validatorContext);
                break;

            case QueryNodeKind.SingleValueOpenPropertyAccess:
                ValidateSingleValueOpenPropertyNode(node as SingleValueOpenPropertyAccessNode, validatorContext);
                break;

            case QueryNodeKind.NamedFunctionParameter:
            case QueryNodeKind.ParameterAlias:
            case QueryNodeKind.EntitySet:
            case QueryNodeKind.KeyLookup:
            case QueryNodeKind.SearchTerm:
            default:
                // No default validation logic for others.
                break;
        }
    }

    /// <summary>
    /// Override this method to restrict the dynamic property inside the $orderby query.
    /// </summary>
    /// <param name="openPropertyNode">The in operator node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleValueOpenPropertyNode(SingleValueOpenPropertyAccessNode openPropertyNode, OrderByValidatorContext validatorContext)
    {
        // No default validation logic here.
    }

    /// <summary>
    /// Override this method to restrict the 'in' operator in the $orderby query.
    /// </summary>
    /// <param name="inNode">The in operator node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateInNode(InNode inNode, OrderByValidatorContext validatorContext)
    {
        ValidateQueryNode(inNode?.Left, validatorContext);
        ValidateQueryNode(inNode?.Right, validatorContext);
    }

    /// <summary>
    /// Override this method to restrict the 'constant' inside the $orderby query.
    /// </summary>
    /// <param name="constantNode">The constant node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateConstantNode(ConstantNode constantNode, OrderByValidatorContext validatorContext)
    {
        // No default validation logic here.
    }

    /// <summary>
    /// Override this method to restrict the 'cast' inside the $orderby query.
    /// </summary>
    /// <param name="convertNode">The convert node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateConvertNode(ConvertNode convertNode, OrderByValidatorContext validatorContext)
    {
        // Validate child nodes but not the ConvertNode itself.
        ValidateQueryNode(convertNode?.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to restrict the '$count' inside the $orderBy query.
    /// </summary>
    /// <param name="countNode">The count node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCountNode(CountNode countNode, OrderByValidatorContext validatorContext)
    {
        ValidateQueryNode(countNode?.Source, validatorContext);

        if (countNode?.FilterClause != null)
        {
            ValidateQueryNode(countNode.FilterClause.Expression, validatorContext);
        }

        if (countNode?.SearchClause != null)
        {
            ValidateQueryNode(countNode.SearchClause.Expression, validatorContext);
        }
    }

    /// <summary>
    /// Override this method to restrict the binary operators inside the $orderby query.
    /// That includes all the logical operators except 'not' and all math operators.
    /// </summary>
    /// <param name="binaryOperatorNode">The binary operator node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, OrderByValidatorContext validatorContext)
    {
        // recursion case goes here
        ValidateQueryNode(binaryOperatorNode?.Left, validatorContext);
        ValidateQueryNode(binaryOperatorNode?.Right, validatorContext);
    }

    /// <summary>
    /// Override this method to validate the parameter used in the $orderby query.
    /// </summary>
    /// <param name="rangeVariable">The range variable node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateRangeVariable(RangeVariable rangeVariable, OrderByValidatorContext validatorContext)
    {
        if (rangeVariable != null)
        {
            string variableName = rangeVariable.Name;
            if (!IsAllowed(validatorContext.ValidationSettings, variableName))
            {
                throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, variableName, "AllowedOrderByProperties"));
            }
        }
    }

    /// <summary>
    /// Override this method to validate property accessors.
    /// </summary>
    /// <param name="propertyAccessNode">The single value property access node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleValuePropertyAccessNode(SingleValuePropertyAccessNode propertyAccessNode, OrderByValidatorContext validatorContext)
    {
        if (propertyAccessNode == null || validatorContext == null)
        {
            return;
        }

        IEdmModel model = validatorContext.Model;
        bool enableOrderBy = validatorContext.Context.DefaultQueryConfigurations.EnableOrderBy;
        IEdmProperty property = propertyAccessNode.Property;

        // Check whether the property is sortable.
        if (validatorContext.ValidationSettings.AllowedOrderByProperties.Count == 0)
        {
            bool notSortable = false;
            if (propertyAccessNode.Source != null)
            {
                if (propertyAccessNode.Source.Kind == QueryNodeKind.SingleNavigationNode)
                {
                    SingleNavigationNode singleNavigationNode = propertyAccessNode.Source as SingleNavigationNode;
                    notSortable = EdmHelpers.IsNotSortable(property, singleNavigationNode.NavigationProperty, singleNavigationNode.NavigationProperty.ToEntityType(), model, enableOrderBy);
                }
                else if (propertyAccessNode.Source.Kind == QueryNodeKind.SingleComplexNode)
                {
                    SingleComplexNode singleComplexNode = propertyAccessNode.Source as SingleComplexNode;
                    notSortable = EdmHelpers.IsNotSortable(property, singleComplexNode.Property, property.DeclaringType, model, enableOrderBy);
                }
                else
                {
                    notSortable = EdmHelpers.IsNotSortable(property, validatorContext.Property, validatorContext.StructuredType, model, enableOrderBy);
                }
            }

            if (notSortable)
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy, property.Name));
            }
        }
        else
        {
            ValidatePropertyAllowed(property.Name, validatorContext.ValidationSettings);
        }

        ValidateQueryNode(propertyAccessNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate Function calls, such as 'length', 'year', etc.
    /// </summary>
    /// <param name="node">The single value function call node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleValueFunctionCallNode(SingleValueFunctionCallNode node, OrderByValidatorContext validatorContext)
    {
        if (node == null || validatorContext == null)
        {
            return;
        }

        QueryValidatorHelpers.ValidateFunction(node, validatorContext.ValidationSettings);

        foreach (QueryNode argumentNode in node.Parameters)
        {
            ValidateQueryNode(argumentNode, validatorContext);
        }
    }

    /// <summary>
    /// Override this method to validate single complex property accessors.
    /// </summary>
    /// <param name="singleComplexNode">The single complex node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleComplexNode(SingleComplexNode singleComplexNode, OrderByValidatorContext validatorContext)
    {
        if (singleComplexNode == null || validatorContext == null)
        {
            return;
        }

        // Check whether the property is sortable.
        if (validatorContext.ValidationSettings.AllowedOrderByProperties.Count == 0)
        {
            IEdmProperty property = singleComplexNode.Property;
            if (EdmHelpers.IsNotSortable(property,
                validatorContext.Property,
                validatorContext.StructuredType,
                validatorContext.Model,
                validatorContext.Context.DefaultQueryConfigurations.EnableOrderBy))
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy, property.Name));
            }
        }
        else
        {
            ValidatePropertyAllowed(singleComplexNode.Property.Name, validatorContext.ValidationSettings);
        }

        ValidateQueryNode(singleComplexNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate single resource function calls.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleResourceFunctionCallNode(SingleResourceFunctionCallNode node, OrderByValidatorContext validatorContext)
    {
        if (node == null || validatorContext == null)
        {
            return;
        }

        // Ideally, we should validate the function call name here.
        // But, there's no validation setting for SingleResourceFunctionCall. So, let's skip the name validation
        // Customer can override this method to add name validation.

        foreach (QueryNode argumentNode in node.Parameters)
        {
            ValidateQueryNode(argumentNode, validatorContext);
        }
    }

    /// <summary>
    /// Override this method if you want to validate casts on single resource.
    /// </summary>
    /// <param name="singleResourceCastNode">The single resource cast node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleResourceCastNode(SingleResourceCastNode singleResourceCastNode, OrderByValidatorContext validatorContext)
    {
        ValidateQueryNode(singleResourceCastNode?.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate the Not operator.
    /// </summary>
    /// <param name="unaryOperatorNode">The unary operator node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode, OrderByValidatorContext validatorContext)
    {
        // no default validation here
    }

    /// <summary>
    /// Override this method for the collection value navigation property node.
    /// </summary>
    /// <param name="collectionNavigation">The source node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateNavigationPropertyNode(CollectionNavigationNode collectionNavigation, OrderByValidatorContext validatorContext)
    {
        if (collectionNavigation == null || validatorContext == null)
        {
            return;
        }

        // Check whether the property is not sortable
        if (validatorContext.ValidationSettings.AllowedOrderByProperties.Count == 0)
        {
            if (EdmHelpers.IsNotSortable(collectionNavigation.NavigationProperty,
            validatorContext.Property,
            validatorContext.StructuredType,
            validatorContext.Model,
            validatorContext.Context.DefaultQueryConfigurations.EnableOrderBy))
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy, collectionNavigation.NavigationProperty.Name));
            }
        }
        else
        {
            ValidatePropertyAllowed(collectionNavigation.NavigationProperty.Name, validatorContext.ValidationSettings);
        }

        ValidateQueryNode(collectionNavigation.Source, validatorContext);
    }

    /// <summary>
    /// Override this method for the single value navigation property node.
    /// </summary>
    /// <param name="singleNavigation">The source node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateNavigationPropertyNode(SingleNavigationNode singleNavigation, OrderByValidatorContext validatorContext)
    {
        if (singleNavigation == null || validatorContext == null)
        {
            return;
        }

        // Check whether the property is not sortable
        if (validatorContext.ValidationSettings.AllowedOrderByProperties.Count == 0)
        {
            if (EdmHelpers.IsNotSortable(singleNavigation.NavigationProperty,
            validatorContext.Property,
            validatorContext.StructuredType,
            validatorContext.Model,
            validatorContext.Context.DefaultQueryConfigurations.EnableOrderBy))
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy, singleNavigation.NavigationProperty.Name));
            }
        }
        else
        {
            ValidatePropertyAllowed(singleNavigation.NavigationProperty.Name, validatorContext.ValidationSettings);
        }

        ValidateQueryNode(singleNavigation.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to restrict the 'all' query inside the $orderby query.
    /// </summary>
    /// <param name="allNode">The all node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateAllNode(AllNode allNode, OrderByValidatorContext validatorContext)
    {
        // no default validation here
    }

    /// <summary>
    /// Override this method to restrict the 'any' query inside the $orderby query.
    /// </summary>
    /// <param name="anyNode">The any node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateAnyNode(AnyNode anyNode, OrderByValidatorContext validatorContext)
    {
        // no default validation here
    }

    /// <summary>
    /// The recursive method that validate most of the query node type is of CollectionNode type.
    /// </summary>
    /// <param name="node">The collection value node.</param>
    /// <param name="validatorContext">The validator context.</param>
    protected virtual void ValidateCollectionNode(CollectionNode node, OrderByValidatorContext validatorContext)
    {
        if (node == null)
        {
            return;
        }

        switch (node.Kind)
        {
            case QueryNodeKind.CollectionPropertyAccess:
                ValidateCollectionPropertyAccessNode((CollectionPropertyAccessNode)node, validatorContext);
                break;

            case QueryNodeKind.CollectionComplexNode:
                ValidateCollectionComplexNode((CollectionComplexNode)node, validatorContext);
                break;

            case QueryNodeKind.CollectionNavigationNode:
                ValidateNavigationPropertyNode((CollectionNavigationNode)node, validatorContext);
                break;

            case QueryNodeKind.CollectionResourceCast:
                ValidateCollectionResourceCastNode((CollectionResourceCastNode)node, validatorContext);
                break;

            case QueryNodeKind.CollectionFunctionCall:
            case QueryNodeKind.CollectionResourceFunctionCall:
            case QueryNodeKind.CollectionOpenPropertyAccess:
            default:
                // by default no validation for them.
                break;
        }
    }

    /// <summary>
    /// Override this method if you want to validate on resource collections cast node.
    /// </summary>
    /// <param name="collectionResourceCastNode">The collection resource cast node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCollectionResourceCastNode(CollectionResourceCastNode collectionResourceCastNode, OrderByValidatorContext validatorContext)
    {
        ValidateQueryNode(collectionResourceCastNode?.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate collection property accessors.
    /// </summary>
    /// <param name="propertyAccessNode">The collection property access node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode, OrderByValidatorContext validatorContext)
    {
        if (propertyAccessNode == null || validatorContext == null)
        {
            return;
        }

        // Check whether the property is sortable.
        if (validatorContext.ValidationSettings.AllowedOrderByProperties.Count == 0)
        {
            IEdmProperty property = propertyAccessNode.Property;
            if (EdmHelpers.IsNotSortable(property,
                validatorContext.Property,
                validatorContext.StructuredType,
                validatorContext.Model,
                validatorContext.Context.DefaultQueryConfigurations.EnableOrderBy))
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy, property.Name));
            }
        }
        else
        {
            ValidatePropertyAllowed(propertyAccessNode.Property.Name, validatorContext.ValidationSettings);
        }

        ValidateQueryNode(propertyAccessNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate collection complex property accessors.
    /// </summary>
    /// <param name="collectionComplexNode">The collection complex node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCollectionComplexNode(CollectionComplexNode collectionComplexNode, OrderByValidatorContext validatorContext)
    {
        if (collectionComplexNode == null || validatorContext == null)
        {
            return;
        }

        // Check whether the property is sortable.
        if (validatorContext.ValidationSettings.AllowedOrderByProperties.Count == 0)
        {
            IEdmProperty property = collectionComplexNode.Property;
            if (EdmHelpers.IsNotSortable(property, validatorContext.Property,
                validatorContext.StructuredType,
                validatorContext.Model,
                validatorContext.Context.DefaultQueryConfigurations.EnableOrderBy))
            {
                throw new ODataException(Error.Format(SRResources.NotSortablePropertyUsedInOrderBy, property.Name));
            }
        }
        else
        {
            ValidatePropertyAllowed(collectionComplexNode.Property.Name, validatorContext.ValidationSettings);
        }

        ValidateQueryNode(collectionComplexNode.Source, validatorContext);
    }

    private static void ValidatePropertyAllowed(string propertyName, ODataValidationSettings validationSettings)
    {
        // An empty collection means client can order the queryable result by any properties
        if (validationSettings.AllowedOrderByProperties.Count == 0)
        {
            return;
        }

        // Then if the property name is not set in the collection, it means client can't order by it.
        if (!validationSettings.AllowedOrderByProperties.Contains(propertyName))
        {
            throw new ODataException(Error.Format(SRResources.NotAllowedOrderByProperty, propertyName, "AllowedOrderByProperties"));
        }
    }

    private static bool IsAllowed(ODataValidationSettings validationSettings, string propertyName)
    {
        return validationSettings.AllowedOrderByProperties.Count == 0 ||
               validationSettings.AllowedOrderByProperties.Contains(propertyName);
    }
}
