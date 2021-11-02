//-----------------------------------------------------------------------------
// <copyright file="OrderByBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
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
        [SuppressMessage("ApiDesign", "RS0022:Constructor make noninheritable base class inheritable", Justification = "We have multi-level inheritance")]
        public OrderByBinder(IServiceProvider requestContainer)
            : base(requestContainer)
        {
        }

        internal OrderByBinder(ODataQuerySettings settings, IAssemblyResolver assembliesResolver, IEdmModel model)
            : base(settings, assembliesResolver, model)
        {
        }

        /// <inheritdoc/>
        public virtual LambdaExpression Bind(OrderByBinderContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            _filterType = context.ElementClrType;

            return BindOrderByClause(context.OrderByClause, context.ElementClrType);
        }

        /// <inheritdoc/>
        public virtual IQueryable Bind(IQueryable source, OrderByBinderContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            _filterType = context.ElementClrType;

            return BindOrderByClause(source, context.OrderByClause, context.ElementClrType, context.AlreadyOrdered);
        }

        internal LambdaExpression BindOrderByClause(OrderByClause orderBy, Type elementType)
        {
            return BindExpression(orderBy.Expression, orderBy.RangeVariable, elementType);
        }

        internal IQueryable BindOrderByClause(IQueryable source, OrderByClause orderBy, Type elementType, bool alreadyOrdered = false)
        {
            LambdaExpression orderByExpression = BindExpression(orderBy.Expression, orderBy.RangeVariable, elementType);
            return ExpressionHelpers.OrderBy(source, orderByExpression, orderBy.Direction, elementType,
                alreadyOrdered);
        }
    }
}
