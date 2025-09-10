//-----------------------------------------------------------------------------
// <copyright file="QueryBinder.SingleValueFunctionCall.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions;

/// <summary>
/// The base class for all expression binders.
/// </summary>
public abstract partial class QueryBinder
{
    /// <summary>
    /// Binds a <see cref="SingleValueFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
    /// represents the semantics of the <see cref="SingleValueFunctionCallNode"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    public virtual Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context);

        switch (node.Name)
        {
            case ClrCanonicalFunctions.StartswithFunctionName:
                return BindStartsWith(node, context);

            case ClrCanonicalFunctions.EndswithFunctionName:
                return BindEndsWith(node, context);

            case ClrCanonicalFunctions.ContainsFunctionName:
                return BindContains(node, context);

            case ClrCanonicalFunctions.SubstringFunctionName:
                return BindSubstring(node, context);

            case ClrCanonicalFunctions.LengthFunctionName:
                return BindLength(node, context);

            case ClrCanonicalFunctions.IndexofFunctionName:
                return BindIndexOf(node, context);

            case ClrCanonicalFunctions.TolowerFunctionName:
                return BindToLower(node, context);

            case ClrCanonicalFunctions.ToupperFunctionName:
                return BindToUpper(node, context);

            case ClrCanonicalFunctions.TrimFunctionName:
                return BindTrim(node, context);

            case ClrCanonicalFunctions.ConcatFunctionName:
                return BindConcat(node, context);

            case ClrCanonicalFunctions.MatchesPatternFunctionName:
                return BindMatchesPattern(node, context);

            case ClrCanonicalFunctions.YearFunctionName:
            case ClrCanonicalFunctions.MonthFunctionName:
            case ClrCanonicalFunctions.DayFunctionName:
                return BindDateRelatedProperty(node, context); // Date & DateTime & DateTimeOffset

            case ClrCanonicalFunctions.HourFunctionName:
            case ClrCanonicalFunctions.MinuteFunctionName:
            case ClrCanonicalFunctions.SecondFunctionName:
                return BindTimeRelatedProperty(node, context); // TimeOfDay & DateTime & DateTimeOffset

            case ClrCanonicalFunctions.FractionalSecondsFunctionName:
                return BindFractionalSeconds(node, context);

            case ClrCanonicalFunctions.RoundFunctionName:
                return BindRound(node, context);

            case ClrCanonicalFunctions.FloorFunctionName:
                return BindFloor(node, context);

            case ClrCanonicalFunctions.CeilingFunctionName:
                return BindCeiling(node, context);

            case ClrCanonicalFunctions.CastFunctionName:
                return BindCastSingleValue(node, context);

            case ClrCanonicalFunctions.IsofFunctionName:
                return BindIsOf(node, context);

            case ClrCanonicalFunctions.DateFunctionName:
                return BindDate(node, context);

            case ClrCanonicalFunctions.TimeFunctionName:
                return BindTime(node, context);

            case ClrCanonicalFunctions.NowFunctionName:
                return BindNow(node, context);

            default:
                // Get Expression of custom binded method.
                Expression expression = BindCustomMethodExpressionOrNull(node, context);
                if (expression != null)
                {
                    return expression;
                }

                throw new NotImplementedException(Error.Format(SRResources.ODataFunctionNotSupported, node.Name));
        }
    }

    /// <summary>
    /// Binds a 'startswith' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindStartsWith(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "startswith");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.StartsWith, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'endswith' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindEndsWith(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "endswith");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.EndsWith, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'contains' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindContains(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "contains");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Contains, context.QuerySettings, arguments[0], arguments[1]);
    }

    /// <summary>
    /// Binds a 'substring' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindSubstring(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "substring");

        Expression[] arguments = BindArguments(node.Parameters, context);
        if (arguments[0].Type != typeof(string))
        {
            throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, node.Name));
        }

        ODataQuerySettings querySettings = context.QuerySettings;

        Expression functionCall;
        if (arguments.Length == 2)
        {
            Contract.Assert(ExpressionBinderHelper.IsInteger(arguments[1].Type));

            // When null propagation is allowed, we use a safe version of String.Substring(int).
            // But for providers that would not recognize custom expressions like this, we map
            // directly to String.Substring(int)
            if (context.QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // Safe function is static and takes string "this" as first argument
                functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStartNoThrow, querySettings, arguments);
            }
            else
            {
                functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStart, querySettings, arguments);
            }
        }
        else
        {
            // arguments.Length == 3 implies String.Substring(int, int)
            Contract.Assert(arguments.Length == 3 && ExpressionBinderHelper.IsInteger(arguments[1].Type) && ExpressionBinderHelper.IsInteger(arguments[2].Type));

            // When null propagation is allowed, we use a safe version of String.Substring(int, int).
            // But for providers that would not recognize custom expressions like this, we map
            // directly to String.Substring(int, int)
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // Safe function is static and takes string "this" as first argument
                functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLengthNoThrow, querySettings, arguments);
            }
            else
            {
                functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLength, querySettings, arguments);
            }
        }

        return functionCall;
    }

    /// <summary>
    /// Binds a 'length' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindLength(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "length");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Length, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'indexof' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindIndexOf(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "indexof");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.IndexOf, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'tolower' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindToLower(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "tolower");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.ToLower, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'toupper' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindToUpper(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "toupper");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.ToUpper, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'trim' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindTrim(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "trim");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Trim, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'concat' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindConcat(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "concat");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Concat, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds a 'matchesPattern' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindMatchesPattern(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "matchesPattern");

        Expression[] arguments = BindArguments(node.Parameters, context);
        ValidateAllStringArguments(node.Name, arguments);

        Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

        //add argument that must be ECMAScript compatible regex
        arguments = new[] { arguments[0], arguments[1], Expression.Constant(RegexOptions.ECMAScript) };

        return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.MatchesMattern, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds date related functions to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindDateRelatedProperty(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context);

        Expression[] arguments = BindArguments(node.Parameters, context);
        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDateRelated(arguments[0].Type));

        // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
        Expression parameter = arguments[0];

        PropertyInfo property;
        if (ExpressionBinderHelper.IsDate(parameter.Type))
        {
            Contract.Assert(ClrCanonicalFunctions.DateProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.DateProperties[node.Name];
        }
        else if (parameter.Type.IsDateOnly())
        {
            Contract.Assert(ClrCanonicalFunctions.DateOnlyProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.DateOnlyProperties[node.Name];
        }
        else if (ExpressionBinderHelper.IsDateTime(parameter.Type))
        {
            Contract.Assert(ClrCanonicalFunctions.DateTimeProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.DateTimeProperties[node.Name];
        }
        else
        {
            Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
        }

        return ExpressionBinderHelper.MakeFunctionCall(property, context.QuerySettings, parameter);
    }

    /// <summary>
    /// Binds time related functions to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindTimeRelatedProperty(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context);

        Expression[] arguments = BindArguments(node.Parameters, context);

        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsTimeRelated(arguments[0].Type));

        // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
        Expression parameter = arguments[0];

        PropertyInfo property;
        if (ExpressionBinderHelper.IsTimeOfDay(parameter.Type))
        {
            Contract.Assert(ClrCanonicalFunctions.TimeOfDayProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.TimeOfDayProperties[node.Name];
        }
        else if (parameter.Type.IsTimeOnly())
        {
            Contract.Assert(ClrCanonicalFunctions.TimeOnlyProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.TimeOnlyProperties[node.Name];
        }
        else if (ExpressionBinderHelper.IsDateTime(parameter.Type))
        {
            Contract.Assert(ClrCanonicalFunctions.DateTimeProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.DateTimeProperties[node.Name];
        }
        else if (ExpressionBinderHelper.IsTimeSpan(parameter.Type))
        {
            Contract.Assert(ClrCanonicalFunctions.TimeSpanProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.TimeSpanProperties[node.Name];
        }
        else
        {
            Contract.Assert(ClrCanonicalFunctions.DateTimeOffsetProperties.ContainsKey(node.Name));
            property = ClrCanonicalFunctions.DateTimeOffsetProperties[node.Name];
        }

        return ExpressionBinderHelper.MakeFunctionCall(property, context.QuerySettings, parameter);
    }

    /// <summary>
    /// Binds 'fractionalseconds' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindFractionalSeconds(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "fractionalseconds");

        Expression[] arguments = BindArguments(node.Parameters, context);
        Contract.Assert(arguments.Length == 1 && (ExpressionBinderHelper.IsTimeRelated(arguments[0].Type)));

        // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
        Expression parameter = arguments[0];

        PropertyInfo property;
        if (ExpressionBinderHelper.IsTimeOfDay(parameter.Type))
        {
            property = ClrCanonicalFunctions.TimeOfDayProperties[ClrCanonicalFunctions.MillisecondFunctionName];
        }
        else if (ExpressionBinderHelper.IsDateTime(parameter.Type))
        {
            property = ClrCanonicalFunctions.DateTimeProperties[ClrCanonicalFunctions.MillisecondFunctionName];
        }
        else if (ExpressionBinderHelper.IsTimeSpan(parameter.Type))
        {
            property = ClrCanonicalFunctions.TimeSpanProperties[ClrCanonicalFunctions.MillisecondFunctionName];
        }
        else
        {
            property = ClrCanonicalFunctions.DateTimeOffsetProperties[ClrCanonicalFunctions.MillisecondFunctionName];
        }

        // Millisecond
        Expression milliSecond = ExpressionBinderHelper.MakePropertyAccess(property, parameter, context.QuerySettings);
        Expression decimalMilliSecond = Expression.Convert(milliSecond, typeof(decimal));
        Expression fractionalSeconds = Expression.Divide(decimalMilliSecond, Expression.Constant(1000m, typeof(decimal)));

        return ExpressionBinderHelper.CreateFunctionCallWithNullPropagation(fractionalSeconds, arguments, context.QuerySettings);
    }

    /// <summary>
    /// Binds 'round' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindRound(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "round");

        Expression[] arguments = BindArguments(node.Parameters, context);

        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDoubleOrDecimal(arguments[0].Type));

        MethodInfo round = ExpressionBinderHelper.IsType<double>(arguments[0].Type)
            ? ClrCanonicalFunctions.RoundOfDouble
            : ClrCanonicalFunctions.RoundOfDecimal;
        return ExpressionBinderHelper.MakeFunctionCall(round, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds 'floor' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindFloor(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "floor");

        Expression[] arguments = BindArguments(node.Parameters, context);

        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDoubleOrDecimal(arguments[0].Type));

        MethodInfo floor = ExpressionBinderHelper.IsType<double>(arguments[0].Type)
            ? ClrCanonicalFunctions.FloorOfDouble
            : ClrCanonicalFunctions.FloorOfDecimal;
        return ExpressionBinderHelper.MakeFunctionCall(floor, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds 'ceiling' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindCeiling(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "ceiling");

        Expression[] arguments = BindArguments(node.Parameters, context);

        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDoubleOrDecimal(arguments[0].Type));

        MethodInfo ceiling = ExpressionBinderHelper.IsType<double>(arguments[0].Type)
            ? ClrCanonicalFunctions.CeilingOfDouble
            : ClrCanonicalFunctions.CeilingOfDecimal;
        return ExpressionBinderHelper.MakeFunctionCall(ceiling, context.QuerySettings, arguments);
    }

    /// <summary>
    /// Binds 'cast' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindCastSingleValue(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "cast");

        Expression[] arguments = BindArguments(node.Parameters, context);
        Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

        Expression source = arguments.Length == 1 ? context.CurrentParameter : arguments[0];

        string targetTypeName = null;
        QueryNode queryNode = node.Parameters.Last();
        if (queryNode is ConstantNode constantNode)
        {
            targetTypeName = (string)constantNode.Value;
        }
        else if (queryNode is SingleResourceCastNode singleResourceCastNode)
        {
            targetTypeName = singleResourceCastNode.TypeReference.FullName();
        }

        IEdmType targetEdmType = context.Model.FindType(targetTypeName);
        Type targetClrType = null;

        if (targetEdmType != null)
        {
            IEdmTypeReference targetEdmTypeReference = targetEdmType.ToEdmTypeReference(false);
            targetClrType = context.Model.GetClrType(targetEdmTypeReference);

            if (source != NullConstant)
            {
                if (source.Type == targetClrType)
                {
                    return source;
                }

                if ((!targetEdmTypeReference.IsPrimitive() && !targetEdmTypeReference.IsEnum()) ||
                    (context.Model.GetEdmPrimitiveTypeReference(source.Type) == null && !TypeHelper.IsEnum(source.Type)))
                {
                    // Cast fails and return null.
                    return NullConstant;
                }
            }
        }

        if (targetClrType == null || source == NullConstant)
        {
            return NullConstant;
        }

        if (targetClrType == typeof(string))
        {
            return ExpressionBinderHelper.BindCastToStringType(source);
        }
        else if (TypeHelper.IsEnum(targetClrType))
        {
            return BindCastToEnumType(source.Type, targetClrType, node.Parameters.First(), arguments.Length, context);
        }
        else
        {
            if (TypeHelper.IsNullable(source.Type) && !TypeHelper.IsNullable(targetClrType))
            {
                // Make the target Clr type nullable to avoid failure while casting
                // nullable source, whose value may be null, to a non-nullable type.
                // For example: cast(NullableInt32Property,Edm.Int64)
                // The target Clr type should be Nullable<Int64> rather than Int64.
                targetClrType = typeof(Nullable<>).MakeGenericType(targetClrType);
            }

            try
            {
                return Expression.Convert(source, targetClrType);
            }
            catch (InvalidOperationException)
            {
                // Cast fails and return null.
                return NullConstant;
            }
        }
    }

    /// <summary>
    /// Binds a 'isof' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindIsOf(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "isof");

        Expression[] arguments = BindArguments(node.Parameters, context);

        // Edm.Boolean isof(type)  or
        // Edm.Boolean isof(expression,type)
        Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

        Expression source = arguments.Length == 1 ? context.CurrentParameter : arguments[0];
        if (source == NullConstant)
        {
            return FalseConstant;
        }

        string typeName = null;
        QueryNode queryNode = node.Parameters.Last();
        if (queryNode is ConstantNode constantNode)
        {
            typeName = (string)constantNode.Value;
        }
        else if (queryNode is SingleResourceCastNode singleResourceCastNode)
        {
            typeName = singleResourceCastNode.TypeReference.FullName();
        }

        IEdmType edmType = context.Model.FindType(typeName);
        Type clrType = null;
        if (edmType != null)
        {
            // bool nullable = source.Type.IsNullable();
            IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(false);
            clrType = context.Model.GetClrType(edmTypeReference);
        }

        if (clrType == null)
        {
            return FalseConstant;
        }

        bool isSourcePrimitiveOrEnum = context.Model.GetEdmPrimitiveTypeReference(source.Type) != null ||
                                       TypeHelper.IsEnum(source.Type);

        bool isTargetPrimitiveOrEnum = context.Model.GetEdmPrimitiveTypeReference(clrType) != null ||
                                       TypeHelper.IsEnum(clrType);

        if (isSourcePrimitiveOrEnum && isTargetPrimitiveOrEnum)
        {
            if (TypeHelper.IsNullable(source.Type))
            {
                clrType = TypeHelper.ToNullable(clrType);
            }
        }

        // Be caution: Type method of LINQ to Entities only supports entity type.
        return Expression.Condition(Expression.TypeIs(source, clrType), TrueConstant, FalseConstant);
    }

    /// <summary>
    /// Binds a 'date' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindDate(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "date");

        Expression[] arguments = BindArguments(node.Parameters, context);

        // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDateOrOffset(arguments[0].Type));

        // EF doesn't support new Date(int, int, int), also doesn't support other property access, for example DateTime.Date.
        // Therefore, we just return the source (DateTime or DateTimeOffset).
        return arguments[0];
    }

    /// <summary>
    /// Binds a 'time' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindTime(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "time");

        Expression[] arguments = BindArguments(node.Parameters, context);

        // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
        Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDateOrOffset(arguments[0].Type));

        // EF doesn't support new TimeOfDay(int, int, int, int), also doesn't support other property access, for example DateTimeOffset.DateTime.
        // Therefore, we just return the source (DateTime or DateTimeOffset).
        return arguments[0];
    }

    /// <summary>
    /// Binds a 'now' function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindNow(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context, "now");

        // Function Now() does not take any arguments.
        Expression[] arguments = BindArguments(node.Parameters, context);
        Contract.Assert(arguments.Length == 0);

        return Expression.Property(null, typeof(DateTimeOffset), "UtcNow");
    }

    /// <summary>
    /// Binds customized function to create a LINQ <see cref="Expression"/>.
    /// </summary>
    /// <param name="node">The query node to bind.</param>
    /// <param name="context">The query binder context.</param>
    /// <returns>The LINQ <see cref="Expression"/> created.</returns>
    protected virtual Expression BindCustomMethodExpressionOrNull(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        CheckArgumentNull(node, context);

        Expression[] arguments = BindArguments(node.Parameters, context);
        IEnumerable<Type> methodArgumentsType = arguments.Select(argument => argument.Type);

        // Search for custom method info that are binded to the node name
        MethodInfo methodInfo;
        if (UriFunctionsBinder.TryGetMethodInfo(node.Name, methodArgumentsType, out methodInfo))
        {
            return ExpressionBinderHelper.MakeCustomFunctionCall(methodInfo, arguments);
        }

        return null;
    }

    [DebuggerStepThrough]
    private static void CheckArgumentNull(SingleValueFunctionCallNode node, QueryBinderContext context, string nodeName)
    {
        if (node == null || node.Name != nodeName)
        {
            throw Error.ArgumentNull(nameof(node));
        }

        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }
    }
}
