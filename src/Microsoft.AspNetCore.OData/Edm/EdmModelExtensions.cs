// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal static class EdmModelExtensions
    {
        /// <summary>
        /// Resolve the <see cref="IEdmProperty"/> using the property name. This method supports the property name case insensitive.
        /// However, ODL only support case-sensitive. Here's the logic:
        /// 1) If we match
        /// </summary>
        /// <param name="structuredType">The given structural type </param>
        /// <param name="propertyName">The given property name.</param>
        /// <returns>The resolved <see cref="IEdmProperty"/>.</returns>
        public static IEdmProperty ResolveProperty(this IEdmStructuredType structuredType, string propertyName)
        {
            if (structuredType == null)
            {
                throw Error.ArgumentNull(nameof(structuredType));
            }

            bool ambiguous = false;
            IEdmProperty edmProperty = null;
            foreach (var property in structuredType.StructuralProperties())
            {
                string name = property.Name;
                if (name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    if (name.Equals(propertyName, StringComparison.Ordinal))
                    {
                        return property;
                    }
                    else if (edmProperty != null)
                    {
                        ambiguous = true;
                    }
                    else
                    {
                        edmProperty = property;
                    }
                }
            }

            if (ambiguous)
            {
                throw new ODataException(Error.Format(SRResources.AmbiguousPropertyNameFound, propertyName));
            }

            return edmProperty;
        }

        public static IEdmSchemaType ResolveType(this IEdmModel model, string typeName, bool enableCaseInsensitive = false)
        {
            IEdmSchemaType type = model.FindType(typeName);
            if (type != null || !enableCaseInsensitive)
            {
                return type;
            }

            var types = model.SchemaElements.OfType<IEdmSchemaType>()
                .Where(e => string.Equals(typeName, e.FullName(), enableCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

            foreach (var refModels in model.ReferencedModels)
            {
                var refedTypes = refModels.SchemaElements.OfType<IEdmSchemaType>()
                    .Where(e => string.Equals(typeName, e.FullName(), enableCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

                types = types.Concat(refedTypes);
            }

            if (types.Count() > 1)
            {
                throw new Exception($"Multiple type found from the model for '{typeName}'.");
            }

            return types.SingleOrDefault();
        }

        /// <summary>
        /// Resolve the navigation source using the input identifier
        /// </summary>
        /// <param name="model">The Edm model.</param>
        /// <param name="identifier">The indentifier</param>
        /// <param name="enableCaseInsensitive">Enable case insensitive</param>
        /// <returns>Null or the found navigation source.</returns>
        public static IEdmNavigationSource ResolveNavigationSource(this IEdmModel model, string identifier, bool enableCaseInsensitive = false)
        {
            IEdmNavigationSource navSource = model.FindDeclaredNavigationSource(identifier);
            if (navSource != null || !enableCaseInsensitive)
            {
                return navSource;
            }

            IEdmEntityContainer container = model.EntityContainer;
            if (container == null)
            {
                return null;
            }

            var result = container.Elements.OfType<IEdmNavigationSource>()
                .Where(source => string.Equals(identifier, source.Name, StringComparison.OrdinalIgnoreCase)).ToList();

            if (result.Count > 1)
            {
                throw new Exception($"More than one navigation sources match the name '{identifier}' found in model.");
            }

            return result.SingleOrDefault();
        }

        public static IEnumerable<IEdmOperationImport> ResolveOperationImports(this IEdmModel model,
            string identifier,
            bool enableCaseInsensitive = false)
        {
            IEnumerable<IEdmOperationImport> results = model.FindDeclaredOperationImports(identifier);
            if (results.Any() || !enableCaseInsensitive)
            {
                return results;
            }

            IEdmEntityContainer container = model.EntityContainer;
            if (container == null)
            {
                return null;
            }

            return container.Elements.OfType<IEdmOperationImport>()
                .Where(source => string.Equals(identifier, source.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<IEdmOperation> ResolveOperations(this IEdmModel model, string identifier,
            IEdmType bindingType, bool enableCaseInsensitive = false)
        {
            IEnumerable<IEdmOperation> results;
            if (identifier.Contains(".", StringComparison.Ordinal))
            {
                results = FindAcrossModels<IEdmOperation>(model, identifier, true, enableCaseInsensitive);
            }
            else
            {
                results = FindAcrossModels<IEdmOperation>(model, identifier, false, enableCaseInsensitive);
            }

            var operations = results?.ToList();
            if (operations != null && operations.Any())
            {
                IList<IEdmOperation> matchedOperation = new List<IEdmOperation>();
                for (int i = 0; i < operations.Count; i++)
                {
                    if (operations[i].HasEquivalentBindingType(bindingType))
                    {
                        matchedOperation.Add(operations[i]);
                    }
                }

                return matchedOperation;
            }

            return Enumerable.Empty<IEdmOperation>();
        }

        internal static IEdmEntitySetBase GetTargetEntitySet(this IEdmOperation operation, IEdmNavigationSource source, IEdmModel model)
        {
            if (source == null)
            {
                return null;
            }

            if (operation.IsBound && operation.Parameters.Any())
            {
                IEdmOperationParameter parameter;
                Dictionary<IEdmNavigationProperty, IEdmPathExpression> path;
                IEdmEntityType lastEntityType;

                if (operation.TryGetRelativeEntitySetPath(model, out parameter, out path, out lastEntityType, out IEnumerable<EdmError> _))
                {
                    IEdmNavigationSource target = source;

                    foreach (var navigation in path)
                    {
                        target = target.FindNavigationTarget(navigation.Key, navigation.Value);
                    }

                    return target as IEdmEntitySetBase;
                }
            }

            return null;
        }

        public static IEdmNavigationSource FindNavigationTarget(this IEdmNavigationSource navigationSource,
            IEdmNavigationProperty navigationProperty,
            IList<ODataSegmentTemplate> parsedSegments,
            out IEdmPathExpression bindingPath)
        {
            bindingPath = null;

            if (navigationProperty.ContainsTarget)
            {
                return navigationSource;
            }

            IEnumerable<IEdmNavigationPropertyBinding> bindings =
                navigationSource.FindNavigationPropertyBindings(navigationProperty);

            if (bindings != null)
            {
                foreach (var binding in bindings)
                {
                    if (BindingPathHelper.MatchBindingPath(binding.Path, parsedSegments))
                    {
                        bindingPath = binding.Path;
                        return binding.Target;
                    }
                }
            }

            return null;
        }

        private static IEnumerable<T> FindAcrossModels<T>(IEdmModel model,
            string identifier, bool fullName, bool caseInsensitive) where T : IEdmSchemaElement
        {
            Func<IEdmModel, IEnumerable<T>> finder = (refModel) =>
                refModel.SchemaElements.OfType<T>()
                .Where(e => string.Equals(identifier, fullName ? e.FullName() : e.Name,
                caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));

            IEnumerable<T> results = finder(model);

            foreach (IEdmModel reference in model.ReferencedModels)
            {
                results.Concat(finder(reference));
            }

            return results;
        }

        internal static bool IsStructuredCollectionType(this IEdmTypeReference typeReference)
        {
            return typeReference.Definition.IsStructuredCollectionType();
        }

        internal static bool IsStructuredCollectionType(this IEdmType type)
        {
            IEdmCollectionType collectionType = type as IEdmCollectionType;

            if (collectionType == null
                || (collectionType.ElementType != null
                    && (collectionType.ElementType.TypeKind() != EdmTypeKind.Entity && collectionType.ElementType.TypeKind() != EdmTypeKind.Complex)))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Try to get the entity type <see cref="IEdmEntityType"/> from the input <see cref="IEdmType"/>
        /// </summary>
        /// <param name="edmType">The input Edm Type.</param>
        /// <param name="entityType">The output Entity Type.</param>
        /// <returns>True/false</returns>
        public static bool TryGetEntityType(this IEdmType edmType, out IEdmEntityType entityType)
        {
            if (edmType == null || edmType.TypeKind != EdmTypeKind.Collection)
            {
                entityType = null;
                return false;
            }

            entityType = ((IEdmCollectionType)edmType).ElementType.Definition as IEdmEntityType;
            return entityType != null;
        }

        public static bool IsEntityOrEntityCollectionType(this IEdmType edmType, out IEdmEntityType entityType)
        {
            if (edmType.TypeKind == EdmTypeKind.Entity)
            {
                entityType = (IEdmEntityType)edmType;
                return true;
            }

            if (edmType.TypeKind != EdmTypeKind.Collection)
            {
                entityType = null;
                return false;
            }

            entityType = ((IEdmCollectionType)edmType).ElementType.Definition as IEdmEntityType;
            return entityType != null;
        }

        internal static bool IsResourceOrCollectionResource(this IEdmTypeReference edmType)
        {
            if (edmType.IsEntity() || edmType.IsComplex())
            {
                return true;
            }

            if (edmType.IsCollection())
            {
                return IsResourceOrCollectionResource(edmType.AsCollection().ElementType());
            }

            return false;
        }

        internal static bool IsEnumOrCollectionEnum(this IEdmTypeReference edmType)
        {
            if (edmType.IsEnum())
            {
                return true;
            }

            if (edmType.IsCollection())
            {
                return IsEnumOrCollectionEnum(edmType.AsCollection().ElementType());
            }

            return false;
        }

        internal static string GetNavigationSourceUrl(this IEdmModel model, IEdmNavigationSource navigationSource)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (navigationSource == null)
            {
                throw new ArgumentNullException(nameof(navigationSource));
            }

            //NavigationSourceUrlAnnotation annotation = model.GetAnnotationValue<NavigationSourceUrlAnnotation>(navigationSource);
            //if (annotation == null)
            //{
            //    return navigationSource.Name;
            //}
            //else
            //{
            //    return annotation.Url;
            //}

            return null;
        }

        public static IEnumerable<IEdmStructuredType> BaseTypes(this IEdmStructuredType structuralType)
        {
            IEdmStructuredType baseType = structuralType.BaseType;
            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        public static IEnumerable<IEdmStructuredType> ThisAndBaseTypes(this IEdmStructuredType structuralType)
        {
            IEdmStructuredType baseType = structuralType;
            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        public static IEnumerable<IEdmStructuredType> DerivedTypes(this IEdmStructuredType structuralType, IEdmModel model)
        {
            return model.FindAllDerivedTypes(structuralType);
        }

        /// <summary>
        /// Find the given type in a structured type inheritance, include itself.
        /// </summary>
        /// <param name="structuralType">The starting structural type.</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="typeName">The searching type name.</param>
        /// <returns>The found type.</returns>
        public static IEdmStructuredType FindTypeInInheritance(this IEdmStructuredType structuralType, IEdmModel model, string typeName)
        {
            IEdmStructuredType baseType = structuralType;
            while (baseType != null)
            {
                if (GetName(baseType) == typeName)
                {
                    return baseType;
                }

                baseType = baseType.BaseType;
            }

            return model.FindAllDerivedTypes(structuralType).FirstOrDefault(c => GetName(c) == typeName);
        }

        private static string GetName(IEdmStructuredType type)
        {
            IEdmEntityType entityType = type as IEdmEntityType;
            if (entityType != null)
            {
                return entityType.Name;
            }

            return ((IEdmComplexType)type).Name;
        }

        public static IEdmTypeReference GetElementTypeOrSelf(this IEdmTypeReference typeReference)
        {
            if (typeReference.TypeKind() == EdmTypeKind.Collection)
            {
                IEdmCollectionTypeReference collectType = typeReference.AsCollection();
                return collectType.ElementType();
            }

            return typeReference;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmAction> GetAvailableActions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, false).OfType<IEdmAction>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmFunction> GetAvailableFunctions(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, false).OfType<IEdmFunction>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmOperation> GetAvailableOperationsBoundToCollection(this IEdmModel model, IEdmEntityType entityType)
        {
            return model.GetAvailableOperations(entityType, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="entityType"></param>
        /// <param name="boundToCollection"></param>
        /// <returns></returns>
        public static IEnumerable<IEdmOperation> GetAvailableOperations(this IEdmModel model, IEdmEntityType entityType, bool boundToCollection = false)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            BindableOperationFinder annotation = model.GetAnnotationValue<BindableOperationFinder>(model);
            if (annotation == null)
            {
                annotation = new BindableOperationFinder(model);
                model.SetAnnotationValue(model, annotation);
            }

            if (boundToCollection)
            {
                return annotation.FindOperationsBoundToCollection(entityType);
            }
            else
            {
                return annotation.FindOperations(entityType);
            }
        }
    }
}
