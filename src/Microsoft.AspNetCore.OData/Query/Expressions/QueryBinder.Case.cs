//-----------------------------------------------------------------------------
// <copyright file="QueryBinder.Case.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions;

public abstract partial class QueryBinder
{
    /// <summary>
    /// Binds a 'case' function with compatible scalar results to a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The first matching result, or null if no condition matches.</returns>
    protected virtual Expression BindCase(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, ClrCanonicalFunctions.CaseFunctionName);

        QueryNode[] arguments = node.Parameters.ToArray();
        if (arguments.Length is 0 || arguments.Length % 2 is not 0)
        {
            throw new ODataException(SRResources.CaseRequiresConditionResultPairs);
        }

        Expression[] values = new Expression[arguments.Length / 2];
        for (int index = 0; index < values.Length; index++)
        {
            values[index] = Bind(arguments[index * 2 + 1], context);
        }

        Type resultType = GetCaseResultType(values, context);
        if (resultType.IsValueType)
        {
            resultType = typeof(Nullable<>).MakeGenericType(resultType);
        }

        // Nest the pairs in reverse order so evaluation stops at the first true condition.
        Expression result = Expression.Constant(null, resultType);
        for (int index = arguments.Length - 2; index is >= 0; index -= 2)
        {
            Expression condition = Bind(arguments[index], context);
            if (condition.Type == typeof(bool?))
            {
                condition = Expression.Equal(condition, Expression.Constant((bool?)true, typeof(bool?)));
            }
            else if (condition.Type != typeof(bool))
            {
                throw new ODataException(SRResources.CaseRequiresBooleanConditions);
            }

            Expression value = values[index / 2];
            if (value is ConstantExpression { Value: null })
            {
                value = Expression.Constant(null, resultType);
            }
            else if (value.Type != resultType)
            {
                value = Expression.Convert(value, resultType);
            }

            result = Expression.Condition(condition, value, result);
        }

        return result;
    }

    private static Type GetCaseResultType(Expression[] values, QueryBinderContext context)
    {
        // ODataLib can report Edm.Untyped for compatible values with different nullability.
        // Infer from the bound expressions to preserve their scalar CLR type for query providers.
        Type resultType = null;
        IODataTypeMapper mapper = context.Model.GetTypeMapper();
        foreach (Expression value in values)
        {
            if (value is ConstantExpression { Value: null })
            {
                continue;
            }

            Type valueType = Nullable.GetUnderlyingType(value.Type) ?? value.Type;
            IEdmTypeReference edmType = mapper.GetEdmTypeReference(context.Model, valueType);
            if (edmType is not { Definition.TypeKind: EdmTypeKind.Primitive or EdmTypeKind.Enum })
            {
                throw new ODataException(SRResources.CaseRequiresScalarResults);
            }

            if (resultType is null || resultType == valueType)
            {
                resultType = valueType;
                continue;
            }

            int resultRank = GetCaseNumericRank(resultType);
            int valueRank = GetCaseNumericRank(valueType);
            if (resultRank is 0 || valueRank is 0)
            {
                throw new ODataException(SRResources.CaseRequiresCompatibleResults);
            }

            resultType = resultRank is <= 4 && valueRank is <= 4
                ? typeof(int)
                : resultRank >= valueRank ? resultType : valueType;
        }

        return resultType ?? typeof(object);
    }

    private static int GetCaseNumericRank(Type type) => type.IsEnum ? 0 : Type.GetTypeCode(type) switch
    {
        TypeCode.Byte => 1,
        TypeCode.SByte => 2,
        TypeCode.Int16 => 3,
        TypeCode.Int32 => 4,
        TypeCode.Int64 => 5,
        TypeCode.Decimal => 6,
        TypeCode.Single => 7,
        TypeCode.Double => 8,
        _ => 0,
    };
}
