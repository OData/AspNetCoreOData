//-----------------------------------------------------------------------------
// <copyright file="OrderByBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    public class OrderByBinderResult
    {
        public OrderByBinderResult(Expression orderByExpression, OrderByDirection direction)
        {
            OrderByExpression = orderByExpression;
            Direction = direction;
        }

        public Expression OrderByExpression { get; }

        public OrderByDirection Direction { get; }

        public OrderByBinderResult ThenBy { get; set; }
    }

    public interface IOrderByBinder
    {
        // pet => pet.Age
        // Expression BindOrderBy(OrderByClause orderByClause, QueryBinderContext context);

        OrderByBinderResult BindOrderBy(OrderByClause orderByClause, QueryBinderContext context);
    }

    /// <summary>
    /// The default implementation to bind an OData $filter represented by <see cref="FilterClause"/>
    /// or $orderby represented by <see cref="OrderByClause"/> to an <see cref="Expression"/>.
    /// </summary>
    public class OrderByBinder : QueryBinder, IOrderByBinder
    {
        /// <summary>
        /// Binds an OData $orderby represented by <see cref="OrderByClause"/> to an <see cref="Expression"/>.
        /// </summary>
        /// <param name="orderByClause">The orderby clause <see cref="OrderByClause"/>.</param>
        /// <param name="context">The query binder context.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindOrderBy1(OrderByClause orderByClause, QueryBinderContext context)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull(nameof(orderByClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            LambdaExpression orderByLambda = BindExpression(orderByClause.Expression, orderByClause.RangeVariable, context);
            return orderByLambda;
        }

        public virtual OrderByBinderResult BindOrderBy(OrderByClause orderByClause, QueryBinderContext context)
        {
            if (orderByClause == null)
            {
                throw Error.ArgumentNull(nameof(orderByClause));
            }

            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            OrderByBinderResult head = null;
            OrderByBinderResult last = null;
            for (OrderByClause clause = orderByClause; clause != null; clause = clause.ThenBy)
            {
                LambdaExpression orderByLambda = BindExpression(clause.Expression, clause.RangeVariable, context);

                OrderByBinderResult result = new OrderByBinderResult(orderByLambda, clause.Direction);

                if (head == null)
                {
                    head = result;
                    last = result;
                }
                else
                {
                    Contract.Assert(last != null);
                    last.ThenBy = result;
                    last = result;
                }
            }

            return head;
        }
    }
}
