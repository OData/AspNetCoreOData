using System.Collections.Generic;
using System.Diagnostics.Contracts;
using QueryBuilder.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.ModelBuilder.Annotations;
using Microsoft.OData.ModelBuilder.Config;

namespace QueryBuilder.Edm
{
    /// <summary>
    /// Provides the functionalities related to the Edm type.
    /// </summary>
    internal static class EdmHelpers
    {
        /// <summary>
        /// Get element type reference if it's collection or return itself
        /// </summary>
        /// <param name="typeReference">The test type reference.</param>
        /// <returns>Element type or itself.</returns>
        public static IEdmTypeReference GetElementTypeOrSelf(this IEdmTypeReference typeReference)
        {
            if (typeReference == null)
            {
                return typeReference;
            }

            if (typeReference.TypeKind() == EdmTypeKind.Collection)
            {
                IEdmCollectionTypeReference collectType = typeReference.AsCollection();
                return collectType.ElementType();
            }

            return typeReference;
        }

        /// <summary>
        /// Get the elementType if it's collection or return itself's type
        /// </summary>
        /// <param name="edmTypeReference">The test type reference.</param>
        /// <returns>Element type or itself.</returns>
        public static IEdmType GetElementType(this IEdmTypeReference edmTypeReference)
        {
            return edmTypeReference.GetElementTypeOrSelf()?.Definition;
        }

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
            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

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

        public static IEnumerable<IEdmEntityType> GetAllDerivedEntityTypes(IEdmEntityType entityType, IEdmModel edmModel)
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

        public static bool IsTopLimitExceeded(IEdmProperty property, IEdmStructuredType structuredType,
           IEdmModel edmModel, int top, DefaultQueryConfigurations defaultQueryConfigs, out int maxTop)
        {
            maxTop = 0;
            ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(property, structuredType, defaultQueryConfigs);
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

            ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(property, structuredType);
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

                ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(pathEdmProperty, pathEdmStructuredType);
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

                ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(pathEdmProperty, pathEdmStructuredType);
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

            ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(pathEdmProperty, pathEdmStructuredType);
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
            ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(property, structuredType);
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

        public static ModelBoundQuerySettings GetModelBoundQuerySettingsOrNull(this IEdmModel edmModel, IEdmStructuredType structuredType, IEdmProperty property)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            ModelBoundQuerySettings querySettingsOnType = null;
            if (structuredType != null)
            {
                querySettingsOnType = edmModel.GetAnnotationValue<ModelBoundQuerySettings>(structuredType);
            }

            if (property == null)
            {
                return querySettingsOnType;
            }

            ModelBoundQuerySettings querySettingsOnProperty = edmModel.GetAnnotationValue<ModelBoundQuerySettings>(property);
            if (querySettingsOnProperty == null)
            {
                return querySettingsOnType;
            }
            else
            {
                if (querySettingsOnType == null)
                {
                    return querySettingsOnProperty;
                }
                else
                {
                    // Settings on property is higher priority than the ones on type.
                    return GetMergedPropertyQuerySettings(querySettingsOnProperty, querySettingsOnType);
                }
            }
        }

        public static ModelBoundQuerySettings GetModelBoundQuerySettings(this IEdmModel edmModel, IEdmProperty property,
           IEdmStructuredType structuredType, DefaultQueryConfigurations defaultQueryConfigs = null)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            ModelBoundQuerySettings querySettings = edmModel.GetModelBoundQuerySettings(structuredType, defaultQueryConfigs);
            if (property == null)
            {
                return querySettings;
            }
            else
            {
                // Settings on property is higher priority than the ones on type.
                ModelBoundQuerySettings propertyQuerySettings = edmModel.GetModelBoundQuerySettings(property, defaultQueryConfigs);
                return GetMergedPropertyQuerySettings(propertyQuerySettings, querySettings);
            }
        }

        private static ModelBoundQuerySettings GetModelBoundQuerySettings<T>(this IEdmModel edmModel, T key, DefaultQueryConfigurations defaultQueryConfigs = null)
            where T : IEdmElement
        {
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
                    if (defaultQueryConfigs != null &&
                        (!defaultQueryConfigs.MaxTop.HasValue || defaultQueryConfigs.MaxTop > 0))
                    {
                        querySettings.MaxTop = defaultQueryConfigs.MaxTop;
                    }
                }

                return querySettings;
            }
        }

        private static ModelBoundQuerySettings GetMergedPropertyQuerySettings(ModelBoundQuerySettings propertyQuerySettings, ModelBoundQuerySettings propertyTypeQuerySettings)
        {
            ModelBoundQuerySettings mergedQuerySettings = new ModelBoundQuerySettings(propertyQuerySettings);
            if (propertyTypeQuerySettings != null)
            {
                if (!mergedQuerySettings.PageSize.HasValue)
                {
                    mergedQuerySettings.PageSize = propertyTypeQuerySettings.PageSize;
                }

                if (mergedQuerySettings.MaxTop == 0 && propertyTypeQuerySettings.MaxTop != 0)
                {
                    mergedQuerySettings.MaxTop = propertyTypeQuerySettings.MaxTop;
                }

                if (!mergedQuerySettings.Countable.HasValue)
                {
                    mergedQuerySettings.Countable = propertyTypeQuerySettings.Countable;
                }

                if (mergedQuerySettings.OrderByConfigurations.Count == 0 && !mergedQuerySettings.DefaultEnableOrderBy.HasValue)
                {
                    mergedQuerySettings.CopyOrderByConfigurations(propertyTypeQuerySettings.OrderByConfigurations);
                    mergedQuerySettings.DefaultEnableOrderBy = propertyTypeQuerySettings.DefaultEnableOrderBy;
                }

                if (mergedQuerySettings.FilterConfigurations.Count == 0 && !mergedQuerySettings.DefaultEnableFilter.HasValue)
                {
                    mergedQuerySettings.CopyFilterConfigurations(propertyTypeQuerySettings.FilterConfigurations);
                    mergedQuerySettings.DefaultEnableFilter = propertyTypeQuerySettings.DefaultEnableFilter;
                }

                if (mergedQuerySettings.SelectConfigurations.Count == 0 && !mergedQuerySettings.DefaultSelectType.HasValue)
                {
                    mergedQuerySettings.CopySelectConfigurations(propertyTypeQuerySettings.SelectConfigurations);
                    mergedQuerySettings.DefaultSelectType = propertyTypeQuerySettings.DefaultSelectType;
                }

                if (mergedQuerySettings.ExpandConfigurations.Count == 0 && !mergedQuerySettings.DefaultExpandType.HasValue)
                {
                    mergedQuerySettings.CopyExpandConfigurations(propertyTypeQuerySettings.ExpandConfigurations);
                    mergedQuerySettings.DefaultExpandType = propertyTypeQuerySettings.DefaultExpandType;
                    mergedQuerySettings.DefaultMaxDepth = propertyTypeQuerySettings.DefaultMaxDepth;
                }
            }

            return mergedQuerySettings;
        }

        internal static QueryableRestrictionsAnnotation GetPropertyRestrictions(IEdmProperty edmProperty, IEdmModel edmModel)
        {
            Contract.Assert(edmProperty != null);
            Contract.Assert(edmModel != null);

            return edmModel.GetAnnotationValue<QueryableRestrictionsAnnotation>(edmProperty);
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
