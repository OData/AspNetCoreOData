//-----------------------------------------------------------------------------
// <copyright file="OrderByBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// Translates an OData $orderby parse tree represented by <see cref="OrderByClause"/> to
    /// an <see cref="Expression"/>.
    /// </summary>
    public class OrderByBinder : FilterOrderByBinderBase, IOrderByBinder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByBinder"/> class.
        /// </summary>
        /// <param name="requestContainer">The request container.</param>
        public OrderByBinder(IServiceProvider requestContainer)
            : base(requestContainer)
        {
        }

        internal OrderByBinder(ODataQuerySettings settings, IAssemblyResolver assembliesResolver, IEdmModel model)
            : base(settings, assembliesResolver, model)
        {
        }

        /// <inheritdoc/>
        public virtual Expression Bind(IQueryable source, OrderByBinderContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            _filterType = context.ElementClrType;
            BaseQuery = source;

            LambdaExpression orderByExpression = BindOrderByClause(context.OrderByClause, context.ElementClrType);

            return orderByExpression;
        }

        internal LambdaExpression BindOrderByClause(OrderByClause orderBy, Type elementType)
        {
            LambdaExpression orderByLambda = BindExpression(orderBy.Expression, orderBy.RangeVariable, elementType);
            return orderByLambda;
        }
    }
}
