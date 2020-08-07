// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder.Annotations;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// Provides the functionalities related to the <see cref="Type"/> and Edm type.
    /// </summary>
    internal static class EdmHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edmType"></param>
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public static IEdmTypeReference ToEdmTypeReference(this IEdmType edmType, bool isNullable)
        {
            Contract.Assert(edmType != null);

            switch (edmType.TypeKind)
            {
                case EdmTypeKind.Collection:
                    return new EdmCollectionTypeReference((IEdmCollectionType)edmType);

                case EdmTypeKind.Complex:
                    return new EdmComplexTypeReference((IEdmComplexType)edmType, isNullable);

                case EdmTypeKind.Entity:
                    return new EdmEntityTypeReference((IEdmEntityType)edmType, isNullable);

                case EdmTypeKind.EntityReference:
                    return new EdmEntityReferenceTypeReference((IEdmEntityReferenceType)edmType, isNullable);

                case EdmTypeKind.Enum:
                    return new EdmEnumTypeReference((IEdmEnumType)edmType, isNullable);

                case EdmTypeKind.Primitive:
                    return EdmCoreModel.Instance.GetPrimitive(((IEdmPrimitiveType)edmType).PrimitiveKind, isNullable);

                default:
                    throw Error.NotSupported(SRResources.EdmTypeNotSupported, edmType.ToTraceString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNullable(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static ModelBoundQuerySettings GetModelBoundQuerySettings(IEdmProperty property,
            IEdmStructuredType structuredType, IEdmModel edmModel, DefaultQuerySettings defaultQuerySettings = null)
        {
            Contract.Assert(edmModel != null);

            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(structuredType, edmModel,
                defaultQuerySettings);
            if (property == null)
            {
                return querySettings;
            }
            else
            {
                ModelBoundQuerySettings propertyQuerySettings = GetModelBoundQuerySettings(property, edmModel,
                    defaultQuerySettings);
                return GetMergedPropertyQuerySettings(propertyQuerySettings,
                    querySettings);
            }
        }

        private static ModelBoundQuerySettings GetMergedPropertyQuerySettings(
            ModelBoundQuerySettings propertyQuerySettings, ModelBoundQuerySettings propertyTypeQuerySettings)
        {
            ModelBoundQuerySettings mergedQuerySettings = new ModelBoundQuerySettings(propertyQuerySettings);
            if (propertyTypeQuerySettings != null)
            {
                if (!mergedQuerySettings.PageSize.HasValue)
                {
                    mergedQuerySettings.PageSize =
                        propertyTypeQuerySettings.PageSize;
                }

                if (mergedQuerySettings.MaxTop == 0 && propertyTypeQuerySettings.MaxTop != 0)
                {
                    mergedQuerySettings.MaxTop =
                        propertyTypeQuerySettings.MaxTop;
                }

                if (!mergedQuerySettings.Countable.HasValue)
                {
                    mergedQuerySettings.Countable = propertyTypeQuerySettings.Countable;
                }

                if (mergedQuerySettings.OrderByConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultEnableOrderBy.HasValue)
                {
                    mergedQuerySettings.CopyOrderByConfigurations(propertyTypeQuerySettings.OrderByConfigurations);
                    mergedQuerySettings.DefaultEnableOrderBy = propertyTypeQuerySettings.DefaultEnableOrderBy;
                }

                if (mergedQuerySettings.FilterConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultEnableFilter.HasValue)
                {
                    mergedQuerySettings.CopyFilterConfigurations(propertyTypeQuerySettings.FilterConfigurations);
                    mergedQuerySettings.DefaultEnableFilter = propertyTypeQuerySettings.DefaultEnableFilter;
                }

                if (mergedQuerySettings.SelectConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultSelectType.HasValue)
                {
                    mergedQuerySettings.CopySelectConfigurations(propertyTypeQuerySettings.SelectConfigurations);
                    mergedQuerySettings.DefaultSelectType = propertyTypeQuerySettings.DefaultSelectType;
                }

                if (mergedQuerySettings.ExpandConfigurations.Count == 0 &&
                    !mergedQuerySettings.DefaultExpandType.HasValue)
                {
                    mergedQuerySettings.CopyExpandConfigurations(
                        propertyTypeQuerySettings.ExpandConfigurations);
                    mergedQuerySettings.DefaultExpandType = propertyTypeQuerySettings.DefaultExpandType;
                    mergedQuerySettings.DefaultMaxDepth = propertyTypeQuerySettings.DefaultMaxDepth;
                }
            }
            return mergedQuerySettings;
        }

        private static ModelBoundQuerySettings GetModelBoundQuerySettings<T>(T key, IEdmModel edmModel,
            DefaultQuerySettings defaultQuerySettings = null)
            where T : IEdmElement
        {
            Contract.Assert(edmModel != null);

            if (key == null)
            {
                return null;
            }
            else
            {
                ModelBoundQuerySettings querySettings = edmModel.GetAnnotationValue<ModelBoundQuerySettings>(key);
                if (querySettings == null)
                {
                    querySettings = new ModelBoundQuerySettings();
                    if (defaultQuerySettings != null &&
                        (!defaultQuerySettings.MaxTop.HasValue || defaultQuerySettings.MaxTop > 0))
                    {
                        querySettings.MaxTop = defaultQuerySettings.MaxTop;
                    }
                }
                return querySettings;
            }
        }

        public static IEnumerable<IEdmEntityType> GetAllDerivedEntityTypes(
            IEdmEntityType entityType, IEdmModel edmModel)
        {
            List<IEdmEntityType> derivedEntityTypes = new List<IEdmEntityType>();
            if (entityType != null)
            {
                List<IEdmStructuredType> typeList = new List<IEdmStructuredType>();
                typeList.Add(entityType);
                while (typeList.Count > 0)
                {
                    var head = typeList[0];
                    derivedEntityTypes.Add(head as IEdmEntityType);
                    var derivedTypes = edmModel.FindDirectlyDerivedTypes(head);
                    if (derivedTypes != null)
                    {
                        typeList.AddRange(derivedTypes);
                    }

                    typeList.RemoveAt(0);
                }
            }

            derivedEntityTypes.RemoveAt(0);
            return derivedEntityTypes;
        }

        private static QueryableRestrictionsAnnotation GetPropertyRestrictions(IEdmProperty edmProperty,
            IEdmModel edmModel)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(edmModel != null);

            return edmModel.GetAnnotationValue<QueryableRestrictionsAnnotation>(edmProperty);
        }

        public static bool IsAutoExpand(IEdmProperty navigationProperty,
            IEdmProperty pathProperty, IEdmStructuredType pathStructuredType, IEdmModel edmModel,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(navigationProperty, edmModel);
            if (annotation != null && annotation.Restrictions.AutoExpand)
            {
                return !annotation.Restrictions.DisableAutoExpandWhenSelectIsPresent || !isSelectPresent;
            }

            if (querySettings == null)
            {
                querySettings = GetModelBoundQuerySettings(pathProperty, pathStructuredType, edmModel);
            }

            if (querySettings != null && querySettings.IsAutomaticExpand(navigationProperty.Name))
            {
                return true;
            }

            return false;
        }

        public static bool IsAutoSelect(IEdmProperty property, IEdmProperty pathProperty,
            IEdmStructuredType pathStructuredType, IEdmModel edmModel, ModelBoundQuerySettings querySettings = null)
        {
            if (querySettings == null)
            {
                querySettings = GetModelBoundQuerySettings(pathProperty, pathStructuredType, edmModel);
            }

            if (querySettings != null && querySettings.IsAutomaticSelect(property.Name))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<IEdmNavigationProperty> GetAutoExpandNavigationProperties(
            IEdmProperty pathProperty, IEdmStructuredType pathStructuredType, IEdmModel edmModel,
            bool isSelectPresent = false, ModelBoundQuerySettings querySettings = null)
        {
            List<IEdmNavigationProperty> autoExpandNavigationProperties = new List<IEdmNavigationProperty>();
            IEdmEntityType baseEntityType = pathStructuredType as IEdmEntityType;
            if (baseEntityType != null)
            {
                List<IEdmEntityType> entityTypes = new List<IEdmEntityType>();
                entityTypes.Add(baseEntityType);
                entityTypes.AddRange(GetAllDerivedEntityTypes(baseEntityType, edmModel));
                foreach (var entityType in entityTypes)
                {
                    IEnumerable<IEdmNavigationProperty> navigationProperties = entityType == baseEntityType
                        ? entityType.NavigationProperties()
                        : entityType.DeclaredNavigationProperties();

                    if (navigationProperties != null)
                    {
                        autoExpandNavigationProperties.AddRange(
                            navigationProperties.Where(
                                navigationProperty =>
                                    IsAutoExpand(navigationProperty, pathProperty, entityType, edmModel,
                                        isSelectPresent, querySettings)));
                    }
                }
            }

            return autoExpandNavigationProperties;
        }

        public static IEnumerable<IEdmStructuralProperty> GetAutoSelectProperties(
            IEdmProperty pathProperty,
            IEdmStructuredType pathStructuredType,
            IEdmModel edmModel,
            ModelBoundQuerySettings querySettings = null)
        {
            List<IEdmStructuralProperty> autoSelectProperties = new List<IEdmStructuralProperty>();
            IEdmEntityType baseEntityType = pathStructuredType as IEdmEntityType;
            if (baseEntityType != null)
            {
                List<IEdmEntityType> entityTypes = new List<IEdmEntityType>();
                entityTypes.Add(baseEntityType);
                entityTypes.AddRange(GetAllDerivedEntityTypes(baseEntityType, edmModel));
                foreach (var entityType in entityTypes)
                {
                    IEnumerable<IEdmStructuralProperty> properties = entityType == baseEntityType
                        ? entityType.StructuralProperties()
                        : entityType.DeclaredStructuralProperties();
                    if (properties != null)
                    {
                        autoSelectProperties.AddRange(
                            properties.Where(
                                property =>
                                    IsAutoSelect(property, pathProperty, entityType, edmModel,
                                        querySettings)));
                    }
                }
            }
            else if (pathStructuredType != null)
            {
                IEnumerable<IEdmStructuralProperty> properties = pathStructuredType.StructuralProperties();
                if (properties != null)
                {
                    autoSelectProperties.AddRange(
                        properties.Where(
                            property =>
                                IsAutoSelect(property, pathProperty, pathStructuredType, edmModel,
                                    querySettings)));
                }
            }

            return autoSelectProperties;
        }

        /// <summary>
        /// Check whether the two are properly related types
        /// </summary>
        /// <param name="first">the first type</param>
        /// <param name="second">the second type</param>
        /// <returns>Whether the two types are related.</returns>
        public static bool IsRelatedTo(IEdmType first, IEdmType second)
        {
            return second.IsOrInheritsFrom(first) || first.IsOrInheritsFrom(second);
        }
    }
}
