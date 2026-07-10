//-----------------------------------------------------------------------------
// <copyright file="ApplyQueryValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Validator;

/// <summary>
/// Represents a validator used to validate an <see cref="ApplyQueryOption"/> based on the <see cref="ODataValidationSettings"/>.
/// </summary>
/// <remarks>
/// The <c>filter</c> transformation is validated with the same <see cref="IFilterQueryValidator"/> used by
/// <c>$filter</c>, so a property marked as not filterable is rejected consistently. The <c>groupby</c>,
/// <c>aggregate</c> and <c>compute</c> transformations reject any referenced property that the model marks as
/// not filterable or configures as not selectable, consistent with <c>$filter</c> and <c>$select</c>.
/// </remarks>
public class ApplyQueryValidator : IApplyQueryValidator
{
    /// <summary>
    /// Validates an <see cref="ApplyQueryOption" />.
    /// </summary>
    /// <param name="applyQueryOption">The $apply query.</param>
    /// <param name="validationSettings">The validation settings.</param>
    public virtual void Validate(ApplyQueryOption applyQueryOption, ODataValidationSettings validationSettings)
    {
        if (applyQueryOption == null)
        {
            throw Error.ArgumentNull(nameof(applyQueryOption));
        }

        if (validationSettings == null)
        {
            throw Error.ArgumentNull(nameof(validationSettings));
        }

        ApplyClause applyClause = applyQueryOption.ApplyClause;
        if (applyClause == null)
        {
            return;
        }

        foreach (TransformationNode transformation in applyClause.Transformations)
        {
            ValidateTransformation(transformation, applyQueryOption.Context, validationSettings);
        }
    }

    private static void ValidateTransformation(TransformationNode transformation, ODataQueryContext context, ODataValidationSettings validationSettings)
    {
        if (transformation == null)
        {
            return;
        }

        switch (transformation.Kind)
        {
            case TransformationNodeKind.Filter:
                FilterTransformationNode filterTransformation = (FilterTransformationNode)transformation;

                // Validate the filter transformation with the same validator used by $filter so that
                // not-filterable properties and the configured limits are enforced consistently.
                FilterQueryOption filterQueryOption = new FilterQueryOption(context, filterTransformation.FilterClause);
                filterQueryOption.Validate(validationSettings);
                break;

            case TransformationNodeKind.GroupBy:
                GroupByTransformationNode groupByTransformation = (GroupByTransformationNode)transformation;
                if (groupByTransformation.GroupingProperties != null)
                {
                    foreach (GroupByPropertyNode groupingProperty in groupByTransformation.GroupingProperties)
                    {
                        ValidateGroupByPropertyNode(groupingProperty, context);
                    }
                }

                ValidateTransformation(groupByTransformation.ChildTransformations, context, validationSettings);
                break;

            case TransformationNodeKind.Aggregate:
                AggregateTransformationNode aggregateTransformation = (AggregateTransformationNode)transformation;
                if (aggregateTransformation.AggregateExpressions != null)
                {
                    foreach (AggregateExpressionBase aggregateExpression in aggregateTransformation.AggregateExpressions)
                    {
                        ValidateAggregateExpression(aggregateExpression, context);
                    }
                }
                break;

            case TransformationNodeKind.Compute:
                ComputeTransformationNode computeTransformation = (ComputeTransformationNode)transformation;
                if (computeTransformation.Expressions != null)
                {
                    foreach (ComputeExpression computeExpression in computeTransformation.Expressions)
                    {
                        QueryNodeRestrictionValidator.Validate(computeExpression.Expression, context);
                    }
                }
                break;

            default:
                // Only the aggregate, groupby, compute and filter transformations are bound and
                // executed by ApplyQueryOptions.ApplyTo, so those are the only kinds that can
                // contribute a referenced property to enforce. Any other transformation kind is
                // intentionally not walked here.
                break;
        }
    }

    private static void ValidateGroupByPropertyNode(GroupByPropertyNode groupingProperty, ODataQueryContext context)
    {
        if (groupingProperty == null)
        {
            return;
        }

        if (groupingProperty.Expression != null)
        {
            QueryNodeRestrictionValidator.Validate(groupingProperty.Expression, context);
        }

        if (groupingProperty.ChildTransformations != null)
        {
            foreach (GroupByPropertyNode childProperty in groupingProperty.ChildTransformations)
            {
                ValidateGroupByPropertyNode(childProperty, context);
            }
        }
    }

    private static void ValidateAggregateExpression(AggregateExpressionBase aggregateExpression, ODataQueryContext context)
    {
        if (aggregateExpression == null)
        {
            return;
        }

        if (aggregateExpression is AggregateExpression singleAggregateExpression)
        {
            QueryNodeRestrictionValidator.Validate(singleAggregateExpression.Expression, context);
        }
        else if (aggregateExpression is EntitySetAggregateExpression entitySetAggregateExpression)
        {
            QueryNodeRestrictionValidator.Validate(entitySetAggregateExpression.Expression, context);

            if (entitySetAggregateExpression.Children != null)
            {
                foreach (AggregateExpressionBase child in entitySetAggregateExpression.Children)
                {
                    ValidateAggregateExpression(child, context);
                }
            }
        }
    }
}
