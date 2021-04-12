// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace Microsoft.AspNetCore.OData.Edm
{
    internal static class EdmModelExtensions
    {
        /// <summary>
        /// Gets the <see cref="NavigationSourceLinkBuilderAnnotation"/> to be used while generating self and navigation
        /// links for the given navigation source.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the navigation source.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <returns>The <see cref="NavigationSourceLinkBuilderAnnotation"/> if set for the given the singleton; otherwise,
        /// a new <see cref="NavigationSourceLinkBuilderAnnotation"/> that generates URLs that follow OData URL conventions.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmNavigationSource is more relevant here.")]
        public static NavigationSourceLinkBuilderAnnotation GetNavigationSourceLinkBuilder(this IEdmModel model,
            IEdmNavigationSource navigationSource)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            NavigationSourceLinkBuilderAnnotation annotation = model
                .GetAnnotationValue<NavigationSourceLinkBuilderAnnotation>(navigationSource);
            if (annotation == null)
            {
                // construct and set a navigation source link builder that follows OData URL conventions.
                annotation = new NavigationSourceLinkBuilderAnnotation(navigationSource, model);
                model.SetNavigationSourceLinkBuilder(navigationSource, annotation);
            }

            return annotation;
        }

        /// <summary>
        /// Sets the <see cref="NavigationSourceLinkBuilderAnnotation"/> to be used while generating self and navigation
        /// links for the given navigation source.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the navigation source.</param>
        /// <param name="navigationSource">The navigation source.</param>
        /// <param name="navigationSourceLinkBuilder">The <see cref="NavigationSourceLinkBuilderAnnotation"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmNavigationSource is more relevant here.")]
        public static void SetNavigationSourceLinkBuilder(this IEdmModel model, IEdmNavigationSource navigationSource,
            NavigationSourceLinkBuilderAnnotation navigationSourceLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(navigationSource, navigationSourceLinkBuilder);
        }

        /// <summary>
        /// Gets the <see cref="OperationLinkBuilder"/> to be used while generating operation links for the given action.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the operation.</param>
        /// <param name="operation">The operation for which the link builder is needed.</param>
        /// <returns>The <see cref="OperationLinkBuilder"/> for the given operation if one is set; otherwise, a new
        /// <see cref="OperationLinkBuilder"/> that generates operation links following OData URL conventions.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmActionImport is more relevant here.")]
        public static OperationLinkBuilder GetOperationLinkBuilder(this IEdmModel model, IEdmOperation operation)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (operation == null)
            {
                throw Error.ArgumentNull("operation");
            }

            OperationLinkBuilder linkBuilder = model.GetAnnotationValue<OperationLinkBuilder>(operation);
            if (linkBuilder == null)
            {
                linkBuilder = GetDefaultOperationLinkBuilder(operation);
                model.SetOperationLinkBuilder(operation, linkBuilder);
            }

            return linkBuilder;
        }

        /// <summary>
        /// Sets the <see cref="OperationLinkBuilder"/> to be used for generating the OData operation link for the given operation.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel"/> containing the entity set.</param>
        /// <param name="operation">The operation for which the operation link is to be generated.</param>
        /// <param name="operationLinkBuilder">The <see cref="OperationLinkBuilder"/> to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "IEdmActionImport is more relevant here.")]
        public static void SetOperationLinkBuilder(this IEdmModel model, IEdmOperation operation, OperationLinkBuilder operationLinkBuilder)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            model.SetAnnotationValue(operation, operationLinkBuilder);
        }

        private static OperationLinkBuilder GetDefaultOperationLinkBuilder(IEdmOperation operation)
        {
            OperationLinkBuilder linkBuilder = null;
            if (operation.Parameters != null)
            {
                if (operation.Parameters.First().Type.IsEntity())
                {
                    if (operation is IEdmAction)
                    {
                        linkBuilder = new OperationLinkBuilder(
                            (ResourceContext resourceContext) =>
                                resourceContext.GenerateActionLink(operation), followsConventions: true);
                    }
                    else
                    {
                        linkBuilder = new OperationLinkBuilder(
                            (ResourceContext resourceContext) =>
                                resourceContext.GenerateFunctionLink(operation), followsConventions: true);
                    }
                }
                else if (operation.Parameters.First().Type.IsCollection())
                {
                    if (operation is IEdmAction)
                    {
                        linkBuilder =
                            new OperationLinkBuilder(
                                (ResourceSetContext reseourceSetContext) =>
                                    reseourceSetContext.GenerateActionLink(operation), followsConventions: true);
                    }
                    else
                    {
                        linkBuilder =
                            new OperationLinkBuilder(
                                (ResourceSetContext reseourceSetContext) =>
                                    reseourceSetContext.GenerateFunctionLink(operation), followsConventions: true);
                    }
                }
            }
            return linkBuilder;
        }

        public static IEdmProperty ResolveProperty(this IEdmStructuredType type, string propertyName, bool enableCaseInsensitive = false)
        {
            IEdmProperty property = type.FindProperty(propertyName);
            if (property != null || !enableCaseInsensitive)
            {
                return property;
            }

            var result = type.Properties()
            .Where(_ => string.Equals(propertyName, _.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

            return result.SingleOrDefault();
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

        public static IEnumerable<IEdmStructuredType> BaseTypes(
            this IEdmStructuredType structuralType)
        {
            IEdmStructuredType baseType = structuralType.BaseType;
            while (baseType != null)
            {
                yield return baseType;

                baseType = baseType.BaseType;
            }
        }

        public static IEnumerable<IEdmStructuredType> ThisAndBaseTypes(
            this IEdmStructuredType structuralType)
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
