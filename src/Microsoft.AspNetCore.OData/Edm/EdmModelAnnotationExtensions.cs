// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OData.Edm.Vocabularies.V1;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.AspNetCore.OData.Edm
{
    /// <summary>
    /// The extensions for the <see cref="IEdmModel"/> for the annotations.
    /// </summary>
    public static class EmdModelAnnotationExtensions
    {
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
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
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
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (edmProperty == null)
            {
                throw new ArgumentNullException(nameof(edmProperty));
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
                throw new ArgumentNullException(nameof(edmModel));
            }

            if (edmType == null)
            {
                throw new ArgumentNullException(nameof(edmType));
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
                throw new ArgumentNullException(nameof(model));
            }

            ModelNameAnnotation annotation =
                model.GetAnnotationValue<ModelNameAnnotation>(model);
            if (annotation != null)
            {
                return annotation.ModelName;
            }

            return SetModelName(model);
        }

        /// <summary>
        /// Sets the Edm model name.
        /// </summary>
        /// <param name="model">The Edm model.</param>
        public static string SetModelName(this IEdmModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            string name = Guid.NewGuid().ToString();
            model.SetAnnotationValue(model, new ModelNameAnnotation(name));
            return name;
        }
    }
}
