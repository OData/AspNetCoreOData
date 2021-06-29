// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Annotations;
using Microsoft.OData.ModelBuilder.Config;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// Provides the functionalities related to the Edm type.
    /// </summary>
    internal static class EdmHelpers
    {
        /// <summary>
        /// Converts the <see cref="IEdmType"/> to <see cref="IEdmCollectionType"/>.
        /// </summary>
        /// <param name="edmType">The given Edm type.</param>
        /// <param name="isNullable">Nullable or not.</param>
        /// <returns>The collection type.</returns>
        public static IEdmCollectionType ToCollection(this IEdmType edmType, bool isNullable)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            return new EdmCollectionType(edmType.ToEdmTypeReference(isNullable));
        }

        /// <summary>
        /// Converts an Edm Type to Edm type reference.
        /// </summary>
        /// <param name="edmType">The Edm type.</param>
        /// <param name="isNullable">Nullable value.</param>
        /// <returns>The Edm type reference.</returns>
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

                case EdmTypeKind.Path:
                    return new EdmPathTypeReference((IEdmPathType)edmType, isNullable);

                case EdmTypeKind.TypeDefinition:
                    return new EdmTypeDefinitionReference((IEdmTypeDefinition)edmType, isNullable);

                default:
                    throw Error.NotSupported(SRResources.EdmTypeNotSupported, edmType.ToTraceString());
            }
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

        public static void GetPropertyAndStructuredTypeFromPath(IEnumerable<ODataPathSegment> segments,
            out IEdmProperty property, out IEdmStructuredType structuredType, out string name)
        {
            property = null;
            structuredType = null;
            name = String.Empty;
            string typeCast = String.Empty;
            if (segments != null)
            {
                IEnumerable<ODataPathSegment> reverseSegments = segments.Reverse();
                foreach (var segment in reverseSegments)
                {
                    NavigationPropertySegment navigationPathSegment = segment as NavigationPropertySegment;
                    if (navigationPathSegment != null)
                    {
                        property = navigationPathSegment.NavigationProperty;
                        if (structuredType == null)
                        {
                            structuredType = navigationPathSegment.NavigationProperty.ToEntityType();
                        }

                        name = navigationPathSegment.NavigationProperty.Name + typeCast;
                        return;
                    }

                    PropertySegment propertyAccessPathSegment = segment as PropertySegment;
                    if (propertyAccessPathSegment != null)
                    {
                        property = propertyAccessPathSegment.Property;
                        if (structuredType == null)
                        {
                            structuredType = GetElementType(property.Type) as IEdmStructuredType;
                        }
                        name = property.Name + typeCast;
                        return;
                    }

                    EntitySetSegment entitySetSegment = segment as EntitySetSegment;
                    if (entitySetSegment != null)
                    {
                        if (structuredType == null)
                        {
                            structuredType = entitySetSegment.EntitySet.EntityType();
                        }
                        name = entitySetSegment.EntitySet.Name + typeCast;
                        return;
                    }

                    TypeSegment typeSegment = segment as TypeSegment;
                    if (typeSegment != null)
                    {
                        structuredType = GetElementType(typeSegment.EdmType.ToEdmTypeReference(false)) as IEdmStructuredType;
                        typeCast = "/" + structuredType;
                    }
                }
            }
        }

        public static IEdmType GetElementType(IEdmTypeReference edmTypeReference)
        {
            if (edmTypeReference.IsCollection())
            {
                return edmTypeReference.AsCollection().ElementType().Definition;
            }

            return edmTypeReference.Definition;
        }

        public static bool IsTopLimitExceeded(IEdmProperty property, IEdmStructuredType structuredType,
           IEdmModel edmModel, int top, DefaultQuerySettings defaultQuerySettings, out int maxTop)
        {
            maxTop = 0;
            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(property, structuredType, edmModel,
                defaultQuerySettings);
            if (querySettings != null && top > querySettings.MaxTop)
            {
                maxTop = querySettings.MaxTop.Value;
                return true;
            }
            return false;
        }

        public static bool IsNotCountable(IEdmProperty property, IEdmStructuredType structuredType, IEdmModel edmModel,
           bool enableCount)
        {
            if (property != null)
            {
                QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(property, edmModel);
                if (annotation != null && annotation.Restrictions.NotCountable)
                {
                    return true;
                }
            }

            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(property, structuredType, edmModel);
            if (querySettings != null &&
                ((!querySettings.Countable.HasValue && !enableCount) ||
                 querySettings.Countable == false))
            {
                return true;
            }

            return false;
        }

        public static bool IsNotFilterable(IEdmProperty edmProperty, IEdmProperty pathEdmProperty,
            IEdmStructuredType pathEdmStructuredType,
            IEdmModel edmModel, bool enableFilter)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            if (annotation != null && annotation.Restrictions.NotFilterable)
            {
                return true;
            }
            else
            {
                if (pathEdmStructuredType == null)
                {
                    pathEdmStructuredType = edmProperty.DeclaringType;
                }

                ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(pathEdmProperty,
                    pathEdmStructuredType, edmModel);
                if (!enableFilter)
                {
                    return !querySettings.Filterable(edmProperty.Name);
                }

                bool enable;
                if (querySettings.FilterConfigurations.TryGetValue(edmProperty.Name, out enable))
                {
                    return !enable;
                }
                else
                {
                    return querySettings.DefaultEnableFilter == false;
                }
            }
        }

        public static bool IsNotSortable(IEdmProperty edmProperty, IEdmProperty pathEdmProperty,
            IEdmStructuredType pathEdmStructuredType, IEdmModel edmModel, bool enableOrderBy)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            if (annotation != null && annotation.Restrictions.NotSortable)
            {
                return true;
            }
            else
            {
                if (pathEdmStructuredType == null)
                {
                    pathEdmStructuredType = edmProperty.DeclaringType;
                }

                ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(pathEdmProperty,
                    pathEdmStructuredType, edmModel);
                if (!enableOrderBy)
                {
                    return !querySettings.Sortable(edmProperty.Name);
                }

                bool enable;
                if (querySettings.OrderByConfigurations.TryGetValue(edmProperty.Name, out enable))
                {
                    return !enable;
                }
                else
                {
                    return querySettings.DefaultEnableOrderBy == false;
                }
            }
        }

        public static bool IsNotSelectable(IEdmProperty edmProperty, IEdmProperty pathEdmProperty,
            IEdmStructuredType pathEdmStructuredType, IEdmModel edmModel, bool enableSelect)
        {
            if (pathEdmStructuredType == null)
            {
                pathEdmStructuredType = edmProperty.DeclaringType;
            }

            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(pathEdmProperty,
                pathEdmStructuredType, edmModel);
            if (!enableSelect)
            {
                return !querySettings.Selectable(edmProperty.Name);
            }

            SelectExpandType enable;
            if (querySettings.SelectConfigurations.TryGetValue(edmProperty.Name, out enable))
            {
                return enable == SelectExpandType.Disabled;
            }
            else
            {
                return querySettings.DefaultSelectType == SelectExpandType.Disabled;
            }
        }

        public static bool IsNotNavigable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotNavigable;
        }

        public static bool IsNotExpandable(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            QueryableRestrictionsAnnotation annotation = GetPropertyRestrictions(edmProperty, edmModel);
            return annotation == null ? false : annotation.Restrictions.NotExpandable;
        }

        public static bool IsExpandable(string propertyName, IEdmProperty property, IEdmStructuredType structuredType,
            IEdmModel edmModel,
            out ExpandConfiguration expandConfiguration)
        {
            expandConfiguration = null;
            ModelBoundQuerySettings querySettings = GetModelBoundQuerySettings(property, structuredType, edmModel);
            if (querySettings != null)
            {
                bool result = querySettings.Expandable(propertyName);
                if (!querySettings.ExpandConfigurations.TryGetValue(propertyName, out expandConfiguration) && result)
                {
                    expandConfiguration = new ExpandConfiguration
                    {
                        ExpandType = querySettings.DefaultExpandType.Value,
                        MaxDepth = querySettings.DefaultMaxDepth
                    };
                }

                return result;
            }

            return false;
        }

        internal static void SetOperationTitleAnnotation(this IEdmModel model, IEdmOperation action, OperationTitleAnnotation title)
        {
            Contract.Assert(model != null);
            model.SetAnnotationValue(action, title);
        }

        internal static OperationTitleAnnotation GetOperationTitleAnnotation(this IEdmModel model, IEdmOperation operation)
        {
            Contract.Assert(model != null);
            return model.GetAnnotationValue<OperationTitleAnnotation>(operation);
        }
    }
}
