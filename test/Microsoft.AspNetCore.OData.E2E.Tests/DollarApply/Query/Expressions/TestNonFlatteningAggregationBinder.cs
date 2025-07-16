//-----------------------------------------------------------------------------
// <copyright file="TestNonFlatteningAggregationBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarApply.Query.Expressions;

internal class TestNonFlatteningAggregationBinder : QueryBinder, IAggregationBinder
{
    private readonly IAggregationBinder aggregationBinder = new TestAggregationBinder();

    public Expression BindGroupBy(TransformationNode transformationNode, QueryBinderContext context)
    {
        return aggregationBinder.BindGroupBy(transformationNode, context);
    }

    public Expression BindSelect(TransformationNode transformationNode, QueryBinderContext context)
    {
        return aggregationBinder.BindSelect(transformationNode, context);
    }
}
