//-----------------------------------------------------------------------------
// <copyright file="ExpressionBinderBase.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Query.Container;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Query.Expressions
{
    /// <summary>
    /// The base class for all expression binders.
    /// </summary>
    public abstract class ExpressionBinderBase
    {
        internal static readonly MethodInfo StringCompareMethodInfo = typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) });
        internal static readonly MethodInfo GuidCompareMethodInfo = typeof(Guid).GetMethod("CompareTo", new[] { typeof(Guid) });
        internal static readonly string DictionaryStringObjectIndexerName = typeof(Dictionary<string, object>).GetDefaultMembers()[0].Name;

        internal static readonly Expression NullConstant = Expression.Constant(null);
        internal static readonly Expression FalseConstant = Expression.Constant(false);
        internal static readonly Expression TrueConstant = Expression.Constant(true);
        internal static readonly Expression ZeroConstant = Expression.Constant(0);

        // .NET 6 adds a new overload: TryParse<TEnum>(ReadOnlySpan<Char>, TEnum)
        // Now, with `TryParse<TEnum>(String, TEnum)`, there will have two versions with two parameters
        // So, the previous Single() will throw exception.
        internal static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethod("TryParse",
            new[]
            {
                typeof(string),
                Type.MakeGenericMethodParameter(0).MakeByRefType()
            });

        internal static readonly Dictionary<BinaryOperatorKind, ExpressionType> BinaryOperatorMapping = new Dictionary<BinaryOperatorKind, ExpressionType>
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

        internal IEdmModel Model { get; set; }

        internal ODataQuerySettings QuerySettings { get; set; }

        internal IAssemblyResolver InternalAssembliesResolver { get; set; }

        internal bool HasInstancePropertyContainer;

        /// <summary>
        /// Base query used for the binder.
        /// </summary>
        internal IQueryable BaseQuery;

        /// <summary>
        /// Flattened list of properties from base query, for case when binder is applied for aggregated query.
        /// </summary>
        internal IDictionary<string, Expression> FlattenedPropertyContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBinderBase"/> class.
        /// </summary>
        /// <param name="requestContainer">The request container.</param>
        protected ExpressionBinderBase(IServiceProvider requestContainer)
        {
            Contract.Assert(requestContainer != null);

            QuerySettings = requestContainer.GetRequiredService<ODataQuerySettings>();
            Model = requestContainer.GetRequiredService<IEdmModel>();

            // The IWebApiAssembliesResolver service is internal and can only be injected by WebApi.
            // This code path may be used in the cases when the service container available
            // but may not contain an instance of IWebApiAssembliesResolver.
            IAssemblyResolver injectedResolver = requestContainer.GetService<IAssemblyResolver>();
            InternalAssembliesResolver = (injectedResolver != null) ? injectedResolver : AssemblyResolverHelper.Default;
        }

        internal ExpressionBinderBase(IEdmModel model, IAssemblyResolver assembliesResolver, ODataQuerySettings querySettings)
            : this(model, querySettings)
        {
            InternalAssembliesResolver = assembliesResolver;
        }

        internal ExpressionBinderBase(IEdmModel model, ODataQuerySettings querySettings)
        {
            Contract.Assert(model != null);
            Contract.Assert(querySettings != null);

            QuerySettings = querySettings;
            Model = model;
        }

        /// <summary>
        /// Binds a <see cref="QueryNode"/> to create a LINQ <see cref="Expression"/> that represents the semantics
        /// of the <see cref="QueryNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
        public abstract Expression Bind(QueryNode node);

        /// <summary>
        /// Gets $it parameter
        /// </summary>
        /// <returns></returns>
        protected abstract ParameterExpression Parameter { get; }

        /// <summary>
        /// Binds a <see cref="ConstantNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="ConstantNode"/>.
        /// </summary>
        /// <param name="constantNode">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindConstantNode(ConstantNode constantNode)
        {
            Contract.Assert(constantNode != null);

            // no need to parameterize null's as there cannot be multiple values for null.
            if (constantNode.Value == null)
            {
                return NullConstant;
            }

            object value = constantNode.Value;
            Type constantType = RetrieveClrTypeForConstant(constantNode.TypeReference, ref value);

            if (QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(constantType, value);
            }
            else
            {
                return Expression.Constant(value, constantType);
            }
        }

        /// <summary>
        /// Binds a <see cref="SingleValueFunctionCallNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="SingleValueFunctionCallNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
                        Justification = "These are simple binding functions and cannot be split up.")]
        public virtual Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            switch (node.Name)
            {
                case ClrCanonicalFunctions.StartswithFunctionName:
                    return BindStartsWith(node);

                case ClrCanonicalFunctions.EndswithFunctionName:
                    return BindEndsWith(node);

                case ClrCanonicalFunctions.ContainsFunctionName:
                    return BindContains(node);

                case ClrCanonicalFunctions.SubstringFunctionName:
                    return BindSubstring(node);

                case ClrCanonicalFunctions.LengthFunctionName:
                    return BindLength(node);

                case ClrCanonicalFunctions.IndexofFunctionName:
                    return BindIndexOf(node);

                case ClrCanonicalFunctions.TolowerFunctionName:
                    return BindToLower(node);

                case ClrCanonicalFunctions.ToupperFunctionName:
                    return BindToUpper(node);

                case ClrCanonicalFunctions.TrimFunctionName:
                    return BindTrim(node);

                case ClrCanonicalFunctions.ConcatFunctionName:
                    return BindConcat(node);

                case ClrCanonicalFunctions.YearFunctionName:
                case ClrCanonicalFunctions.MonthFunctionName:
                case ClrCanonicalFunctions.DayFunctionName:
                    return BindDateRelatedProperty(node); // Date & DateTime & DateTimeOffset

                case ClrCanonicalFunctions.HourFunctionName:
                case ClrCanonicalFunctions.MinuteFunctionName:
                case ClrCanonicalFunctions.SecondFunctionName:
                    return BindTimeRelatedProperty(node); // TimeOfDay & DateTime & DateTimeOffset

                case ClrCanonicalFunctions.FractionalSecondsFunctionName:
                    return BindFractionalSeconds(node);

                case ClrCanonicalFunctions.RoundFunctionName:
                    return BindRound(node);

                case ClrCanonicalFunctions.FloorFunctionName:
                    return BindFloor(node);

                case ClrCanonicalFunctions.CeilingFunctionName:
                    return BindCeiling(node);

                case ClrCanonicalFunctions.CastFunctionName:
                    return BindCastSingleValue(node);

                case ClrCanonicalFunctions.IsofFunctionName:
                    return BindIsOf(node);

                case ClrCanonicalFunctions.DateFunctionName:
                    return BindDate(node);

                case ClrCanonicalFunctions.TimeFunctionName:
                    return BindTime(node);

                case ClrCanonicalFunctions.NowFunctionName:
                    return BindNow(node);

                default:
                    // Get Expression of custom binded method.
                    Expression expression = BindCustomMethodExpressionOrNull(node);
                    if (expression != null)
                    {
                        return expression;
                    }

                    throw new NotImplementedException(Error.Format(SRResources.ODataFunctionNotSupported, node.Name));
            }
        }

        /// <summary>
        /// Binds a <see cref="CollectionConstantNode"/> to create a LINQ <see cref="Expression"/> that
        /// represents the semantics of the <see cref="CollectionConstantNode"/>.
        /// </summary>
        /// <param name="node">The node to bind.</param>
        /// <returns>The LINQ <see cref="Expression"/> created.</returns>
        public virtual Expression BindCollectionConstantNode(CollectionConstantNode node)
        {
            if (node == null)
            {
                throw Error.ArgumentNull(nameof(node));
            }

            // It's fine if the collection is empty; the returned value will be an empty list.
            ConstantNode firstNode = node.Collection.FirstOrDefault();
            object value = null;
            if (firstNode != null)
            {
                value = firstNode.Value;
            }

            Type constantType = RetrieveClrTypeForConstant(node.ItemType, ref value);
            Type nullableConstantType = node.ItemType.IsNullable && constantType.IsValueType && Nullable.GetUnderlyingType(constantType) == null
                ? typeof(Nullable<>).MakeGenericType(constantType)
                : constantType;
            Type listType = typeof(List<>).MakeGenericType(nullableConstantType);
            IList castedList = Activator.CreateInstance(listType) as IList;

            // Getting a LINQ expression to dynamically cast each item in the Collection during runtime is tricky,
            // so using a foreach loop and doing an implicit cast from object to the CLR type of ItemType.
            foreach (ConstantNode item in node.Collection)
            {
                object member;
                if (item.Value == null)
                {
                    member = null;
                }
                else if (constantType.IsEnum)
                {
                    member = EnumDeserializationHelpers.ConvertEnumValue(item.Value, constantType);
                }
                else
                {
                    member = item.Value;
                }

                castedList.Add(member);
            }

            if (QuerySettings.EnableConstantParameterization)
            {
                return LinqParameterContainer.Parameterize(listType, castedList);
            }

            return Expression.Constant(castedList, listType);
        }

        private Expression BindIsOf(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.IsofFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // Edm.Boolean isof(type)  or
            // Edm.Boolean isof(expression,type)
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? this.Parameter : arguments[0];
            if (source == NullConstant)
            {
                return FalseConstant;
            }

            string typeName = (string)((ConstantNode)node.Parameters.Last()).Value;

            IEdmType edmType = Model.FindType(typeName);
            Type clrType = null;
            if (edmType != null)
            {
                // bool nullable = source.Type.IsNullable();
                IEdmTypeReference edmTypeReference = edmType.ToEdmTypeReference(false);
                clrType = Model.GetClrType(edmTypeReference);
            }

            if (clrType == null)
            {
                return FalseConstant;
            }

            bool isSourcePrimitiveOrEnum = source.Type.GetEdmPrimitiveType() != null ||
                                           TypeHelper.IsEnum(source.Type);

            bool isTargetPrimitiveOrEnum = clrType.GetEdmPrimitiveType() != null ||
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

        private Expression BindCeiling(SingleValueFunctionCallNode node)
        {
            Contract.Assert("ceiling" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo ceiling = ExpressionBinderHelper.IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.CeilingOfDouble
                : ClrCanonicalFunctions.CeilingOfDecimal;
            return ExpressionBinderHelper.MakeFunctionCall(ceiling, QuerySettings, arguments);
        }

        private Expression BindFloor(SingleValueFunctionCallNode node)
        {
            Contract.Assert("floor" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo floor = ExpressionBinderHelper.IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.FloorOfDouble
                : ClrCanonicalFunctions.FloorOfDecimal;
            return ExpressionBinderHelper.MakeFunctionCall(floor, QuerySettings, arguments);
        }

        private Expression BindRound(SingleValueFunctionCallNode node)
        {
            Contract.Assert("round" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDoubleOrDecimal(arguments[0].Type));

            MethodInfo round = ExpressionBinderHelper.IsType<double>(arguments[0].Type)
                ? ClrCanonicalFunctions.RoundOfDouble
                : ClrCanonicalFunctions.RoundOfDecimal;
            return ExpressionBinderHelper.MakeFunctionCall(round, QuerySettings, arguments);
        }

        private Expression BindDate(SingleValueFunctionCallNode node)
        {
            Contract.Assert("date" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDateOrOffset(arguments[0].Type));

            // EF doesn't support new Date(int, int, int), also doesn't support other property access, for example DateTime.Date.
            // Therefore, we just return the source (DateTime or DateTimeOffset).
            return arguments[0];
        }

        private Expression BindNow(SingleValueFunctionCallNode node)
        {
            Contract.Assert("now" == node.Name);

            // Function Now() does not take any arguments.
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 0);

            return Expression.Property(null, typeof(DateTimeOffset), "UtcNow");
        }

        private Expression BindTime(SingleValueFunctionCallNode node)
        {
            Contract.Assert("time" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDateOrOffset(arguments[0].Type));

            // EF doesn't support new TimeOfDay(int, int, int, int), also doesn't support other property access, for example DateTimeOffset.DateTime.
            // Therefore, we just return the source (DateTime or DateTimeOffset).
            return arguments[0];
        }

        private Expression BindFractionalSeconds(SingleValueFunctionCallNode node)
        {
            Contract.Assert("fractionalseconds" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
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
            Expression milliSecond = ExpressionBinderHelper.MakePropertyAccess(property, parameter, QuerySettings);
            Expression decimalMilliSecond = Expression.Convert(milliSecond, typeof(decimal));
            Expression fractionalSeconds = Expression.Divide(decimalMilliSecond, Expression.Constant(1000m, typeof(decimal)));

            return ExpressionBinderHelper.CreateFunctionCallWithNullPropagation(fractionalSeconds, arguments, QuerySettings);
        }

        private Expression BindDateRelatedProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && ExpressionBinderHelper.IsDateRelated(arguments[0].Type));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (ExpressionBinderHelper.IsDate(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.DateProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.DateProperties[node.Name];
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

            return ExpressionBinderHelper.MakeFunctionCall(property, QuerySettings, parameter);
        }

        private Expression BindTimeRelatedProperty(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 && (ExpressionBinderHelper.IsTimeRelated(arguments[0].Type)));

            // We should support DateTime & DateTimeOffset even though DateTime is not part of OData v4 Spec.
            Expression parameter = arguments[0];

            PropertyInfo property;
            if (ExpressionBinderHelper.IsTimeOfDay(parameter.Type))
            {
                Contract.Assert(ClrCanonicalFunctions.TimeOfDayProperties.ContainsKey(node.Name));
                property = ClrCanonicalFunctions.TimeOfDayProperties[node.Name];
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

            return ExpressionBinderHelper.MakeFunctionCall(property, QuerySettings, parameter);
        }

        private Expression BindConcat(SingleValueFunctionCallNode node)
        {
            Contract.Assert("concat" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Concat, QuerySettings, arguments);
        }

        private Expression BindTrim(SingleValueFunctionCallNode node)
        {
            Contract.Assert("trim" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Trim, QuerySettings, arguments);
        }

        private Expression BindToUpper(SingleValueFunctionCallNode node)
        {
            Contract.Assert("toupper" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.ToUpper, QuerySettings, arguments);
        }

        private Expression BindToLower(SingleValueFunctionCallNode node)
        {
            Contract.Assert("tolower" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.ToLower, QuerySettings, arguments);
        }

        private Expression BindIndexOf(SingleValueFunctionCallNode node)
        {
            Contract.Assert("indexof" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.IndexOf, QuerySettings, arguments);
        }

        private Expression BindSubstring(SingleValueFunctionCallNode node)
        {
            Contract.Assert("substring" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            if (arguments[0].Type != typeof(string))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, node.Name));
            }

            Expression functionCall;
            if (arguments.Length == 2)
            {
                Contract.Assert(ExpressionBinderHelper.IsInteger(arguments[1].Type));

                // When null propagation is allowed, we use a safe version of String.Substring(int).
                // But for providers that would not recognize custom expressions like this, we map
                // directly to String.Substring(int)
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStartNoThrow, QuerySettings, arguments);
                }
                else
                {
                    functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStart, QuerySettings, arguments);
                }
            }
            else
            {
                // arguments.Length == 3 implies String.Substring(int, int)
                Contract.Assert(arguments.Length == 3 && ExpressionBinderHelper.IsInteger(arguments[1].Type) && ExpressionBinderHelper.IsInteger(arguments[2].Type));

                // When null propagation is allowed, we use a safe version of String.Substring(int, int).
                // But for providers that would not recognize custom expressions like this, we map
                // directly to String.Substring(int, int)
                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True)
                {
                    // Safe function is static and takes string "this" as first argument
                    functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLengthNoThrow, QuerySettings, arguments);
                }
                else
                {
                    functionCall = ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.SubstringStartAndLength, QuerySettings, arguments);
                }
            }

            return functionCall;
        }

        private Expression BindLength(SingleValueFunctionCallNode node)
        {
            Contract.Assert("length" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 1 && arguments[0].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Length, QuerySettings, arguments);
        }

        private Expression BindContains(SingleValueFunctionCallNode node)
        {
            Contract.Assert("contains" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.Contains, QuerySettings, arguments[0], arguments[1]);
        }

        private Expression BindStartsWith(SingleValueFunctionCallNode node)
        {
            Contract.Assert("startswith" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.StartsWith, QuerySettings, arguments);
        }

        private Expression BindEndsWith(SingleValueFunctionCallNode node)
        {
            Contract.Assert("endswith" == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            ValidateAllStringArguments(node.Name, arguments);

            Contract.Assert(arguments.Length == 2 && arguments[0].Type == typeof(string) && arguments[1].Type == typeof(string));

            return ExpressionBinderHelper.MakeFunctionCall(ClrCanonicalFunctions.EndsWith, QuerySettings, arguments);
        }

        private Expression BindCustomMethodExpressionOrNull(SingleValueFunctionCallNode node)
        {
            Expression[] arguments = BindArguments(node.Parameters);
            IEnumerable<Type> methodArgumentsType = arguments.Select(argument => argument.Type);

            // Search for custom method info that are binded to the node name
            MethodInfo methodInfo;
            if (UriFunctionsBinder.TryGetMethodInfo(node.Name, methodArgumentsType, out methodInfo))
            {
                return ExpressionBinderHelper.MakeFunctionCall(methodInfo, QuerySettings, arguments);
            }

            return null;
        }

        private Expression BindCastSingleValue(SingleValueFunctionCallNode node)
        {
            Contract.Assert(ClrCanonicalFunctions.CastFunctionName == node.Name);

            Expression[] arguments = BindArguments(node.Parameters);
            Contract.Assert(arguments.Length == 1 || arguments.Length == 2);

            Expression source = arguments.Length == 1 ? this.Parameter : arguments[0];
            string targetTypeName = (string)((ConstantNode)node.Parameters.Last()).Value;
            IEdmType targetEdmType = Model.FindType(targetTypeName);
            Type targetClrType = null;

            if (targetEdmType != null)
            {
                IEdmTypeReference targetEdmTypeReference = targetEdmType.ToEdmTypeReference(false);
                targetClrType = Model.GetClrType(targetEdmTypeReference);

                if (source != NullConstant)
                {
                    if (source.Type == targetClrType)
                    {
                        return source;
                    }

                    if ((!targetEdmTypeReference.IsPrimitive() && !targetEdmTypeReference.IsEnum()) ||
                        (source.Type.GetEdmPrimitiveType() == null && !TypeHelper.IsEnum(source.Type)))
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
                return BindCastToEnumType(source.Type, targetClrType, node.Parameters.First(), arguments.Length);
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

        private static void ValidateAllStringArguments(string functionName, Expression[] arguments)
        {
            if (arguments.Any(arg => arg.Type != typeof(string)))
            {
                throw new ODataException(Error.Format(SRResources.FunctionNotSupportedOnEnum, functionName));
            }
        }

        /// <summary>
        /// Recognize $it.Source where $it is FlatteningWrapper
        /// Using that do avoid wrapping it redundant into Null propagation 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private bool IsFlatteningSource(Expression source)
        {
            var member = source as MemberExpression;
            return member != null
                && this.Parameter.Type.IsGenericType
                && this.Parameter.Type.GetGenericTypeDefinition() == typeof(FlatteningWrapper<>)
                && member.Expression == this.Parameter;
        }

        private static MethodCallExpression SkipFilters(MethodCallExpression expression)
        {
            while (expression.Method.Name == "Where")
            {
                expression = expression.Arguments.FirstOrDefault() as MethodCallExpression;
            }

            return expression;
        }

        private static void CollectContainerAssignments(Expression source, MethodCallExpression expression, Dictionary<string, Expression> result)
        {
            CollectAssigments(result, Expression.Property(source, "GroupByContainer"), ExtractContainerExpression(expression.Arguments.FirstOrDefault() as MethodCallExpression, "GroupByContainer"));
            CollectAssigments(result, Expression.Property(source, "Container"), ExtractContainerExpression(expression, "Container"));
        }

        private static void CollectAssigments(IDictionary<string, Expression> flattenPropertyContainer, Expression source, MemberInitExpression expression, string prefix = null)
        {
            if (expression == null)
            {
                return;
            }

            string nameToAdd = null;
            Type resultType = null;
            MemberInitExpression nextExpression = null;
            Expression nestedExpression = null;
            foreach (var expr in expression.Bindings.OfType<MemberAssignment>())
            {
                var initExpr = expr.Expression as MemberInitExpression;
                if (initExpr != null && expr.Member.Name == "Next")
                {
                    nextExpression = initExpr;
                }
                else if (expr.Member.Name == "Name")
                {
                    nameToAdd = (expr.Expression as ConstantExpression).Value as string;
                }
                else if (expr.Member.Name == "Value" || expr.Member.Name == "NestedValue")
                {
                    resultType = expr.Expression.Type;
                    if (resultType == typeof(object) && expr.Expression.NodeType == ExpressionType.Convert)
                    {
                        resultType = ((UnaryExpression)expr.Expression).Operand.Type;
                    }

                    if (typeof(GroupByWrapper).IsAssignableFrom(resultType))
                    {
                        nestedExpression = expr.Expression;
                    }
                }
            }

            if (prefix != null)
            {
                nameToAdd = prefix + "\\" + nameToAdd;
            }

            if (typeof(GroupByWrapper).IsAssignableFrom(resultType))
            {
                flattenPropertyContainer.Add(nameToAdd, Expression.Property(source, "NestedValue"));
            }
            else
            {
                flattenPropertyContainer.Add(nameToAdd, Expression.Convert(Expression.Property(source, "Value"), resultType));
            }

            if (nextExpression != null)
            {
                CollectAssigments(flattenPropertyContainer, Expression.Property(source, "Next"), nextExpression, prefix);
            }

            if (nestedExpression != null)
            {
                var nestedAccessor = ((nestedExpression as MemberInitExpression).Bindings.First() as MemberAssignment).Expression as MemberInitExpression;
                var newSource = Expression.Property(Expression.Property(source, "NestedValue"), "GroupByContainer");
                CollectAssigments(flattenPropertyContainer, newSource, nestedAccessor, nameToAdd);
            }
        }

        private static MemberInitExpression ExtractContainerExpression(MethodCallExpression expression, string containerName)
        {
            if (expression == null || expression.Arguments.Count < 2)
            {
                return null;
            }

            var memberInitExpression = ((expression.Arguments[1] as UnaryExpression).Operand as LambdaExpression).Body as MemberInitExpression;
            if (memberInitExpression != null)
            {
                var containerAssigment = memberInitExpression.Bindings.FirstOrDefault(m => m.Member.Name == containerName) as MemberAssignment;
                if (containerAssigment != null)
                {
                    return containerAssigment.Expression as MemberInitExpression;
                }
            }
            return null;
        }

        /// <summary>
        /// Bind function arguments
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        protected Expression[] BindArguments(IEnumerable<QueryNode> nodes)
        {
            return nodes.OfType<SingleValueNode>().Select(n => Bind(n)).ToArray();
        }

        /// <summary>
        /// Gets property for dynamic properties dictionary.
        /// </summary>
        /// <param name="openNode"></param>
        /// <returns>Returns CLR property for dynamic properties container.</returns>
        protected PropertyInfo GetDynamicPropertyContainer(SingleValueOpenPropertyAccessNode openNode)
        {
            if (openNode == null)
            {
                throw Error.ArgumentNull(nameof(openNode));
            }

            IEdmStructuredType edmStructuredType;
            IEdmTypeReference edmTypeReference = openNode.Source.TypeReference;
            if (edmTypeReference.IsEntity())
            {
                edmStructuredType = edmTypeReference.AsEntity().EntityDefinition();
            }
            else if (edmTypeReference.IsComplex())
            {
                edmStructuredType = edmTypeReference.AsComplex().ComplexDefinition();
            }
            else
            {
                throw Error.NotSupported(SRResources.QueryNodeBindingNotSupported, openNode.Kind, typeof(ExpressionBinderBase).Name);
            }

            return Model.GetDynamicPropertyDictionary(edmStructuredType);
        }

        /// <summary>
        /// Analyze previous query and extract grouped properties.
        /// </summary>
        /// <param name="source"></param>
        protected void EnsureFlattenedPropertyContainer(ParameterExpression source)
        {
            if (this.BaseQuery != null)
            {
                this.HasInstancePropertyContainer = this.BaseQuery.ElementType.IsGenericType
                    && this.BaseQuery.ElementType.GetGenericTypeDefinition() == typeof(ComputeWrapper<>);

                this.FlattenedPropertyContainer = this.FlattenedPropertyContainer ?? GetFlattenedProperties(source);
            }
        }

        /// <summary>
        /// Gets expression for property from previously aggregated query
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <returns>Returns null if no aggregations were used so far</returns>
        protected Expression GetFlattenedPropertyExpression(string propertyPath)
        {
            if (FlattenedPropertyContainer == null)
            {
                return null;
            }

            Expression expression;
            if (FlattenedPropertyContainer.TryGetValue(propertyPath, out expression))
            {
                return expression;
            }

            if (this.HasInstancePropertyContainer)
            {
                return null;
            }

            throw new ODataException(Error.Format(SRResources.PropertyOrPathWasRemovedFromContext, propertyPath));
        }

        internal string GetFullPropertyPath(SingleValueNode node)
        {
            string path = null;
            SingleValueNode parent = null;
            switch (node.Kind)
            {
                case QueryNodeKind.SingleComplexNode:
                    var complexNode = (SingleComplexNode)node;
                    path = complexNode.Property.Name;
                    parent = complexNode.Source;
                    break;
                case QueryNodeKind.SingleValuePropertyAccess:
                    var propertyNode = ((SingleValuePropertyAccessNode)node);
                    path = propertyNode.Property.Name;
                    parent = propertyNode.Source;
                    break;
                case QueryNodeKind.SingleNavigationNode:
                    var navNode = ((SingleNavigationNode)node);
                    path = navNode.NavigationProperty.Name;
                    parent = navNode.Source;
                    break;
            }

            if (parent != null)
            {
                var parentPath = GetFullPropertyPath(parent);
                if (parentPath != null)
                {
                    path = parentPath + "\\" + path;
                }
            }

            return path;
        }

        internal Expression CreatePropertyAccessExpression(Expression source, IEdmProperty property, string propertyPath = null)
        {
            string propertyName = Model.GetClrPropertyName(property);
            propertyPath = propertyPath ?? propertyName;
            if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type) &&
                source != this.Parameter &&
                !IsFlatteningSource(source))
            {
                Expression cleanSource = ExpressionBinderHelper.RemoveInnerNullPropagation(source, QuerySettings);
                Expression propertyAccessExpression = null;
                propertyAccessExpression = GetFlattenedPropertyExpression(propertyPath) ?? Expression.Property(cleanSource, propertyName);

                // source.property => source == null ? null : [CastToNullable]RemoveInnerNullPropagation(source).property
                // Notice that we are checking if source is null already. so we can safely remove any null checks when doing source.Property

                Expression ifFalse = ExpressionBinderHelper.ToNullable(ConvertNonStandardPrimitives(propertyAccessExpression));
                return
                    Expression.Condition(
                        test: Expression.Equal(source, NullConstant),
                        ifTrue: Expression.Constant(null, ifFalse.Type),
                        ifFalse: ifFalse);
            }
            else
            {
                return GetFlattenedPropertyExpression(propertyPath)
                    ?? ConvertNonStandardPrimitives(GetPropertyExpression(source, (this.HasInstancePropertyContainer && !propertyPath.Contains("\\", StringComparison.Ordinal) ? "Instance\\" : String.Empty) + propertyName));
            }
        }

        internal static Expression GetPropertyExpression(Expression source, string propertyPath)
        {
            string[] propertyNameParts = propertyPath.Split('\\');
            Expression propertyValue = source;
            foreach (var propertyName in propertyNameParts)
            {
                propertyValue = Expression.Property(propertyValue, propertyName);
            }
            return propertyValue;
        }

        // If the expression is of non-standard edm primitive type (like uint), convert the expression to its standard edm type.
        // Also, note that only expressions generated for ushort, uint and ulong can be understood by linq2sql and EF.
        // The rest (char, char[], Binary) would cause issues with linq2sql and EF.
        internal Expression ConvertNonStandardPrimitives(Expression source)
        {
            bool isNonstandardEdmPrimitive;
            Type conversionType = source.Type.IsNonstandardEdmPrimitive(out isNonstandardEdmPrimitive);

            if (isNonstandardEdmPrimitive)
            {
                Type sourceType = TypeHelper.GetUnderlyingTypeOrSelf(source.Type);

                Contract.Assert(sourceType != conversionType);

                Expression convertedExpression = null;

                if (TypeHelper.IsEnum(sourceType))
                {
                    // we handle enum conversions ourselves
                    convertedExpression = source;
                }
                else
                {
                    switch (Type.GetTypeCode(sourceType))
                    {
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            convertedExpression = Expression.Convert(ExpressionBinderHelper.ExtractValueFromNullableExpression(source), conversionType);
                            break;

                        case TypeCode.Char:
                            convertedExpression = Expression.Call(ExpressionBinderHelper.ExtractValueFromNullableExpression(source), "ToString", typeArguments: null, arguments: null);
                            break;

                        case TypeCode.DateTime:
                            convertedExpression = source;
                            break;

                        case TypeCode.Object:
                            if (sourceType == typeof(char[]))
                            {
                                convertedExpression = Expression.New(typeof(string).GetConstructor(new[] { typeof(char[]) }), source);
                            }
                            else if (sourceType == typeof(XElement))
                            {
                                convertedExpression = Expression.Call(source, "ToString", typeArguments: null, arguments: null);
                            }
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
                            else if (sourceType == typeof(Binary))
                            {
                                convertedExpression = Expression.Call(source, "ToArray", typeArguments: null, arguments: null);
                            }
#endif
                            break;

                        default:
                            Contract.Assert(false, Error.Format("missing non-standard type support for {0}", sourceType.Name));
                            break;
                    }
                }

                if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True && ExpressionBinderHelper.IsNullable(source.Type))
                {
                    // source == null ? null : source
                    return Expression.Condition(
                        ExpressionBinderHelper.CheckForNull(source),
                        ifTrue: Expression.Constant(null, ExpressionBinderHelper.ToNullable(convertedExpression.Type)),
                        ifFalse: ExpressionBinderHelper.ToNullable(convertedExpression));
                }
                else
                {
                    return convertedExpression;
                }
            }

            return source;
        }

        internal Expression CreateConvertExpression(ConvertNode convertNode, Expression source)
        {
            Type conversionType = Model.GetClrType(convertNode.TypeReference, InternalAssembliesResolver);

            if (conversionType == typeof(bool?) && source.Type == typeof(bool))
            {
                // we handle null propagation ourselves. So, if converting from bool to Nullable<bool> ignore.
                return source;
            }
            else if (conversionType == typeof(Date?) &&
                (source.Type == typeof(DateTimeOffset?) || source.Type == typeof(DateTime?)))
            {
                return source;
            }
            if ((conversionType == typeof(TimeOfDay?) && source.Type == typeof(TimeOfDay)) ||
                ((conversionType == typeof(Date?) && source.Type == typeof(Date))))
            {
                return source;
            }
            else if (conversionType == typeof(TimeOfDay?) &&
                (source.Type == typeof(DateTimeOffset?) || source.Type == typeof(DateTime?) || source.Type == typeof(TimeSpan?)))
            {
                return source;
            }
            else if (ExpressionBinderHelper.IsDateAndTimeRelated(conversionType) && ExpressionBinderHelper.IsDateAndTimeRelated(source.Type))
            {
                return source;
            }
            else if (source == NullConstant)
            {
                return source;
            }
            else
            {
                if (TypeHelper.IsEnum(source.Type))
                {
                    // we handle enum conversions ourselves
                    return source;
                }
                else
                {
                    // if a cast is from Nullable<T> to Non-Nullable<T> we need to check if source is null
                    if (QuerySettings.HandleNullPropagation == HandleNullPropagationOption.True
                        && ExpressionBinderHelper.IsNullable(source.Type) && !ExpressionBinderHelper.IsNullable(conversionType))
                    {
                        // source == null ? null : source.Value
                        return
                            Expression.Condition(
                            test: ExpressionBinderHelper.CheckForNull(source),
                            ifTrue: Expression.Constant(null, ExpressionBinderHelper.ToNullable(conversionType)),
                            ifFalse: Expression.Convert(ExpressionBinderHelper.ExtractValueFromNullableExpression(source), ExpressionBinderHelper.ToNullable(conversionType)));
                    }
                    else
                    {
                        return Expression.Convert(source, conversionType);
                    }
                }
            }
        }

        internal IDictionary<string, Expression> GetFlattenedProperties(ParameterExpression source)
        {
            if (this.BaseQuery == null)
            {
                return null;
            }

            if (!typeof(GroupByWrapper).IsAssignableFrom(BaseQuery.ElementType))
            {
                return null;
            }

            var expression = BaseQuery.Expression as MethodCallExpression;
            if (expression == null)
            {
                return null;
            }

            // After $apply we could have other clauses, like $filter, $orderby etc.
            // Skip of filter expressions
            expression = SkipFilters(expression);

            if (expression == null)
            {
                return null;
            }

            var result = new Dictionary<string, Expression>();
            CollectContainerAssignments(source, expression, result);
            if (this.HasInstancePropertyContainer)
            {
                var instanceProperty = Expression.Property(source, "Instance");
                if (typeof(DynamicTypeWrapper).IsAssignableFrom(instanceProperty.Type))
                {
                    var computeExpression = expression.Arguments.FirstOrDefault() as MethodCallExpression;
                    computeExpression = SkipFilters(computeExpression);
                    if (computeExpression != null)
                    {
                        CollectContainerAssignments(instanceProperty, computeExpression, result);
                    }
                }
            }

            return result;
        }

        internal Type RetrieveClrTypeForConstant(IEdmTypeReference edmTypeReference, ref object value)
        {
            Type constantType = Model.GetClrType(edmTypeReference, InternalAssembliesResolver);

            if (value != null && edmTypeReference != null && edmTypeReference.IsEnum())
            {
                ODataEnumValue odataEnumValue = (ODataEnumValue)value;
                string strValue = odataEnumValue.Value;
                Contract.Assert(strValue != null);

                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;

                IEdmEnumType enumType = edmTypeReference.AsEnum().EnumDefinition();
                ClrEnumMemberAnnotation memberMapAnnotation = Model.GetClrEnumMemberAnnotation(enumType);
                if (memberMapAnnotation != null)
                {
                    IEdmEnumMember enumMember = enumType.Members.FirstOrDefault(m => m.Name == strValue);
                    if (enumMember == null)
                    {
                        enumMember = enumType.Members.FirstOrDefault(m => m.Value.ToString() == strValue);
                    }

                    if (enumMember != null)
                    {
                        Enum clrMember = memberMapAnnotation.GetClrEnumMember(enumMember);
                        if (clrMember != null)
                        {
                            value = clrMember;
                        }
                        else
                        {
                            throw new ODataException(Error.Format(SRResources.CannotGetEnumClrMember, enumMember.Name));
                        }
                    }
                    else
                    {
                        value = Enum.Parse(constantType, strValue);
                    }
                }
                else
                {
                    value = Enum.Parse(constantType, strValue);
                }
            }

            if (edmTypeReference != null &&
                edmTypeReference.IsNullable &&
                (edmTypeReference.IsDate() || edmTypeReference.IsTimeOfDay()))
            {
                constantType = Nullable.GetUnderlyingType(constantType) ?? constantType;
            }

            return constantType;
        }

        internal Expression BindCastToEnumType(Type sourceType, Type targetClrType, QueryNode firstParameter, int parameterLength)
        {
            Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(targetClrType);
            ConstantNode sourceNode = firstParameter as ConstantNode;

            if (parameterLength == 1 || sourceNode == null || sourceType != typeof(string))
            {
                // We only support to cast Enumeration type from constant string now,
                // because LINQ to Entities does not recognize the method Enum.TryParse.
                return NullConstant;
            }
            else
            {
                object[] parameters = new[] { sourceNode.Value, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)EnumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (isSuccessful)
                {
                    if (QuerySettings.EnableConstantParameterization)
                    {
                        return LinqParameterContainer.Parameterize(targetClrType, parameters[1]);
                    }
                    else
                    {
                        return Expression.Constant(parameters[1], targetClrType);
                    }
                }
                else
                {
                    return NullConstant;
                }
            }
        }
    }
}
