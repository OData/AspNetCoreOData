//-----------------------------------------------------------------------------
// <copyright file="DollarSearchBinder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.E2E.Tests.DollarSearch;

/// <summary>
/// A simple search binder
/// </summary>
public class DollarSearchBinder : QueryBinder, ISearchBinder
{
    internal static readonly MethodInfo StringEqualsMethodInfo = typeof(string).GetMethod("Equals",
        new[]
        {
            typeof(string),
            typeof(string),
            typeof(StringComparison)
        });

    private static ISet<string> _categories = new HashSet<string>
    {
        "food", "office", "device"
    };

    private static ISet<string> _colors = new HashSet<string>
    {
        "white", "red", "green", "blue", "brown"
    };

    public Expression BindSearch(SearchClause searchClause, QueryBinderContext context)
    {
        Expression exp = BindSingleValueNode(searchClause.Expression, context);

        LambdaExpression lambdaExp = Expression.Lambda(exp, context.CurrentParameter);

        return lambdaExp;
    }

    public override Expression BindSingleValueNode(SingleValueNode node, QueryBinderContext context)
    {
        switch (node.Kind)
        {
            case QueryNodeKind.BinaryOperator:
                return BindBinaryOperatorNode(node as BinaryOperatorNode, context);

            case QueryNodeKind.SearchTerm:
                return BindSearchTerm(node as SearchTermNode, context);

            case QueryNodeKind.UnaryOperator:
                return BindUnaryOperatorNode(node as UnaryOperatorNode, context);
        }

        return null;
    }

    public override Expression BindBinaryOperatorNode(BinaryOperatorNode binaryOperatorNode, QueryBinderContext context)
    {
        Expression left = Bind(binaryOperatorNode.Left, context);

        Expression right = Bind(binaryOperatorNode.Right, context);

        return ExpressionBinderHelper.CreateBinaryExpression(binaryOperatorNode.OperatorKind, left, right, liftToNull: false, context.QuerySettings);
    }

    public Expression BindSearchTerm(SearchTermNode node, QueryBinderContext context)
    {
        // Source is from context;
        Expression source = context.CurrentParameter;

        string text = node.Text.ToLowerInvariant();
        if (_categories.Contains(text))
        {
            // $it.Category
            Expression categoryProperty = Expression.Property(source, "Category");

            // $it.Category.Name
            Expression categoryName = Expression.Property(categoryProperty, "Name");

            // string.Equals($it.Category.Name, text, StringComparison.OrdinalIgnoreCase);
            return Expression.Call(null, StringEqualsMethodInfo,
                categoryName, Expression.Constant(text, typeof(string)), Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));
        }
        else if (_colors.Contains(text))
        {
            // $it.Color
            Expression colorProperty = Expression.Property(source, "Color");

            // $it.Color.ToString()
            Expression colorPropertyString = Expression.Call(colorProperty, "ToString", typeArguments: null, arguments: null);

            // string.Equals($it.Color.ToString(), text, StringComparison.OrdinalIgnoreCase);
            return Expression.Call(null, StringEqualsMethodInfo,
                colorPropertyString, Expression.Constant(text, typeof(string)), Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)));
        }
        else
        {
            return Expression.Constant(false, typeof(bool));
        }
    }
}
