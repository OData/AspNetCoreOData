//-----------------------------------------------------------------------------
// <copyright file="FilterBinderTestsHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Tests.Query.Expressions
{
    public static class FilterBinderTestsHelper
    {
        public static Expression TestBind(FilterClause filterClause, Type filterType, IEdmModel model,
            IAssemblyResolver assembliesResolver, ODataQuerySettings querySettings)
        {
            if (filterClause == null)
            {
                throw Error.ArgumentNull(nameof(filterClause));
            }

            if (filterType == null)
            {
                throw Error.ArgumentNull(nameof(filterType));
            }

            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (assembliesResolver == null)
            {
                throw Error.ArgumentNull(nameof(assembliesResolver));
            }

            FilterBinder binder = new FilterBinder(querySettings, assembliesResolver, model, filterType);
            ODataQueryContext context = new ODataQueryContext(model, filterType);

            FilterBinderContext filterBinderContext = new FilterBinderContext()
            {
                Source = null,
                FilterClause = filterClause,
                QueryContext = context,
                QuerySettings = querySettings,
                ElementClrType = filterType
            };

            return binder.BindFilterClause(filterBinderContext);
        }
    }

    public class MyNoneQueryNode : QueryNode
    {
        public override QueryNodeKind Kind => QueryNodeKind.None;
    }
}
