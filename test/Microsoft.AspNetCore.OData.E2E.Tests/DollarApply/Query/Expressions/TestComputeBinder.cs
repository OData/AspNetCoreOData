//-----------------------------------------------------------------------------
// <copyright file="TestComputeBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Container;
using Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Wrapper;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Expressions
{
    internal class TestComputeBinder : QueryBinder, IComputeBinder
    {
        public Expression BindCompute(ComputeTransformationNode computeTransformationNode, QueryBinderContext context)
        {
            Type wrapperType = typeof(TestComputeWrapper<>).MakeGenericType(context.TransformationElementType);
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
                TestAggregationPropertyContainer.CreateNextNamedPropertyContainer(properties)));

            return Expression.Lambda(
                Expression.MemberInit(Expression.New(wrapperType), wrapperTypeMemberAssignments), context.CurrentParameter);
        }
    }
}
