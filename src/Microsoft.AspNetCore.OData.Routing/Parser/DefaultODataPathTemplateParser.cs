// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public class DefaultODataPathTemplateParser : IODataPathTemplateParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="odataPath"></param>
        /// <returns></returns>
        public virtual ODataPathTemplate Parse111(IEdmModel model, string odataPath)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (odataPath == null)
            {
                throw new ArgumentNullException(nameof(odataPath));
            }

            List<ODataSegmentTemplate> pathSegments = new List<ODataSegmentTemplate>();
            ODataSegmentTemplate pathSegment = null;
            IEdmType previousEdmType = null;
            foreach (string segment in ParseSegments(odataPath))
            {
                pathSegment = ParseNextSegment(model, pathSegment, previousEdmType, segment);

                // If the Uri stops matching the model at any point, return null
                if (pathSegment == null)
                {
                    return null;
                }

                pathSegments.Add(pathSegment);
              //  previousEdmType = pathSegment.GetEdmType(previousEdmType);
            }

            return new ODataPathTemplate(pathSegments);
        }

        /// <summary>
        /// Parse the string like "/users/{id}/contactFolders/{contactFolderId}/contacts"
        /// to segments
        /// </summary>
        /// <param name="model">the Edm model.</param>
        /// <param name="odataPath">the setting.</param>
        /// <returns>Null or <see cref="UriParser"/>.</returns>
        public virtual ODataPathTemplate Parse(IEdmModel model, string odataPath)
        {
            if (model == null || String.IsNullOrEmpty(odataPath))
            {
                return null;
            }

            // TODO: process the one-drive uri escape function call
            string[] items = odataPath.Split('/');
            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
            foreach (var item in items)
            {
                string trimedItem = item.Trim();
                if (string.IsNullOrEmpty(trimedItem))
                {
                    continue;
                }

                if (segments.Count == 0)
                {
                    CreateFirstSegment(trimedItem, model, segments);
                }
                else
                {
                    CreateNextSegment(trimedItem, model, segments);
                }
            }

            return new ODataPathTemplate(segments);
        }

        /// <summary>
        /// Process the first segment in the request uri.
        /// The first segment could be only singleton/entityset/operationimport, doesn't consider the $metadata, $batch
        /// </summary>
        /// <param name="identifier">the whole identifier of this segment</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The out put of the path, because it may include the key segment.</param>
        internal static void CreateFirstSegment(string identifier, IEdmModel model,
            IList<ODataSegmentTemplate> path)
        {
            // the identifier maybe include the key, for example: ~/users({id})
            identifier = identifier.ExtractParenthesis(out string parenthesisExpressions);

            // Try to bind entity set or singleton
            if (TryBindNavigationSource(identifier, parenthesisExpressions, model, path))
            {
                return;
            }

            // Try to bind operation import
            if (TryBindOperationImport(identifier, parenthesisExpressions, model, path))
            {
                return;
            }

            throw new Exception($"Unknown kind of first segment: '{identifier}'");
        }

        /// <summary>
        /// Create the next segment for the request uri.
        /// </summary>
        /// <param name="identifier">The request segment uri literal</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The out of the path</param>
        internal static void CreateNextSegment(string identifier, IEdmModel model, IList<ODataSegmentTemplate> path)
        {
            // GET /Users/{id}
            // GET /Users({id})
            // GET /me/outlook/supportedTimeZones(TimeZoneStandard=microsoft.graph.timeZoneStandard'{timezone_format}')

            // maybe key or function parameters
            identifier = identifier.ExtractParenthesis(out string parenthesisExpressions);

            // can be "property, navproperty"
            if (TryBindPropertySegment(identifier, parenthesisExpressions, model, path))
            {
                return;
            }

            // bind to type cast.
            if (TryBindTypeCastSegment(identifier, parenthesisExpressions, model, path))
            {
                return;
            }

            // bound operations
            if (TryBindOperations(identifier, parenthesisExpressions, model, path))
            {
                return;
            }

            // Handle Key as Segment
            if (TryBindKeySegment("(" + identifier + ")", path))
            {
                return;
            }

            throw new Exception($"Unknown kind of segment: '{identifier}', previous segment: '{path.Last().Literal}'.");
        }

        /// <summary>
        /// Try to bind the idenfier as navigation source segment,
        /// Append it into path.
        /// </summary>
        internal static bool TryBindNavigationSource(string identifier,
            string parenthesisExpressions, // the potention parenthesis expression after identifer
            IEdmModel model,
            IList<ODataSegmentTemplate> path)
        {
            IEdmNavigationSource source = model.FindDeclaredNavigationSource(identifier);
            IEdmEntitySet entitySet = source as IEdmEntitySet;
            IEdmSingleton singleton = source as IEdmSingleton;

            if (entitySet != null)
            {
                path.Add(new EntitySetSegmentTemplate(entitySet));

                // can append parenthesis after entity set. it should be the key
                if (parenthesisExpressions != null)
                {
                    if (!TryBindKeySegment(parenthesisExpressions, path))
                    {
                        throw new Exception($"Unknown parenthesis '{parenthesisExpressions}' after an entity set '{identifier}'.");
                    }
                }

                return true;
            }
            else if (singleton != null)
            {
                path.Add(new SingletonSegmentTemplate(singleton));

                // can't append parenthesis after singleton
                if (parenthesisExpressions != null)
                {
                    throw new Exception($"Unknown parenthesis '{parenthesisExpressions}' after a singleton '{identifier}'.");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to bind the idenfier as operation import (function import or action import) segment,
        /// Append it into path.
        /// </summary>
        private static bool TryBindOperationImport(string identifier, string parenthesisExpressions,
            IEdmModel model, IList<ODataSegmentTemplate> path)
        {
            // split the parameter key/value pair
            parenthesisExpressions.ExtractKeyValuePairs(out IDictionary<string, string> parameters, out string remaining);
            IList<string> parameterNames = parameters == null ? null : parameters.Keys.ToList();

            IEdmOperationImport operationImport = OperationHelper.ResolveOperationImports(identifier, parameterNames, model, true);
            if (operationImport != null)
            {
                operationImport.TryGetStaticEntitySet(model, out IEdmEntitySetBase entitySetBase);
       //         path.Add(new OperationImportSegment(operationImport, entitySetBase, identifier));
                if (remaining != null && operationImport.IsFunctionImport())
                {
                    IEdmFunction function = (IEdmFunction)operationImport.Operation;
                    if (function.IsComposable)
                    {
                        if (TryBindKeySegment(parenthesisExpressions, path))
                        {
                            return true;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to bind the idenfier as property segment,
        /// Append it into path.
        /// </summary>
        private static bool TryBindPropertySegment(string identifier, string parenthesisExpressions, IEdmModel model,
            IList<ODataSegmentTemplate> path)
        {
            ODataSegmentTemplate preSegment = path.LastOrDefault();
            if (preSegment == null || !preSegment.IsSingle)
            {
                return false;
            }
            /*
            IEdmStructuredType structuredType = preSegment.EdmType as IEdmStructuredType;
            if (structuredType == null)
            {
                return false;
            }

            IEdmProperty property = structuredType.FindProperty(identifier);
            if (property == null)
            {
                return false;
            }

            ODataSegmentTemplate segment;
            if (property.PropertyKind == EdmPropertyKind.Navigation)
            {
                var navigationProperty = (IEdmNavigationProperty)property;

                IEdmNavigationSource navigationSource = null;
                if (preSegment.NavigationSource != null)
                {
                    IEdmPathExpression bindingPath;
                    navigationSource = preSegment.NavigationSource.FindNavigationTarget(navigationProperty, path, out bindingPath);
                }

                // Relationship between TargetMultiplicity and navigation property:
                //  1) EdmMultiplicity.Many <=> collection navigation property
                //  2) EdmMultiplicity.ZeroOrOne <=> nullable singleton navigation property
                //  3) EdmMultiplicity.One <=> non-nullable singleton navigation property
                segment = new NavigationSegmentTemplate(navigationProperty, navigationSource, identifier);
            }
            else
            {
                segment = new PropertySegmentTemplate((IEdmStructuralProperty)property);
            }

            path.Add(segment);

            if (parenthesisExpressions != null && !property.Type.IsCollection() && !property.Type.AsCollection().ElementType().IsEntity())
            {
                throw new Exception($"Invalid '{parenthesisExpressions}' after property '{identifier}'.");
            }

            TryBindKeySegment(parenthesisExpressions, path);*/
            return true;
        }

        /// <summary>
        /// Try to bind the parenthesisExpressions as key,
        /// parenthesisExpressions should have '(' and ')' wrapped.
        /// </summary>
        /// <param name="parenthesisExpressions">'(' and ')' wrapped string.</param>
        /// <param name="path">the decorated path.</param>
        internal static bool TryBindKeySegment(string parenthesisExpressions, IList<ODataSegmentTemplate> path)
        {
            // key segment can't be the first segment.
            // key segment only apply to collection.
            ODataSegmentTemplate preSegment = path.LastOrDefault();
            if (preSegment == null || preSegment.IsSingle || string.IsNullOrEmpty(parenthesisExpressions))
            {
                return false;
            }
/*
            IEdmEntityType targetEntityType;
            if (!preSegment.EdmType.TryGetEntityType(out targetEntityType))
            {
                // key segment only apply to collection of entity
                return false;
            }

            // Retrieve the key/value pairs
            parenthesisExpressions.ExtractKeyValuePairs(out IDictionary<string, string> retrievedkeys, out string remaining);
            var typeKeys = targetEntityType.Key().ToList();
            if (typeKeys.Count != retrievedkeys.Count)
            {
                // make sure the key count is same
                return false;
            }

            if (typeKeys.Count == 1)
            {
                string keyName = retrievedkeys.First().Key;
                if (keyName != String.Empty)
                {
                    if (typeKeys[0].Name != keyName)
                    {
                        return false;
                    }
                }
            }
            else
            {
                foreach (var items in typeKeys)
                {
                    if (!retrievedkeys.ContainsKey(items.Name))
                    {
                        return false;
                    }
                }
            }

            if (remaining != null)
            {
                // not allowed as (key=value)(....)
                throw new Exception($"Invalid key parathesis '{parenthesisExpressions}'.");
            }

            path.Add(new KeySegmentTemplate(retrievedkeys, targetEntityType, preSegment.NavigationSource));*/
            return true;
        }

        /// <summary>
        /// Try to bind namespace-qualified type cast segment.
        /// </summary>
        internal static bool TryBindTypeCastSegment(string identifier, string parenthesisExpressions, IEdmModel model,
            IList<ODataSegmentTemplate> path)
        {
            if (identifier == null || identifier.IndexOf('.') < 0)
            {
                // type cast should be namespace-qualified name
                return false;
            }

            ODataSegmentTemplate preSegment = path.LastOrDefault();
            if (preSegment == null)
            {
                // type cast should not be the first segment.
                return false;
            }

            IEdmSchemaType schemaType = model.ResolveType(identifier, true);
            if (schemaType == null)
            {
                return false;
            }

            IEdmType targetEdmType = schemaType as IEdmType;
            if (targetEdmType == null)
            {
                return false;
            }
            /*
            IEdmType previousEdmType = preSegment.EdmType;
            bool isNullable = false;
            if (previousEdmType.TypeKind == EdmTypeKind.Collection)
            {
                previousEdmType = ((IEdmCollectionType)previousEdmType).ElementType.Definition;
                isNullable = ((IEdmCollectionType)previousEdmType).ElementType.IsNullable;
            }

            if (!targetEdmType.IsOrInheritsFrom(previousEdmType) && !previousEdmType.IsOrInheritsFrom(targetEdmType))
            {
                throw new Exception($"Type cast {targetEdmType.FullTypeName()} has no relationship with previous {previousEdmType.FullTypeName()}.");
            }

            // We want the type of the type segment to be a collection if the previous segment was a collection
            IEdmType actualTypeOfTheTypeSegment = targetEdmType;
            if (preSegment.EdmType.TypeKind == EdmTypeKind.Collection)
            {
                var actualEntityTypeOfTheTypeSegment = targetEdmType as IEdmEntityType;
                if (actualEntityTypeOfTheTypeSegment != null)
                {
                    actualTypeOfTheTypeSegment = new EdmCollectionType(new EdmEntityTypeReference(actualEntityTypeOfTheTypeSegment, isNullable));
                }
                else
                {
                    // Complex collection supports type cast too.
                    var actualComplexTypeOfTheTypeSegment = actualTypeOfTheTypeSegment as IEdmComplexType;
                    if (actualComplexTypeOfTheTypeSegment != null)
                    {
                        actualTypeOfTheTypeSegment = new EdmCollectionType(new EdmComplexTypeReference(actualComplexTypeOfTheTypeSegment, isNullable));
                    }
                    else
                    {
                        throw new Exception($"Invalid type cast of {identifier}, it should be entity or complex.");
                    }
                }
            }

            TypeSegment typeCast = new TypeSegment(actualTypeOfTheTypeSegment, preSegment.EdmType, preSegment.NavigationSource, identifier);
            path.Add(typeCast);
            */
            TryBindKeySegment(parenthesisExpressions, path);

            return true;
        }

        /// <summary>
        /// Try to bind the idenfier as bound operation segment,
        /// Append it into path.
        /// </summary>
        internal static bool TryBindOperations(string identifier, string parenthesisExpressions,
            IEdmModel model, IList<ODataSegmentTemplate> path)
        {
            ODataSegmentTemplate preSegment = path.LastOrDefault();
            if (preSegment == null)
            {
                // bound operation cannot be the first segment.
                return false;
            }
            /*
            IEdmType bindingType = preSegment.EdmType;

            // operation
            parenthesisExpressions.ExtractKeyValuePairs(out IDictionary<string, string> parameters, out string remaining);
            IList<string> parameterNames = parameters == null ? null : parameters.Keys.ToList();

            IEdmOperation operation = OperationHelper.ResolveOperations(identifier, parameterNames, bindingType, model, settings.EnableCaseInsensitive);
            if (operation != null)
            {
                IEdmEntitySetBase targetset = null;
                if (operation.ReturnType != null)
                {
                    IEdmNavigationSource source = preSegment == null ? null : preSegment.NavigationSource;
                    targetset = operation.GetTargetEntitySet(source, model);
                }

                path.Add(new OperationSegmentTemplate(operation, targetset, identifier));

                if (remaining != null && operation.IsFunction())
                {
                    IEdmFunction function = (IEdmFunction)operation;
                    if (function.IsComposable)
                    {
                        // to process the ~/ .../ NS.Function(p1 ={ abc})({ id})
                        if (TryBindKeySegment(parenthesisExpressions, path))
                        {
                            return true;
                        }
                    }
                }

                return true;
            }
            */
            return false;
        }

        /// <summary>
        /// Parses the OData path into segments.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <returns>The segments of the OData path.</returns>
        internal static IEnumerable<string> ParseSegments(string odataPath)
        {
            string[] segments = odataPath.Split('/');

            foreach (string segment in segments)
            {
                int startIndex = 0;
                int openParensIndex = 0;
                bool insideParens = false;
                for (int i = 0; i < segment.Length; i++)
                {
                    switch (segment[i])
                    {
                        case '(':
                            openParensIndex = i;
                            insideParens = true;
                            break;
                        case ')':
                            if (insideParens)
                            {
                                if (openParensIndex > startIndex)
                                {
                                    yield return segment.Substring(startIndex, openParensIndex - startIndex);
                                }
                                if (i > openParensIndex + 1)
                                {
                                    // yield parentheses substring if there are any characters inside the parentheses
                                    yield return segment.Substring(openParensIndex, (i + 1) - openParensIndex);
                                }
                                startIndex = i + 1;
                                insideParens = false;
                            }
                            break;
                    }
                }

                if (startIndex < segment.Length)
                {
                    yield return segment.Substring(startIndex);
                }
            }
        }

        /// <summary>
        /// Parses the next OData path segment.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseNextSegment(IEdmModel model,
            ODataSegmentTemplate previous, IEdmType previousEdmType, string segment)
        {
            if (String.IsNullOrEmpty(segment))
            {
                throw new ArgumentNullException(nameof(segment));
            }

            if (previous == null)
            {
                // Parse first segment
                return ParseFirstSegment(model, segment);
            }
            else
            {
                // Parse non-first segment
                //if (previousEdmType == null)
                //{
                //    throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
                //}

                //switch (previousEdmType.TypeKind)
                //{
                //    case EdmTypeKind.Collection:
                //        return ParseAtCollection(model, previous, previousEdmType, segment);

                //    case EdmTypeKind.Entity:
                //        return ParseAtEntity(model, previous, previousEdmType, segment);

                //    case EdmTypeKind.Complex:
                //        return ParseAtComplex(model, previous, previousEdmType, segment);

                //    case EdmTypeKind.Primitive:
                //        return ParseAtPrimitiveProperty(model, previous, previousEdmType, segment);

                //    default:
                //        throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
                //}

                return null;
            }
        }

        /// <summary>
        /// Parses the first OData segment following the service base URI.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseFirstSegment(IEdmModel model, string segment)
        {
            if (segment == "$metadata")
            {
                return MetadataSegmentTemplate.Instance;
            }
            //else if (segment == ODataSegmentKinds.Batch)
            //{
            //    return new BatchPathSegment();
            //}

            IEdmEntityContainer container = model.EntityContainer;
            if (container == null)
            {
                return null;
            }

            IEdmEntitySet entitySet = container.FindEntitySet(segment);
            if (entitySet != null)
            {
                return new EntitySetSegmentTemplate(entitySet);
            }

            IEdmSingleton singleton = container.FindSingleton(segment);
            if (singleton != null)
            {
                return new SingletonSegmentTemplate(singleton);
            }

            IEdmOperationImport[] operationImports = container.FindOperationImports(segment).ToArray();
            if (operationImports.Length > 0)
            {
                if (operationImports.Length == 1)
                {
                    IEdmOperationImport operationImport = operationImports[0];
                    if (operationImport.IsActionImport())
                    {
                        return new ActionImportSegmentTemplate((IEdmActionImport)operationImport);
                    }
                    else
                    {
                        return new FunctionImportSegmentTemplate((IEdmFunctionImport)operationImport);
                    }
                }

                throw new InvalidOperationException($"Found mulitple operation import '{segment}' in one Edm container.");
            }

            // segment does not match the model
            return null;
        }


    }
}
