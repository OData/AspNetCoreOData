//-----------------------------------------------------------------------------
// <copyright file="ComputeBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Expressions;


/// <summary>
/// The default implementation to bind an OData $apply parse tree represented by a <see cref="ComputeTransformationNode"/> to an <see cref="Expression"/>.
/// </summary>
public class ComputeBinder : QueryBinder, IComputeBinder
{
    /// <inheritdoc/>
    public virtual Expression BindCompute(ComputeTransformationNode computeTransformationNode, QueryBinderContext context)
    {
        // NOTE: compute(X add Y as Z, A mul B as C) adds new properties to the output

        Type wrapperType = typeof(ComputeWrapper<>).MakeGenericType(context.TransformationElementType);
        // Set Instance property
        PropertyInfo wrapperInstanceProperty = wrapperType.GetProperty(QueryConstants.ComputeWrapperInstanceProperty);
        List<MemberAssignment> wrapperTypeMemberAssignments = new List<MemberAssignment>
            {
                Expression.Bind(wrapperInstanceProperty, context.CurrentParameter)
            };

        List<NamedPropertyExpression> properties = new List<NamedPropertyExpression>();
        foreach (ComputeExpression computeExpression in computeTransformationNode.Expressions)
        {
            properties.Add(
                new NamedPropertyExpression(Expression.Constant(computeExpression.Alias),
                WrapConvert(BindAccessExpression(computeExpression.Expression, context))));
        }

        // Initialize property 'Model' on the wrapper class.
        // source = new Wrapper { Model = parameterized(IEdmModel) }
        // Always parameterize as EntityFramework does not let you inject non primitive constant values (like IEdmModel).
        PropertyInfo wrapperModelProperty = wrapperType.GetProperty(QueryConstants.ComputeWrapperModelProperty);
        Expression wrapperModelPropertyValueExpression = LinqParameterContainer.Parameterize(typeof(IEdmModel), context.Model);
        wrapperTypeMemberAssignments.Add(Expression.Bind(wrapperModelProperty, wrapperModelPropertyValueExpression));

        // Set new compute properties
        PropertyInfo wrapperContainerProperty = wrapperType.GetProperty(QueryConstants.GroupByWrapperContainerProperty);
        wrapperTypeMemberAssignments.Add(Expression.Bind(
            wrapperContainerProperty,
            AggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

        return Expression.Lambda(
            Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments), context.CurrentParameter);
    }
}
