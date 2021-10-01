//-----------------------------------------------------------------------------
// <copyright file="ExpressionBinderHelper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// The helper class for all expression binders.
    /// </summary>
    internal static class ExpressionBinderHelper
    {
        private static readonly MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) });
        private static readonly MethodInfo GuidCompareMethodInfo = typeof(Guid).GetMethod("CompareTo", new[] { typeof(Guid) });

        private static readonly Expression NullConstant = Expression.Constant(null);
        private static readonly Expression FalseConstant = Expression.Constant(false);
        private static readonly Expression TrueConstant = Expression.Constant(true);
        private static readonly Expression ZeroConstant = Expression.Constant(0);

        private static readonly Dictionary<BinaryOperatorKind, ExpressionType> BinaryOperatorMapping = new Dictionary<BinaryOperatorKind, ExpressionType>
        {
            { BinaryOperatorKind.Add, ExpressionType.Add },
            { BinaryOperatorKind.And, ExpressionType.AndAlso },
            { BinaryOperatorKind.Divide, ExpressionType.Divide },
            { BinaryOperatorKind.Equal, ExpressionType.Equal },
            { BinaryOperatorKind.GreaterThan, ExpressionType.GreaterThan },
            { BinaryOperatorKind.GreaterThanOrEqual, ExpressionType.GreaterThanOrEqual },
            { BinaryOperatorKind.LessThan, ExpressionType.LessThan },
            { BinaryOperatorKind.LessThanOrEqual, ExpressionType.LessThanOrEqual },
            { BinaryOperatorKind.Modulo, ExpressionType.Modulo },
            { BinaryOperatorKind.Multiply, ExpressionType.Multiply },
            { BinaryOperatorKind.NotEqual, ExpressionType.NotEqual },
            { BinaryOperatorKind.Or, ExpressionType.OrElse },
            { BinaryOperatorKind.Subtract, ExpressionType.Subtract },
        };

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
        public static Expression CreateBinaryExpression(BinaryOperatorKind binaryOperator, Expression left, Expression right, bool liftToNull, ODataQuerySettings querySettings)
        {
            ExpressionType binaryExpressionType;

            // When comparing an enum to a string, parse the string, convert both to the enum underlying type, and compare the values
            // When comparing an enum to an enum with the same type, convert both to the underlying type, and compare the values
            Type leftUnderlyingType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
            Type rightUnderlyingType = Nullable.GetUnderlyingType(right.Type) ?? right.Type;

            // Convert to integers unless Enum type is required
            if ((TypeHelper.IsEnum(leftUnderlyingType) || TypeHelper.IsEnum(rightUnderlyingType)) && binaryOperator != BinaryOperatorKind.Has)
            {
                Type enumType = TypeHelper.IsEnum(leftUnderlyingType) ? leftUnderlyingType : rightUnderlyingType;
                Type enumUnderlyingType = Enum.GetUnderlyingType(enumType);
                left = ConvertToEnumUnderlyingType(left, enumType, enumUnderlyingType);
                right = ConvertToEnumUnderlyingType(right, enumType, enumUnderlyingType);
            }

            if (leftUnderlyingType == typeof(DateTime) && rightUnderlyingType == typeof(DateTimeOffset))
            {
                right = DateTimeOffsetToDateTime(right, querySettings.TimeZone, querySettings);
            }
            else if (rightUnderlyingType == typeof(DateTime) && leftUnderlyingType == typeof(DateTimeOffset))
            {
                left = DateTimeOffsetToDateTime(left, querySettings.TimeZone, querySettings);
            }

            if ((IsDateOrOffset(leftUnderlyingType) && IsDate(rightUnderlyingType)) ||
                (IsDate(leftUnderlyingType) && IsDateOrOffset(rightUnderlyingType)))
            {
                left = CreateDateBinaryExpression(left, querySettings);
                right = CreateDateBinaryExpression(right, querySettings);
            }

            if ((IsDateOrOffset(leftUnderlyingType) && IsTimeOfDay(rightUnderlyingType)) ||
                (IsTimeOfDay(leftUnderlyingType) && IsDateOrOffset(rightUnderlyingType)) ||
                (IsTimeSpan(leftUnderlyingType) && IsTimeOfDay(rightUnderlyingType)) ||
                (IsTimeOfDay(leftUnderlyingType) && IsTimeSpan(rightUnderlyingType)))
            {
                left = CreateTimeBinaryExpression(left, querySettings);
                right = CreateTimeBinaryExpression(right, querySettings);
            }

            if (left.Type != right.Type)
            {
                // one of them must be nullable and the other is not.
                left = ToNullable(left);
                right = ToNullable(right);
            }

            if (left.Type == typeof(Guid) || right.Type == typeof(Guid))
            {
                switch (binaryOperator)
                {
                    case BinaryOperatorKind.GreaterThan:
                    case BinaryOperatorKind.GreaterThanOrEqual:
                    case BinaryOperatorKind.LessThan:
                    case BinaryOperatorKind.LessThanOrEqual:
                        left = Expression.Call(left, GuidCompareMethodInfo, right);
                        right = ZeroConstant;
                        break;
                    default:
                        break;
                }
            }

            if (left.Type == typeof(string) || right.Type == typeof(string))
            {
                // convert nulls of type object to nulls of type string to make the String.Compare call work
                left = ConvertNull(left, typeof(string));
                right = ConvertNull(right, typeof(string));

                // Use string.Compare instead of comparison for gt, ge, lt, le between two strings since direct comparisons are not supported
                switch (binaryOperator)
                {
                    case BinaryOperatorKind.GreaterThan:
                    case BinaryOperatorKind.GreaterThanOrEqual:
                    case BinaryOperatorKind.LessThan:
                    case BinaryOperatorKind.LessThanOrEqual:
                        left = Expression.Call(StringCompareMethodInfo, left, right);
                        right = ZeroConstant;
                        break;
                    default:
                        break;
                }
            }

            if (BinaryOperatorMapping.TryGetValue(binaryOperator, out binaryExpressionType))
            {
                if (left.Type == typeof(byte[]) || right.Type == typeof(byte[]))
                {
                    left = ConvertNull(left, typeof(byte[]));
                    right = ConvertNull(right, typeof(byte[]));

                    switch (binaryExpressionType)
                    {
                        case ExpressionType.Equal:
                            return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: Linq2ObjectsComparisonMethods.AreByteArraysEqualMethodInfo);
                        case ExpressionType.NotEqual:
                            return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: Linq2ObjectsComparisonMethods.AreByteArraysNotEqualMethodInfo);
                        default:
                            IEdmPrimitiveType binaryType = typeof(byte[]).GetEdmPrimitiveType();
                            throw new ODataException(Error.Format(SRResources.BinaryOperatorNotSupported, binaryType.FullName(), binaryType.FullName(), binaryOperator));
                    }
                }
                else
                {
                    return Expression.MakeBinary(binaryExpressionType, left, right, liftToNull, method: null);
                }
            }
            else
            {
                // Enum has a "has" operator
                // {(c1, c2) => c1.HasFlag(Convert(c2))}
                if (TypeHelper.IsEnum(left.Type) && TypeHelper.IsEnum(right.Type) && binaryOperator == BinaryOperatorKind.Has)
                {
                    UnaryExpression flag = Expression.Convert(right, typeof(Enum));
                    return BindHas(left, flag, querySettings);
                }
                else
                {
                    throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, binaryOperator, typeof(ExpressionBinderBase).Name);
                }
            }
        }

        public static Expression MakePropertyAccess(PropertyInfo propertyInfo, Expression argument, ODataQuerySettings querySettings)
        {
            Expression propertyArgument = argument;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // we don't have to check if the argument is null inside the function call as we do it already
                // before calling the function. So remove the redundant null checks.
                propertyArgument = RemoveInnerNullPropagation(argument, querySettings);
            }

            // if the argument is of type Nullable<T>, then translate the argument to Nullable<T>.Value as none
            // of the canonical functions have overloads for Nullable<> arguments.
            propertyArgument = ExtractValueFromNullableExpression(propertyArgument);

            return Expression.Property(propertyArgument, propertyInfo);
        }

        // creates an expression for the corresponding OData function.
        public static Expression MakeFunctionCall(MemberInfo member, ODataQuerySettings querySettings, params Expression[] arguments)
        {
            Contract.Assert(member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Method);

            IEnumerable<Expression> functionCallArguments = arguments;
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // we don't have to check if the argument is null inside the function call as we do it already
                // before calling the function. So remove the redundant null checks.
                functionCallArguments = arguments.Select(a => RemoveInnerNullPropagation(a, querySettings));
            }

            // if the argument is of type Nullable<T>, then translate the argument to Nullable<T>.Value as none
            // of the canonical functions have overloads for Nullable<> arguments.
            functionCallArguments = ExtractValueFromNullableArguments(functionCallArguments);

            Expression functionCall;
            if (member.MemberType == MemberTypes.Method)
            {
                MethodInfo method = member as MethodInfo;
                if (method.IsStatic)
                {
                    functionCall = Expression.Call(null, method, functionCallArguments);
                }
                else
                {
                    functionCall = Expression.Call(functionCallArguments.First(), method, functionCallArguments.Skip(1));
                }
            }
            else
            {
                // property
                functionCall = Expression.Property(functionCallArguments.First(), member as PropertyInfo);
            }

            return CreateFunctionCallWithNullPropagation(functionCall, arguments, querySettings);
        }

        public static Expression CreateFunctionCallWithNullPropagation(Expression functionCall, Expression[] arguments, ODataQuerySettings querySettings)
        {
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                Expression test = CheckIfArgumentsAreNull(arguments);

                if (test == FalseConstant)
                {
                    // none of the arguments are/can be null.
                    // so no need to do any null propagation
                    return functionCall;
                }
                else
                {
                    // if one of the arguments is null, result is null (not defined)
                    return
                        Expression.Condition(
                        test: test,
                        ifTrue: Expression.Constant(null, ToNullable(functionCall.Type)),
                        ifFalse: ToNullable(functionCall));
                }
            }
            else
            {
                return functionCall;
            }
        }

        // we don't have to do null checks inside the function for arguments as we do the null checks before calling
        // the function when null propagation is enabled.
        // this method converts back "arg == null ? null : convert(arg)" to "arg"
        // Also, note that we can do this generically only because none of the odata functions that we support can take null
        // as an argument.
        public static Expression RemoveInnerNullPropagation(Expression expression, ODataQuerySettings querySettings)
        {
            Contract.Assert(expression != null);

            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.True)
            {
                // only null propagation generates conditional expressions
                if (expression.NodeType == ExpressionType.Conditional)
                {
                    // make sure to skip the DateTime IFF clause
                    ConditionalExpression conditionalExpr = (ConditionalExpression)expression;
                    if (conditionalExpr.Test.NodeType != ExpressionType.OrElse)
                    {
                        expression = conditionalExpr.IfFalse;
                        Contract.Assert(expression != null);

                        if (expression.NodeType == ExpressionType.Convert)
                        {
                            UnaryExpression unaryExpression = expression as UnaryExpression;
                            Contract.Assert(unaryExpression != null);

                            if (Nullable.GetUnderlyingType(unaryExpression.Type) == unaryExpression.Operand.Type)
                            {
                                // this is a cast from T to Nullable<T> which is redundant.
                                expression = unaryExpression.Operand;
                            }
                        }
                    }
                }
            }

            return expression;
        }

        private static Expression CheckIfArgumentsAreNull(Expression[] arguments)
        {
            if (arguments.Any(arg => arg == NullConstant))
            {
                return TrueConstant;
            }

            arguments =
                arguments
                .Select(arg => CheckForNull(arg))
                .Where(arg => arg != null)
                .ToArray();

            if (arguments.Any())
            {
                return arguments
                    .Aggregate((left, right) => Expression.OrElse(left, right));
            }
            else
            {
                return FalseConstant;
            }
        }

        public static Expression CheckForNull(Expression expression)
        {
            if (IsNullable(expression.Type) && expression.NodeType != ExpressionType.Constant)
            {
                return Expression.Equal(expression, Expression.Constant(null));
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<Expression> ExtractValueFromNullableArguments(IEnumerable<Expression> arguments)
        {
            return arguments.Select(arg => ExtractValueFromNullableExpression(arg));
        }

        public static Expression ExtractValueFromNullableExpression(Expression source)
        {
            return Nullable.GetUnderlyingType(source.Type) != null ? Expression.Property(source, "Value") : source;
        }

        public static Expression BindHas(Expression left, Expression flag, ODataQuerySettings querySettings)
        {
            Contract.Assert(TypeHelper.IsEnum(left.Type));
            Contract.Assert(flag.Type == typeof(Enum));

            Expression[] arguments = new[] { left, flag };
            return MakeFunctionCall(ClrCanonicalFunctions.HasFlag, querySettings, arguments);
        }

        private static Expression GetProperty(Expression source, string propertyName, ODataQuerySettings querySettings)
        {
            if (IsDateOrOffset(source.Type))
            {
                if (IsDateTime(source.Type))
                {
                    return MakePropertyAccess(ClrCanonicalFunctions.DateTimeProperties[propertyName], source, querySettings);
                }
                else
                {
                    return MakePropertyAccess(ClrCanonicalFunctions.DateTimeOffsetProperties[propertyName], source, querySettings);
                }
            }
            else if (IsDate(source.Type))
            {
                return MakePropertyAccess(ClrCanonicalFunctions.DateProperties[propertyName], source, querySettings);
            }
            else if (IsTimeOfDay(source.Type))
            {
                return MakePropertyAccess(ClrCanonicalFunctions.TimeOfDayProperties[propertyName], source, querySettings);
            }
            else if (IsTimeSpan(source.Type))
            {
                return MakePropertyAccess(ClrCanonicalFunctions.TimeSpanProperties[propertyName], source, querySettings);
            }

            return source;
        }

        private static Expression CreateDateBinaryExpression(Expression source, ODataQuerySettings querySettings)
        {
            source = ConvertToDateTimeRelatedConstExpression(source);

            // Year, Month, Day
            Expression year = GetProperty(source, ClrCanonicalFunctions.YearFunctionName, querySettings);
            Expression month = GetProperty(source, ClrCanonicalFunctions.MonthFunctionName, querySettings);
            Expression day = GetProperty(source, ClrCanonicalFunctions.DayFunctionName, querySettings);

            // return (year * 10000 + month * 100 + day)
            Expression result =
                Expression.Add(
                    Expression.Add(Expression.Multiply(year, Expression.Constant(10000)),
                        Expression.Multiply(month, Expression.Constant(100))), day);

            return CreateFunctionCallWithNullPropagation(result, new[] { source }, querySettings);
        }

        private static Expression CreateTimeBinaryExpression(Expression source, ODataQuerySettings querySettings)
        {
            source = ConvertToDateTimeRelatedConstExpression(source);

            // Hour, Minute, Second, Millisecond
            Expression hour = GetProperty(source, ClrCanonicalFunctions.HourFunctionName, querySettings);
            Expression minute = GetProperty(source, ClrCanonicalFunctions.MinuteFunctionName, querySettings);
            Expression second = GetProperty(source, ClrCanonicalFunctions.SecondFunctionName, querySettings);
            Expression milliSecond = GetProperty(source, ClrCanonicalFunctions.MillisecondFunctionName, querySettings);

            Expression hourTicks = Expression.Multiply(Expression.Convert(hour, typeof(long)), Expression.Constant(TimeOfDay.TicksPerHour));
            Expression minuteTicks = Expression.Multiply(Expression.Convert(minute, typeof(long)), Expression.Constant(TimeOfDay.TicksPerMinute));
            Expression secondTicks = Expression.Multiply(Expression.Convert(second, typeof(long)), Expression.Constant(TimeOfDay.TicksPerSecond));

            // return (hour * TicksPerHour + minute * TicksPerMinute + second * TicksPerSecond + millisecond)
            Expression result = Expression.Add(hourTicks, Expression.Add(minuteTicks, Expression.Add(secondTicks, Expression.Convert(milliSecond, typeof(long)))));

            return CreateFunctionCallWithNullPropagation(result, new[] { source }, querySettings);
        }

        private static Expression ConvertToDateTimeRelatedConstExpression(Expression source)
        {
            var parameterizedConstantValue = ExtractParameterizedConstant(source);
            if (parameterizedConstantValue != null && TypeHelper.IsNullable(source.Type))
            {
                var dateTimeOffset = parameterizedConstantValue as DateTimeOffset?;
                if (dateTimeOffset != null)
                {
                    return Expression.Constant(dateTimeOffset.Value, typeof(DateTimeOffset));
                }

                var dateTime = parameterizedConstantValue as DateTime?;
                if (dateTime != null)
                {
                    return Expression.Constant(dateTime.Value, typeof(DateTime));
                }

                var date = parameterizedConstantValue as Date?;
                if (date != null)
                {
                    return Expression.Constant(date.Value, typeof(Date));
                }

                var timeOfDay = parameterizedConstantValue as TimeOfDay?;
                if (timeOfDay != null)
                {
                    return Expression.Constant(timeOfDay.Value, typeof(TimeOfDay));
                }
            }

            return source;
        }

        public static bool IsIQueryable(Type type)
        {
            return typeof(IQueryable).IsAssignableFrom(type);
        }

        public static bool IsDoubleOrDecimal(Type type)
        {
            return IsType<double>(type) || IsType<decimal>(type);
        }

        public static bool IsDateAndTimeRelated(Type type)
        {
            return IsType<Date>(type) ||
                IsType<DateTime>(type) ||
                IsType<DateTimeOffset>(type) ||
                IsType<TimeOfDay>(type) ||
                IsType<TimeSpan>(type);
        }

        public static bool IsDateRelated(Type type)
        {
            return IsType<Date>(type) || IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        public static bool IsTimeRelated(Type type)
        {
            return IsType<TimeOfDay>(type) || IsType<DateTime>(type) || IsType<DateTimeOffset>(type) || IsType<TimeSpan>(type);
        }

        public static bool IsDateOrOffset(Type type)
        {
            return IsType<DateTime>(type) || IsType<DateTimeOffset>(type);
        }

        public static bool IsDateTime(Type type)
        {
            return IsType<DateTime>(type);
        }

        public static bool IsTimeSpan(Type type)
        {
            return IsType<TimeSpan>(type);
        }

        public static bool IsTimeOfDay(Type type)
        {
            return IsType<TimeOfDay>(type);
        }

        public static bool IsDate(Type type)
        {
            return IsType<Date>(type);
        }

        public static bool IsInteger(Type type)
        {
            return IsType<short>(type) || IsType<int>(type) || IsType<long>(type);
        }

        public static bool IsType<T>(Type type) where T : struct
        {
            return type == typeof(T) || type == typeof(T?);
        }

        public static Expression ConvertToEnumUnderlyingType(Expression expression, Type enumType, Type enumUnderlyingType)
        {
            object parameterizedConstantValue = ExtractParameterizedConstant(expression);
            if (parameterizedConstantValue != null)
            {
                string enumStringValue = parameterizedConstantValue as string;
                if (enumStringValue != null)
                {
                    return Expression.Constant(
                        Convert.ChangeType(
                            Enum.Parse(enumType, enumStringValue), enumUnderlyingType, CultureInfo.InvariantCulture));
                }
                else
                {
                    // enum member value
                    return Expression.Constant(
                        Convert.ChangeType(
                            parameterizedConstantValue, enumUnderlyingType, CultureInfo.InvariantCulture));
                }
            }
            else if (expression.Type == enumType)
            {
                return Expression.Convert(expression, enumUnderlyingType);
            }
            else if (Nullable.GetUnderlyingType(expression.Type) == enumType)
            {
                return Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(enumUnderlyingType));
            }
            else if (expression.NodeType == ExpressionType.Constant && ((ConstantExpression)expression).Value == null)
            {
                return expression;
            }
            else
            {
                throw Error.NotSupported(SRResources.ConvertToEnumFailed, enumType, expression.Type);
            }
        }

        // Extract the constant that would have been encapsulated into LinqParameterContainer if this
        // expression represents it else return null.
        public static object ExtractParameterizedConstant(Expression expression)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memberAccess = expression as MemberExpression;
                Contract.Assert(memberAccess != null);

                PropertyInfo propertyInfo = memberAccess.Member as PropertyInfo;
                if (propertyInfo != null && propertyInfo.GetMethod.IsStatic)
                {
                    return propertyInfo.GetValue(new object());
                }

                if (memberAccess.Expression.NodeType == ExpressionType.Constant)
                {
                    ConstantExpression constant = memberAccess.Expression as ConstantExpression;
                    Contract.Assert(constant != null);
                    Contract.Assert(constant.Value != null);
                    LinqParameterContainer value = constant.Value as LinqParameterContainer;
                    Contract.Assert(value != null, "Constants are already embedded into LinqParameterContainer");

                    return value.Property;
                }
            }

            return null;
        }

        public static Expression DateTimeOffsetToDateTime(Expression expression, TimeZoneInfo timeZoneInfo, ODataQuerySettings settings)
        {
            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null)
            {
                if (Nullable.GetUnderlyingType(unaryExpression.Type) == unaryExpression.Operand.Type)
                {
                    // this is a cast from T to Nullable<T> which is redundant.
                    expression = unaryExpression.Operand;
                }
            }
            var parameterizedConstantValue = ExtractParameterizedConstant(expression);
            var dto = parameterizedConstantValue as DateTimeOffset?;
            if (dto != null)
            {
	            if (settings.EnableConstantParameterization)
	            {
		            return LinqParameterContainer.Parameterize(typeof(DateTime), EdmPrimitiveHelper.ConvertPrimitiveValue(dto.Value, typeof(DateTime), timeZoneInfo));
	            }
	            else
	            {
		            return Expression.Constant(EdmPrimitiveHelper.ConvertPrimitiveValue(dto.Value, typeof(DateTime), timeZoneInfo), typeof(DateTime));
	            }
            }
            return expression;
        }

        public static bool IsNullable(Type t)
        {
            if (!t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return true;
            }

            return false;
        }

        public static Type ToNullable(Type t)
        {
            if (IsNullable(t))
            {
                return t;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(t);
            }
        }

        public static Expression ToNullable(Expression expression)
        {
            if (!IsNullable(expression.Type))
            {
                return Expression.Convert(expression, ToNullable(expression.Type));
            }

            return expression;
        }

        public static Expression ConvertNull(Expression expression, Type type)
        {
            ConstantExpression constantExpression = expression as ConstantExpression;
            if (constantExpression != null && constantExpression.Value == null)
            {
                return Expression.Constant(null, type);
            }
            else
            {
                return expression;
            }
        }

        public static Expression BindCastToStringType(Expression source)
        {
            Expression sourceValue;

            if (source.Type.IsGenericType && source.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (TypeHelper.IsEnum(source.Type))
                {
                    // Entity Framework doesn't have ToString method for enum types.
                    // Convert enum types to their underlying numeric types.
                    sourceValue = Expression.Convert(
                        Expression.Property(source, "Value"),
                        Enum.GetUnderlyingType(TypeHelper.GetUnderlyingTypeOrSelf(source.Type)));
                }
                else
                {
                    // Entity Framework has ToString method for numeric types.
                    sourceValue = Expression.Property(source, "Value");
                }

                // Entity Framework doesn't have ToString method for nullable numeric types.
                // Call ToString method on non-nullable numeric types.
                return Expression.Condition(
                    Expression.Property(source, "HasValue"),
                    Expression.Call(sourceValue, "ToString", typeArguments: null, arguments: null),
                    Expression.Constant(null, typeof(string)));
            }
            else
            {
                sourceValue = TypeHelper.IsEnum(source.Type) ?
                    Expression.Convert(source, Enum.GetUnderlyingType(source.Type)) :
                    source;
                return Expression.Call(sourceValue, "ToString", typeArguments: null, arguments: null);
            }
        }
    }
}
