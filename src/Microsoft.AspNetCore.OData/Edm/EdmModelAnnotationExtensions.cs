// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.Community.V1;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// The extensions for the <see cref="IEdmModel"/> for the annotations.
    /// </summary>
    public static class EdmModelAnnotationExtensions
    {
        /// <summary>
        /// Gets the Org.OData.Core.V1.AcceptableMediaTypes
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="target">The vocabulary annotatable target.</param>
        /// <returns>null or the collection of media type.</returns>
        internal static IList<string> GetAcceptableMediaTypes(this IEdmModel model, IEdmVocabularyAnnotatable target)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (target == null)
            {
                throw Error.ArgumentNull(nameof(target));
            }

            IList<string> mediaTypes = null;
            var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(target, CoreVocabularyModel.AcceptableMediaTypesTerm);
            IEdmVocabularyAnnotation annotation = annotations.FirstOrDefault();
            if (annotation != null)
            {
                IEdmCollectionExpression properties = annotation.Value as IEdmCollectionExpression;
                if (properties != null)
                {
                    mediaTypes = new List<string>();
                    foreach (var element in properties.Elements)
                    {
                        IEdmStringConstantExpression elementValue = element as IEdmStringConstantExpression;
                        if (elementValue != null)
                        {
                            mediaTypes.Add(elementValue.Value);
                        }
                    }
                }
            }

            if (mediaTypes == null || mediaTypes.Count == 0)
            {
                return null;
            }

            return mediaTypes;
        }

        /// <summary>
        /// Get concurrency properties.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The concurrency properties.</returns>
        public static IEnumerable<IEdmStructuralProperty> GetConcurrencyProperties(this IEdmModel model, IEdmNavigationSource navigationSource)
        {
            Contract.Assert(model != null);
            Contract.Assert(navigationSource != null);

            // Ensure that concurrency properties cache is attached to model as an annotation to avoid expensive calculations each time
            ConcurrencyPropertiesAnnotation concurrencyProperties = model.GetAnnotationValue<ConcurrencyPropertiesAnnotation>(model);
            if (concurrencyProperties == null)
            {
                concurrencyProperties = new ConcurrencyPropertiesAnnotation();
                model.SetAnnotationValue(model, concurrencyProperties);
            }

            IEnumerable<IEdmStructuralProperty> cachedProperties;
            if (concurrencyProperties.TryGetValue(navigationSource, out cachedProperties))
            {
                return cachedProperties;
            }

            IList<IEdmStructuralProperty> results = new List<IEdmStructuralProperty>();
            IEdmEntityType entityType = navigationSource.EntityType();
            IEdmVocabularyAnnotatable annotatable = navigationSource as IEdmVocabularyAnnotatable;
            if (annotatable != null)
            {
                var annotations = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(annotatable, CoreVocabularyModel.ConcurrencyTerm);
                IEdmVocabularyAnnotation annotation = annotations.FirstOrDefault();
                if (annotation != null)
                {
                    IEdmCollectionExpression properties = annotation.Value as IEdmCollectionExpression;
                    if (properties != null)
                    {
                        foreach (var property in properties.Elements)
                        {
                            IEdmPathExpression pathExpression = property as IEdmPathExpression;
                            if (pathExpression != null)
                            {
                                // So far, we only consider the single path, because only the direct properties from declaring type are used.
                                // However we have an issue tracking on: https://github.com/OData/WebApi/issues/472
                                string propertyName = pathExpression.PathSegments.First();
                                IEdmProperty edmProperty = entityType.FindProperty(propertyName);
                                IEdmStructuralProperty structuralProperty = edmProperty as IEdmStructuralProperty;
                                if (structuralProperty != null)
                                {
                                    results.Add(structuralProperty);
                                }
                            }
                        }
                    }
                }
            }

            concurrencyProperties[navigationSource] = results;
            return results;
        }

        /// <summary>
        /// Gets the Enum member annotations.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="enumType">The Enum Type.</param>
        /// <returns>The Enum member annotation.</returns>
        public static ClrEnumMemberAnnotation GetClrEnumMemberAnnotation(this IEdmModel edmModel, IEdmEnumType enumType)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (enumType == null)
            {
                throw Error.ArgumentNull(nameof(enumType));
            }

            ClrEnumMemberAnnotation annotation = edmModel.GetAnnotationValue<ClrEnumMemberAnnotation>(enumType);
            if (annotation != null)
            {
                return annotation;
            }

            return null;
        }

        /// <summary>
        /// Get the CLR property name.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmProperty">The Edm property.</param>
        /// <returns>The property name.</returns>
        public static string GetClrPropertyName(this IEdmModel edmModel, IEdmProperty edmProperty)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (edmProperty == null)
            {
                throw Error.ArgumentNull(nameof(edmProperty));
            }

            string propertyName = edmProperty.Name;
            ClrPropertyInfoAnnotation annotation = edmModel.GetAnnotationValue<ClrPropertyInfoAnnotation>(edmProperty);
            if (annotation != null)
            {
                PropertyInfo propertyInfo = annotation.ClrPropertyInfo;
                if (propertyInfo != null)
                {
                    propertyName = propertyInfo.Name;
                }
            }

            return propertyName;
        }

        /// <summary>
        /// Gets the dynamic property container name.
        /// </summary>
        /// <param name="edmModel">The Edm model.</param>
        /// <param name="edmType">The Edm type.</param>
        /// <returns>The dynamic property container property info.</returns>
        public static PropertyInfo GetDynamicPropertyDictionary(this IEdmModel edmModel, IEdmStructuredType edmType)
        {
            if (edmModel == null)
            {
                throw Error.ArgumentNull(nameof(edmModel));
            }

            if (edmType == null)
            {
                throw Error.ArgumentNull(nameof(edmType));
            }

            DynamicPropertyDictionaryAnnotation annotation =
                edmModel.GetAnnotationValue<DynamicPropertyDictionaryAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.PropertyInfo;
            }

            return null;
        }

        /// <summary>
        /// Gets the model name.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <returns>The Edm model name.</returns>
        public static string GetModelName(this IEdmModel model)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            ModelNameAnnotation annotation =
                model.GetAnnotationValue<ModelNameAnnotation>(model);
            if (annotation != null)
            {
                return annotation.ModelName;
            }

            string name = Guid.NewGuid().ToString();
            SetModelName(model, name);
            return name;
        }

        /// <summary>
        /// Sets the Edm model name.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="name">The Edm model name.</param>
        public static void SetModelName(this IEdmModel model, string name)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (name == null)
            {
                throw Error.ArgumentNull(nameof(name));
            }

            model.SetAnnotationValue(model, new ModelNameAnnotation(name));
        }

        /// <summary>
        /// Gets the declared alternate keys of the most defined entity with a declared key present.
        /// Each entity type could define a set of alternate keys.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="entityType">The Edm entity type.</param>
        /// <returns>Alternate Keys of this type.</returns>
        public static IEnumerable<IDictionary<string, IEdmPathExpression>> GetAlternateKeys(this IEdmModel model, IEdmEntityType entityType)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            if (entityType == null)
            {
                throw Error.ArgumentNull(nameof(entityType));
            }

            IEnumerable<IDictionary<string, IEdmPathExpression>> alternateCoreKeys = null;
            // Let's search the core.alternate key term first
            IEdmTerm coreAlternateTerm = CoreVocabularyModel.Instance.FindDeclaredTerm("Org.OData.Core.V1.AlternateKeys");
            if (coreAlternateTerm != null)
            {
                model.TryGetAlternateKeys(entityType, coreAlternateTerm, out alternateCoreKeys);
            }

            // for back compability, let's support the community.alternatekey
            IEnumerable<IDictionary<string, IEdmPathExpression>> alternateKeys = null;
            IEdmTerm communityAlternateTerm = AlternateKeysVocabularyModel.Instance.FindDeclaredTerm("OData.Community.Keys.V1.AlternateKeys");
            if (communityAlternateTerm != null)
            {
                model.TryGetAlternateKeys(entityType, communityAlternateTerm, out alternateKeys);
            }

            if (alternateCoreKeys == null)
            {
                return alternateKeys;
            }
            else if (alternateKeys == null)
            {
                return alternateCoreKeys;
            }
            else
            {
                return alternateCoreKeys.Concat(alternateKeys);
            }
        }

        private static bool TryGetAlternateKeys(this IEdmModel model, IEdmEntityType entityType, IEdmTerm term,
            out IEnumerable<IDictionary<string, IEdmPathExpression>> alternateKeys)
        {
            IEdmEntityType checkingType = entityType;
            while (checkingType != null)
            {
                IEnumerable<IDictionary<string, IEdmPathExpression>> declaredAlternateKeys = GetDeclaredAlternateKeysForType(model, checkingType, term);
                if (declaredAlternateKeys != null)
                {
                    alternateKeys = declaredAlternateKeys;
                    return true;
                }

                checkingType = checkingType.BaseEntityType();
            }

            alternateKeys = null;
            return false;
        }

        private static IEnumerable<IDictionary<string, IEdmPathExpression>> GetDeclaredAlternateKeysForType(IEdmModel model, IEdmEntityType type, IEdmTerm term)
        {
            IEdmVocabularyAnnotation annotationValue = model.FindVocabularyAnnotations<IEdmVocabularyAnnotation>(type, term).FirstOrDefault();
            if (annotationValue != null)
            {
                List<IDictionary<string, IEdmPathExpression>> declaredAlternateKeys = new List<IDictionary<string, IEdmPathExpression>>();

                IEdmCollectionExpression keys = annotationValue.Value as IEdmCollectionExpression;

                foreach (IEdmRecordExpression key in keys.Elements.OfType<IEdmRecordExpression>())
                {
                    var edmPropertyConstructor = key.Properties.FirstOrDefault(e => e.Name == "Key");
                    if (edmPropertyConstructor != null)
                    {
                        IEdmCollectionExpression collectionExpression = edmPropertyConstructor.Value as IEdmCollectionExpression;

                        IDictionary<string, IEdmPathExpression> alternateKey = new Dictionary<string, IEdmPathExpression>();
                        foreach (IEdmRecordExpression propertyRef in collectionExpression.Elements.OfType<IEdmRecordExpression>())
                        {
                            var aliasProp = propertyRef.Properties.FirstOrDefault(e => e.Name == "Alias");
                            string alias = ((IEdmStringConstantExpression)aliasProp.Value).Value;

                            var nameProp = propertyRef.Properties.FirstOrDefault(e => e.Name == "Name");
                            alternateKey[alias] = (IEdmPathExpression)nameProp.Value;
                        }

                        if (alternateKey.Any())
                        {
                            declaredAlternateKeys.Add(alternateKey);
                        }
                    }
                }

                return declaredAlternateKeys;
            }

            return null;
        }
    }
}
