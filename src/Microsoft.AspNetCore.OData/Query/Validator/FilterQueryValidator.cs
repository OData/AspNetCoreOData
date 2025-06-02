//-----------------------------------------------------------------------------
// <copyright file="FilterQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate a <see cref="FilterQueryOption" /> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
public class FilterQueryValidator : IFilterQueryValidator
{
    /// <summary>
    /// Validates a <see cref="FilterQueryOption" />.
    /// </summary>
    /// <param name="filterQueryOption">The $filter query.</param>
    /// <param name="settings">The validation settings.</param>
    public virtual void Validate(FilterQueryOption filterQueryOption, ODataValidationSettings settings)
    {
        if (filterQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(filterQueryOption));
        }

        if (settings == null)
        {
            throw Error.ArgumentNull(nameof(settings));
        }

        FilterValidatorContext validatorContext = new FilterValidatorContext
        {
            Filter = filterQueryOption,
            Context = filterQueryOption.Context,
            ValidationSettings = settings,
            Property = filterQueryOption.Context.TargetProperty,
            StructuredType = filterQueryOption.Context.TargetStructuredType,
            CurrentDepth = 0
        };

        ValidateFilter(filterQueryOption.FilterClause, validatorContext);
    }

    /// <summary>
    /// Attempts to validate the <see cref="FilterQueryOption" />.
    /// </summary>
    /// <param name="filterQueryOption">The $filter query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    /// <param name="validationErrors">Contains a collection of <see cref="string"/> describing any validation errors encountered, or an empty collection if validation succeeds.</param>
    /// <returns><see langword="true"/> if the validation succeeded; otherwise, <see langword="false"/>.</returns>
    public virtual bool TryValidate(FilterQueryOption filterQueryOption, ODataValidationSettings validationSettings, out IEnumerable<string> validationErrors)
    {
        if(filterQueryOption == null || validationSettings == null)
        {
            // Preallocate with a reasonable default capacity.
            List<string> errors = new List<string>(2);

            // Validate input parameters
            if (filterQueryOption == null)
            {
                errors.Add(Error.ArgumentNull(nameof(filterQueryOption)).Message);
            }

            if (validationSettings == null)
            {
                errors.Add(Error.ArgumentNull(nameof(validationSettings)).Message);
            }

            validationErrors = errors;
            return false;
        }

        // Validate the filter clause
        try
        {
            // Create a validation context
            var validatorContext = new FilterValidatorContext
            {
                Filter = filterQueryOption,
                Context = filterQueryOption.Context,
                ValidationSettings = validationSettings,
                Property = filterQueryOption.Context.TargetProperty,
                StructuredType = filterQueryOption.Context.TargetStructuredType,
                CurrentDepth = 0
            };

            ValidateFilter(filterQueryOption.FilterClause, validatorContext);
        }
        catch (Exception ex)
        {
            validationErrors = new[] { ex.Message };
            return false;
        }

        // Set the output parameter
        validationErrors = Array.Empty<string>();
        return true;
    }

    /// <summary>
    /// Validates a <see cref="FilterClause" />.
    /// </summary>
    /// <param name="filterClause">The <see cref="FilterClause" />.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateFilter(FilterClause filterClause, FilterValidatorContext validatorContext)
    {
        Contract.Assert(filterClause != null);

        ValidateQueryNode(filterClause.Expression, validatorContext);
    }

    /// <summary>
    /// Override this method to restrict the 'all' query inside the filter query.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="allNode">The all node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateAllNode(AllNode allNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(allNode != null);

        ValidateFunction("all", validatorContext);
        validatorContext.EnterLambda();

        try
        {
            ValidateQueryNode(allNode.Source, validatorContext);

            ValidateQueryNode(allNode.Body, validatorContext);
        }
        finally
        {
            validatorContext.ExitLambda();
        }
    }

    /// <summary>
    /// Override this method to restrict the 'any' query inside the filter query.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="anyNode">The any node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateAnyNode(AnyNode anyNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(anyNode != null);

        ValidateFunction("any", validatorContext);
        validatorContext.EnterLambda();

        try
        {
            ValidateQueryNode(anyNode.Source, validatorContext);

            if (anyNode.Body != null && anyNode.Body.Kind != QueryNodeKind.Constant)
            {
                ValidateQueryNode(anyNode.Body, validatorContext);
            }
        }
        finally
        {
            validatorContext.ExitLambda();
        }
    }

    /// <summary>
    /// override this method to restrict the binary operators inside the filter query. That includes all the logical operators except 'not' and all math operators.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="binaryOperatorNode">The binary operator node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(binaryOperatorNode != null);

        // base case goes
        switch (binaryOperatorNode.OperatorKind)
        {
            case BinaryOperatorKind.Equal:
            case BinaryOperatorKind.NotEqual:
            case BinaryOperatorKind.And:
            case BinaryOperatorKind.GreaterThan:
            case BinaryOperatorKind.GreaterThanOrEqual:
            case BinaryOperatorKind.LessThan:
            case BinaryOperatorKind.LessThanOrEqual:
            case BinaryOperatorKind.Or:
            case BinaryOperatorKind.Has:
                // binary logical operators
                ValidateLogicalOperator(binaryOperatorNode, validatorContext);
                break;
            default:
                // math operators
                ValidateArithmeticOperator(binaryOperatorNode, validatorContext);
                break;
        }
    }

    /// <summary>
    /// Override this method to validate the LogicalOperators such as 'eq', 'ne', 'gt', 'ge', 'lt', 'le', 'and', 'or'.
    /// 
    /// Please note that 'not' is not included here. Please override ValidateUnaryOperatorNode to customize 'not'.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="binaryNode">The binary operator node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateLogicalOperator(BinaryOperatorNode binaryNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(binaryNode != null);
        Contract.Assert(validatorContext != null);

        AllowedLogicalOperators logicalOperator = ToLogicalOperator(binaryNode);

        if ((validatorContext.ValidationSettings.AllowedLogicalOperators & logicalOperator) != logicalOperator)
        {
            // this means the given logical operator is not allowed
            throw new ODataException(Error.Format(SRResources.NotAllowedLogicalOperator, logicalOperator, "AllowedLogicalOperators"));
        }

        // recursion case goes here
        ValidateQueryNode(binaryNode.Left, validatorContext);
        ValidateQueryNode(binaryNode.Right, validatorContext);
    }

    /// <summary>
    /// Override this method for the Arithmetic operators, including add, sub, mul, div, mod.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="binaryNode">The binary operator node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateArithmeticOperator(BinaryOperatorNode binaryNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(binaryNode != null);
        Contract.Assert(validatorContext != null);

        AllowedArithmeticOperators arithmeticOperator = ToArithmeticOperator(binaryNode);

        if ((validatorContext.ValidationSettings.AllowedArithmeticOperators & arithmeticOperator) != arithmeticOperator)
        {
            // this means the given logical operator is not allowed
            throw new ODataException(Error.Format(SRResources.NotAllowedArithmeticOperator, arithmeticOperator, "AllowedArithmeticOperators"));
        }

        // recursion case goes here
        ValidateQueryNode(binaryNode.Left, validatorContext);
        ValidateQueryNode(binaryNode.Right, validatorContext);
    }

    /// <summary>
    /// Override this method to restrict the 'constant' inside the filter query.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="constantNode">The constant node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateConstantNode(ConstantNode constantNode, FilterValidatorContext validatorContext)
    {
        // No default validation logic here.
    }

    /// <summary>
    /// Override this method to restrict the 'cast' inside the filter query.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="convertNode">The convert node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateConvertNode(ConvertNode convertNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(convertNode != null);

        // Validate child nodes but not the ConvertNode itself.
        ValidateQueryNode(convertNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to restrict the '$count' inside the filter query.
    /// </summary>
    /// <param name="countNode">The count node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCountNode(CountNode countNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(countNode != null);
        Contract.Assert(validatorContext != null);

        ValidateQueryNode(countNode.Source, validatorContext);

        if (countNode.FilterClause != null)
        {
            ValidateQueryNode(countNode.FilterClause.Expression, validatorContext);
        }

        if (countNode.SearchClause != null)
        {
            ValidateQueryNode(countNode.SearchClause.Expression, validatorContext);
        }
    }

    /// <summary>
    /// Override this method for the navigation property node.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="sourceNode">The source node to validate.</param>
    /// <param name="navigationProperty">The navigation property.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateNavigationPropertyNode(QueryNode sourceNode, IEdmNavigationProperty navigationProperty, FilterValidatorContext validatorContext)
    {
        Contract.Assert(navigationProperty != null);

        // Check whether the property is not filterable
        if (EdmHelpers.IsNotFilterable(navigationProperty,
            validatorContext.Property,
            validatorContext.StructuredType,
            validatorContext.Model,
            validatorContext.Context.DefaultQueryConfigurations.EnableFilter))
        {
            throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter,
                navigationProperty.Name));
        }

        // recursion
        if (sourceNode != null)
        {
            ValidateQueryNode(sourceNode, validatorContext);
        }
    }

    /// <summary>
    /// Override this method to validate the parameter used in the filter query.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="rangeVariable">The range variable node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateRangeVariable(RangeVariable rangeVariable, FilterValidatorContext validatorContext)
    {
        // No default validation logic here.
    }

    /// <summary>
    /// Override this method to validate property accessors.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="propertyAccessNode">The single value property access node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleValuePropertyAccessNode(SingleValuePropertyAccessNode propertyAccessNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(propertyAccessNode != null);
        Contract.Assert(validatorContext != null);

        IEdmModel model = validatorContext.Model;
        var defaultQueryConfigs = validatorContext.Context.DefaultQueryConfigurations;

        // Check whether the property is filterable.
        IEdmProperty property = propertyAccessNode.Property;
        bool notFilterable = false;
        if (propertyAccessNode.Source != null)
        {
            if (propertyAccessNode.Source.Kind == QueryNodeKind.SingleNavigationNode)
            {
                SingleNavigationNode singleNavigationNode = propertyAccessNode.Source as SingleNavigationNode;
                notFilterable = EdmHelpers.IsNotFilterable(property,
                    singleNavigationNode.NavigationProperty,
                    singleNavigationNode.NavigationProperty.ToEntityType(),
                    model,
                    defaultQueryConfigs.EnableFilter);
            }
            else if (propertyAccessNode.Source.Kind == QueryNodeKind.SingleComplexNode)
            {
                SingleComplexNode singleComplexNode = propertyAccessNode.Source as SingleComplexNode;
                notFilterable = EdmHelpers.IsNotFilterable(property,
                    singleComplexNode.Property,
                    property.DeclaringType,
                    model,
                    defaultQueryConfigs.EnableFilter);
            }
            else if (propertyAccessNode.Source.Kind == QueryNodeKind.ResourceRangeVariableReference)
            {
                ResourceRangeVariableReferenceNode resourceRangeVariableReferenceNode = propertyAccessNode.Source as ResourceRangeVariableReferenceNode;
                notFilterable = EdmHelpers.IsNotFilterable(
                    property,
                    validatorContext.Property,
                    resourceRangeVariableReferenceNode.RangeVariable.StructuredTypeReference.StructuredDefinition(),
                    model,
                    defaultQueryConfigs.EnableFilter);
            }
            else
            {
                notFilterable = EdmHelpers.IsNotFilterable(property,
                    validatorContext.Property,
                    validatorContext.StructuredType,
                    model,
                    defaultQueryConfigs.EnableFilter);
            }
        }

        if (notFilterable)
        {
            throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter, property.Name));
        }

        ValidateQueryNode(propertyAccessNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate single complex property accessors.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="singleComplexNode">The single complex node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleComplexNode(SingleComplexNode singleComplexNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(singleComplexNode != null);
        Contract.Assert(validatorContext != null);

        // Check whether the property is filterable.
        IEdmProperty property = singleComplexNode.Property;
        if (EdmHelpers.IsNotFilterable(property,
            validatorContext.Property,
            validatorContext.StructuredType,
            validatorContext.Model,
            validatorContext.Context.DefaultQueryConfigurations.EnableFilter))
        {
            throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter, property.Name));
        }

        ValidateQueryNode(singleComplexNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate collection property accessors.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="propertyAccessNode">The collection property access node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCollectionPropertyAccessNode(CollectionPropertyAccessNode propertyAccessNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(propertyAccessNode != null);
        Contract.Assert(validatorContext != null);

        // Check whether the property is filterable.
        IEdmProperty property = propertyAccessNode.Property;
        if (EdmHelpers.IsNotFilterable(property,
            validatorContext.Property,
            validatorContext.StructuredType,
            validatorContext.Model,
            validatorContext.Context.DefaultQueryConfigurations.EnableFilter))
        {
            throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter, property.Name));
        }

        ValidateQueryNode(propertyAccessNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate collection complex property accessors.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="collectionComplexNode">The collection complex node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCollectionComplexNode(CollectionComplexNode collectionComplexNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(collectionComplexNode != null);
        Contract.Assert(validatorContext != null);

        // Check whether the property is filterable.
        IEdmProperty property = collectionComplexNode.Property;
        if (EdmHelpers.IsNotFilterable(property, validatorContext.Property,
            validatorContext.StructuredType,
            validatorContext.Model,
            validatorContext.Context.DefaultQueryConfigurations.EnableFilter))
        {
            throw new ODataException(Error.Format(SRResources.NotFilterablePropertyUsedInFilter, property.Name));
        }

        ValidateQueryNode(collectionComplexNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method to validate Function calls, such as 'length', 'year', etc.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="node">The single value function call node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleValueFunctionCallNode(SingleValueFunctionCallNode node, FilterValidatorContext validatorContext)
    {
        Contract.Assert(node != null);
        Contract.Assert(validatorContext != null);

        ValidateFunction(node.Name, validatorContext);

        foreach (QueryNode argumentNode in node.Parameters)
        {
            ValidateQueryNode(argumentNode, validatorContext);
        }
    }

    /// <summary>
    /// Override this method to validate single resource function calls, such as 'cast'.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <param name="validatorContext">The validation context.</param>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit
    /// testing scenarios and is not intended to be called from user code. Call the Validate method to validate a
    /// <see cref="FilterQueryOption" /> instance.
    /// </remarks>
    protected virtual void ValidateSingleResourceFunctionCallNode(SingleResourceFunctionCallNode node, FilterValidatorContext validatorContext)
    {
        Contract.Assert(node != null);
        Contract.Assert(validatorContext != null);

        ValidateFunction(node.Name, validatorContext);

        foreach (QueryNode argumentNode in node.Parameters)
        {
            ValidateQueryNode(argumentNode, validatorContext);
        }
    }

    /// <summary>
    /// Override this method to validate the Not operator.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="unaryOperatorNode">The unary operator node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateUnaryOperatorNode(UnaryOperatorNode unaryOperatorNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(unaryOperatorNode != null);
        Contract.Assert(validatorContext != null);

        ValidateQueryNode(unaryOperatorNode.Operand, validatorContext);

        switch (unaryOperatorNode.OperatorKind)
        {
            case UnaryOperatorKind.Negate:
            case UnaryOperatorKind.Not:
                if ((validatorContext.ValidationSettings.AllowedLogicalOperators & AllowedLogicalOperators.Not) != AllowedLogicalOperators.Not)
                {
                    throw new ODataException(Error.Format(SRResources.NotAllowedLogicalOperator, unaryOperatorNode.OperatorKind, "AllowedLogicalOperators"));
                }
                break;

            default:
                throw Error.NotSupported(SRResources.UnaryNodeValidationNotSupported, unaryOperatorNode.OperatorKind, typeof(FilterQueryValidator).Name);
        }
    }

    /// <summary>
    /// Override this method if you want to visit each query node.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="node">The query node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateQueryNode(QueryNode node, FilterValidatorContext validatorContext)
    {
        Contract.Assert(validatorContext != null);

        // Recursion guard to avoid stack overflows
        RuntimeHelpers.EnsureSufficientExecutionStack();

        SingleValueNode singleNode = node as SingleValueNode;
        CollectionNode collectionNode = node as CollectionNode;

        validatorContext.IncrementNodeCount();

        if (singleNode != null)
        {
            ValidateSingleValueNode(singleNode, validatorContext);
        }
        else if (collectionNode != null)
        {
            ValidateCollectionNode(collectionNode, validatorContext);
        }
    }

    /// <summary>
    /// Override this method if you want to validate casts on resource collections.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="collectionResourceCastNode">The collection resource cast node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateCollectionResourceCastNode(CollectionResourceCastNode collectionResourceCastNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(collectionResourceCastNode != null);
        Contract.Assert(validatorContext != null);

        ValidateQueryNode(collectionResourceCastNode.Source, validatorContext);
    }

    /// <summary>
    /// Override this method if you want to validate casts on single resource.
    /// </summary>
    /// <remarks>
    /// This method is intended to be called from method overrides in subclasses. This method also supports unit-testing scenarios and is not intended to be called from user code.
    /// Call the Validate method to validate a <see cref="FilterQueryOption"/> instance.
    /// </remarks>
    /// <param name="singleResourceCastNode">The single resource cast node.</param>
    /// <param name="validatorContext">The validation context.</param>
    protected virtual void ValidateSingleResourceCastNode(SingleResourceCastNode singleResourceCastNode, FilterValidatorContext validatorContext)
    {
        Contract.Assert(singleResourceCastNode != null);
        Contract.Assert(validatorContext != null);

        ValidateQueryNode(singleResourceCastNode.Source, validatorContext);
    }

    /// <summary>
    /// The recursive method that validate most of the query node type is of CollectionNode type.
    /// </summary>
    /// <param name="node">The single value node.</param>
    /// <param name="validatorContext">The validator context.</param>
    protected virtual void ValidateCollectionNode(CollectionNode node, FilterValidatorContext validatorContext)
    {
        switch (node.Kind)
        {
            case QueryNodeKind.CollectionPropertyAccess:
                CollectionPropertyAccessNode propertyAccessNode = node as CollectionPropertyAccessNode;
                ValidateCollectionPropertyAccessNode(propertyAccessNode, validatorContext);
                break;

            case QueryNodeKind.CollectionOpenPropertyAccess:
                // No identified validations for collection-valued open property yet
                break;

            case QueryNodeKind.CollectionComplexNode:
                CollectionComplexNode collectionComplexNode = node as CollectionComplexNode;
                ValidateCollectionComplexNode(collectionComplexNode, validatorContext);
                break;

            case QueryNodeKind.CollectionNavigationNode:
                CollectionNavigationNode navigationNode = node as CollectionNavigationNode;
                ValidateNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, validatorContext);
                break;

            case QueryNodeKind.CollectionResourceCast:
                ValidateCollectionResourceCastNode(node as CollectionResourceCastNode, validatorContext);
                break;

            case QueryNodeKind.CollectionFunctionCall:
            case QueryNodeKind.CollectionResourceFunctionCall:
            // Unused or have unknown uses.
            default:
                throw Error.NotSupported(SRResources.QueryNodeValidationNotSupported, node.Kind, typeof(FilterQueryValidator).Name);
        }
    }

    /// <summary>
    /// The recursive method that validate most of the query node type is of SingleValueNode type.
    /// </summary>
    /// <param name="node">The single value node.</param>
    /// <param name="validatorContext">The validator context.</param>
    protected virtual void ValidateSingleValueNode(SingleValueNode node, FilterValidatorContext validatorContext)
    {
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
                ValidateRangeVariable((node as ResourceRangeVariableReferenceNode).RangeVariable, validatorContext);
                break;

            case QueryNodeKind.NonResourceRangeVariableReference:
                ValidateRangeVariable((node as NonResourceRangeVariableReferenceNode).RangeVariable, validatorContext);
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
                SingleNavigationNode navigationNode = node as SingleNavigationNode;
                ValidateNavigationPropertyNode(navigationNode.Source, navigationNode.NavigationProperty, validatorContext);
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

            case QueryNodeKind.SingleValueOpenPropertyAccess:
                //no validation on open values?
                break;

            case QueryNodeKind.In:
                // No setting validations
                break;

            case QueryNodeKind.NamedFunctionParameter:
            case QueryNodeKind.ParameterAlias:
            case QueryNodeKind.EntitySet:
            case QueryNodeKind.KeyLookup:
            case QueryNodeKind.SearchTerm:
            // Unused or have unknown uses.
            default:
                throw Error.NotSupported(SRResources.QueryNodeValidationNotSupported, node.Kind, typeof(FilterQueryValidator).Name);
        }
    }

    private static void ValidateFunction(string functionName, FilterValidatorContext validatorContext)
    {
        Contract.Assert(validatorContext != null);

        AllowedFunctions convertedFunction = ToODataFunction(functionName);
        if ((validatorContext.ValidationSettings.AllowedFunctions & convertedFunction) != convertedFunction)
        {
            // this means the given function is not allowed
            throw new ODataException(Error.Format(SRResources.NotAllowedFunction, functionName, "AllowedFunctions"));
        }
    }

    private static AllowedFunctions ToODataFunction(string functionName)
    {
        AllowedFunctions result = AllowedFunctions.None;

        switch (functionName)
        {
            case "any":
                result = AllowedFunctions.Any;
                break;
            case "all":
                result = AllowedFunctions.All;
                break;
            case "cast":
                result = AllowedFunctions.Cast;
                break;
            case ClrCanonicalFunctions.CeilingFunctionName:
                result = AllowedFunctions.Ceiling;
                break;
            case ClrCanonicalFunctions.ConcatFunctionName:
                result = AllowedFunctions.Concat;
                break;
            case ClrCanonicalFunctions.ContainsFunctionName:
                result = AllowedFunctions.Contains;
                break;
            case ClrCanonicalFunctions.DayFunctionName:
                result = AllowedFunctions.Day;
                break;
            case ClrCanonicalFunctions.EndswithFunctionName:
                result = AllowedFunctions.EndsWith;
                break;
            case ClrCanonicalFunctions.FloorFunctionName:
                result = AllowedFunctions.Floor;
                break;
            case ClrCanonicalFunctions.HourFunctionName:
                result = AllowedFunctions.Hour;
                break;
            case ClrCanonicalFunctions.IndexofFunctionName:
                result = AllowedFunctions.IndexOf;
                break;
            case "isof":
                result = AllowedFunctions.IsOf;
                break;
            case ClrCanonicalFunctions.LengthFunctionName:
                result = AllowedFunctions.Length;
                break;
            case ClrCanonicalFunctions.MatchesPatternFunctionName:
                result = AllowedFunctions.MatchesPattern;
                break;
            case ClrCanonicalFunctions.MinuteFunctionName:
                result = AllowedFunctions.Minute;
                break;
            case ClrCanonicalFunctions.MonthFunctionName:
                result = AllowedFunctions.Month;
                break;
            case ClrCanonicalFunctions.RoundFunctionName:
                result = AllowedFunctions.Round;
                break;
            case ClrCanonicalFunctions.SecondFunctionName:
                result = AllowedFunctions.Second;
                break;
            case ClrCanonicalFunctions.StartswithFunctionName:
                result = AllowedFunctions.StartsWith;
                break;
            case ClrCanonicalFunctions.SubstringFunctionName:
                result = AllowedFunctions.Substring;
                break;
            case ClrCanonicalFunctions.TolowerFunctionName:
                result = AllowedFunctions.ToLower;
                break;
            case ClrCanonicalFunctions.ToupperFunctionName:
                result = AllowedFunctions.ToUpper;
                break;
            case ClrCanonicalFunctions.TrimFunctionName:
                result = AllowedFunctions.Trim;
                break;
            case ClrCanonicalFunctions.YearFunctionName:
                result = AllowedFunctions.Year;
                break;
            case ClrCanonicalFunctions.DateFunctionName:
                result = AllowedFunctions.Date;
                break;
            case ClrCanonicalFunctions.TimeFunctionName:
                result = AllowedFunctions.Time;
                break;
            case ClrCanonicalFunctions.FractionalSecondsFunctionName:
                result = AllowedFunctions.FractionalSeconds;
                break;
            default:
                // should never be here
                Contract.Assert(true, "ToODataFunction should never be here.");
                break;
        }

        return result;
    }

    private static AllowedLogicalOperators ToLogicalOperator(BinaryOperatorNode binaryNode)
    {
        AllowedLogicalOperators result = AllowedLogicalOperators.None;

        switch (binaryNode.OperatorKind)
        {
            case BinaryOperatorKind.Equal:
                result = AllowedLogicalOperators.Equal;
                break;

            case BinaryOperatorKind.NotEqual:
                result = AllowedLogicalOperators.NotEqual;
                break;

            case BinaryOperatorKind.And:
                result = AllowedLogicalOperators.And;
                break;

            case BinaryOperatorKind.GreaterThan:
                result = AllowedLogicalOperators.GreaterThan;
                break;

            case BinaryOperatorKind.GreaterThanOrEqual:
                result = AllowedLogicalOperators.GreaterThanOrEqual;
                break;

            case BinaryOperatorKind.LessThan:
                result = AllowedLogicalOperators.LessThan;
                break;

            case BinaryOperatorKind.LessThanOrEqual:
                result = AllowedLogicalOperators.LessThanOrEqual;
                break;

            case BinaryOperatorKind.Or:
                result = AllowedLogicalOperators.Or;
                break;

            case BinaryOperatorKind.Has:
                result = AllowedLogicalOperators.Has;
                break;

            default:
                // should never be here
                Contract.Assert(false, "ToLogicalOperator should never be here.");
                break;
        }

        return result;
    }

    private static AllowedArithmeticOperators ToArithmeticOperator(BinaryOperatorNode binaryNode)
    {
        AllowedArithmeticOperators result = AllowedArithmeticOperators.None;

        switch (binaryNode.OperatorKind)
        {
            case BinaryOperatorKind.Add:
                result = AllowedArithmeticOperators.Add;
                break;

            case BinaryOperatorKind.Divide:
                result = AllowedArithmeticOperators.Divide;
                break;

            case BinaryOperatorKind.Modulo:
                result = AllowedArithmeticOperators.Modulo;
                break;

            case BinaryOperatorKind.Multiply:
                result = AllowedArithmeticOperators.Multiply;
                break;

            case BinaryOperatorKind.Subtract:
                result = AllowedArithmeticOperators.Subtract;
                break;

            default:
                // should never be here
                Contract.Assert(false, "ToArithmeticOperator should never be here.");
                break;
        }

        return result;
    }
}
