//-----------------------------------------------------------------------------
// <copyright file="QueryBinderValidator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNetCore.OData.Query.Validator
{
    /// <summary>
    /// Represents a validator used to validate the results of translating an OData parse tree into expressions.
    /// </summary>
    internal static class QueryBinderValidator
    {
        private static readonly Type groupByWrapperInterfaceTypeOfT = typeof(IGroupByWrapper<,>);
        private static string groupByWrapperInterfaceTypeOfTName = $"{groupByWrapperInterfaceTypeOfT.Namespace}.{groupByWrapperInterfaceTypeOfT.Name.Split('`')[0]}{{TContainer,TWrapper}}";
        private static string dynamicTypeWrapperName = typeof(DynamicTypeWrapper).FullName;
        private static readonly Type flatteningWrapperInterfaceTypeOfT = typeof(IFlatteningWrapper<>);
        private static string flatteningWrapperInterfaceTypeOfTName = $"{flatteningWrapperInterfaceTypeOfT.Namespace}.{flatteningWrapperInterfaceTypeOfT.Name.Split('`')[0]}{{T}}";
        private static readonly Type computeWrapperInterfaceTypeOfT = typeof(IComputeWrapper<>);
        private static string computeWrapperInterfaceTypeOfTName = $"{computeWrapperInterfaceTypeOfT.Namespace}.{computeWrapperInterfaceTypeOfT.Name.Split('`')[0]}{{T}}";

        /// <summary>
        /// Validates that the type representing the expression returned by
        /// <see cref="IAggregationBinder.BindGroupBy(TransformationNode, QueryBinderContext)"/>
        /// implements <see cref="IGroupByWrapper{TContainer, TWrapper}"/> and derives from <see cref="DynamicTypeWrapper"/>.
        /// </summary>
        /// <param name="groupByExpressionType">The type representing the expression returned by
        /// <see cref="IAggregationBinder.BindGroupBy(TransformationNode, QueryBinderContext)"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="groupByExpressionType"/>
        /// does not implement the required interfaces or inherit from the required base class.</exception>
        public static void ValidateGroupByExpressionType(Type groupByExpressionType)
        {
            ValidateTransformationExpressionType(groupByExpressionType);
        }

        /// <summary>
        /// Validates that the type representing the expression returned by
        /// <see cref="IAggregationBinder.BindGroupBy(TransformationNode, QueryBinderContext)"/>
        /// implements <see cref="IGroupByWrapper{TContainer, TWrapper}"/> and derives from <see cref="DynamicTypeWrapper"/>.
        /// </summary>
        /// <param name="selectExpressionType">The type representing the expression returned by
        /// <see cref="IAggregationBinder.BindSelect(TransformationNode, QueryBinderContext)"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="selectExpressionType"/>
        /// does not implement the required interfaces or inherit from the required base class.</exception>
        public static void ValidateSelectExpressionType(Type selectExpressionType)
        {
            ValidateTransformationExpressionType(selectExpressionType);
        }

        /// <summary>
        /// Validates the provided <see cref="AggregationFlatteningResult"/> instance to ensure all required properties are set.
        /// </summary>
        /// <param name="flatteningResult">The <see cref="AggregationFlatteningResult"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="flatteningResult"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="flatteningResult"/> has a null or empty <see cref="AggregationFlatteningResult.FlattenedExpression"/>,
        /// <see cref="AggregationFlatteningResult.RedefinedContextParameter"/>, or <see cref="AggregationFlatteningResult.FlattenedPropertiesMapping"/>.</exception>
        public static void ValidateFlatteningResult(AggregationFlatteningResult flatteningResult)
        {
            if (flatteningResult == null)
            {
                throw Error.ArgumentNull(nameof(flatteningResult));
            }

            if (flatteningResult?.FlattenedExpression == null)
            {
                throw Error.Argument(
                    nameof(flatteningResult),
                    SRResources.PropertyMustBeSet,
                    nameof(flatteningResult.FlattenedExpression));
            }

            if (flatteningResult.RedefinedContextParameter == null)
            {
                throw Error.Argument(
                    nameof(flatteningResult),
                    SRResources.PropertyMustBeSetWhenAnotherPropertyIsSet,
                    nameof(flatteningResult.RedefinedContextParameter),
                    nameof(flatteningResult.FlattenedExpression));
            }

            if (flatteningResult.FlattenedPropertiesMapping == null || flatteningResult.FlattenedPropertiesMapping.Count == 0)
            {
                throw Error.Argument(
                    nameof(flatteningResult),
                    SRResources.PropertyMustBeSetWhenAnotherPropertyIsSet,
                    nameof(flatteningResult.FlattenedPropertiesMapping),
                    nameof(flatteningResult.FlattenedExpression));
            }
        }

        /// <summary>
        /// Validates that the provided type implements <see cref="IGroupByWrapper{TContainer, TWrapper}"/> and <see cref="IFlatteningWrapper{T}"/>, and inherits from <see cref="DynamicTypeWrapper"/>.
        /// </summary>
        /// <param name="flattenedExpressionType">The type representing the flattened expression returned by
        /// <see cref="IFlatteningBinder.FlattenReferencedProperties(TransformationNode, IQueryable, QueryBinderContext)"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="flattenedExpressionType"/>
        /// does not implement the required interfaces or inherit from the required base class.</exception>
        public static void ValidateFlattenedExpressionType(Type flattenedExpressionType)
        {
            ValidateTransformationExpressionType(flattenedExpressionType);

            // Type must implement IFlatteningWrapper<T> interface
            if (!flattenedExpressionType.ImplementsInterface(flatteningWrapperInterfaceTypeOfT))
            {
                throw Error.InvalidOperation(
                    SRResources.TypeMustImplementInterface,
                    flattenedExpressionType.FullName,
                    flatteningWrapperInterfaceTypeOfTName);
            }
        }

        /// <summary>
        /// Validates that the provided type implements <see cref="IGroupByWrapper{TContainer, TWrapper}"/> and <see cref="IComputeWrapper{T}"/>, and inherits from <see cref="DynamicTypeWrapper"/>.
        /// </summary>
        /// <param name="computeExpressionType">The type representing the flattened expression returned by
        /// <see cref="IComputeBinder.BindCompute(ComputeTransformationNode, QueryBinderContext)"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="computeExpressionType"/>
        /// does not implement the required interfaces or inherit from the required base class.</exception>
        public static void ValidateComputeExpressionType(Type computeExpressionType)
        {
            ValidateTransformationExpressionType(computeExpressionType);

            // Type must implement IComputeWrapper<T> interface
            if (!computeExpressionType.ImplementsInterface(computeWrapperInterfaceTypeOfT))
            {
                throw Error.InvalidOperation(
                    SRResources.TypeMustImplementInterface,
                    computeExpressionType.FullName,
                    computeWrapperInterfaceTypeOfTName);
            }
        }

        /// <summary>
        /// Validates that the provided type implements <see cref="IGroupByWrapper{TContainer, TWrapper}"/> and inherits from <see cref="DynamicTypeWrapper"/>.
        /// </summary>
        /// <param name="transformationExpressionType">The type to validate.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="transformationExpressionType"/>
        /// does not implement the required interface or inherit from the required base class.</exception>
        private static void ValidateTransformationExpressionType(Type transformationExpressionType)
        {
            // Type must implement IGroupByWrapper<TContainer, TWrapper> interface
            if (!transformationExpressionType.ImplementsInterface(groupByWrapperInterfaceTypeOfT))
            {
                throw Error.InvalidOperation(
                    SRResources.TypeMustImplementInterface,
                    transformationExpressionType.FullName,
                    groupByWrapperInterfaceTypeOfTName);
            }

            // Type must inherit from DynamicTypeWrapper
            if (!transformationExpressionType.IsSubclassOf(typeof(DynamicTypeWrapper)))
            {
                throw Error.InvalidOperation(
                    SRResources.TypeMustInheritFromType,
                    transformationExpressionType.FullName,
                    dynamicTypeWrapperName);
            }
        }
    }
}
