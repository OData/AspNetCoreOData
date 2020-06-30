// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// Parse the OData routing template
    /// </summary>
    public class DefaultODataPathTemplateParser : IODataPathTemplateParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="odataPath"></param>
        /// <returns></returns>
        public virtual ODataPathTemplate Parse(IEdmModel model, string odataPath)
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
                previousEdmType = pathSegment.GetEdmType(previousEdmType);
            }

            return new ODataPathTemplate(pathSegments);
        }

        /// <summary>
        /// Parse the string like "/users/{id | userPrincipalName}/contactFolders/{contactFolderId}/contacts"
        /// to segments
        /// </summary>
        /// <param name="model">the IEdm model.</param>
        /// <param name="odataPath">the setting.</param>
        /// <returns>Null or <see cref="UriParser"/>.</returns>
        public static ODataPathTemplate Parse(IEdmModel model, string odataPath)
        {
            if (model == null || settings == null || String.IsNullOrEmpty(requestUri))
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
                    CreateFirstSegment(trimedItem, model, segments, settings);
                }
                else
                {
                    CreateNextSegment(trimedItem, model, segments, settings);
                }
            }

            return new UriPath(segments);
        }

        /// <summary>
        /// Process the first segment in the request uri.
        /// The first segment could be only singleton/entityset/operationimport, doesn't consider the $metadata, $batch
        /// </summary>
        /// <param name="identifier">the whole identifier of this segment</param>
        /// <param name="model">The Edm model.</param>
        /// <param name="path">The out put of the path, because it may include the key segment.</param>
        /// <param name="settings">The Uri parser settings</param>
        internal static void CreateFirstSegment(string identifier, IEdmModel model,
            IList<ODataSegmentTemplate> path, PathParserSettings settings)
        {
            // the identifier maybe include the key, for example: ~/users({id})
            identifier = identifier.ExtractParenthesis(out string parenthesisExpressions);

            // Try to bind entity set or singleton
            if (TryBindNavigationSource(identifier, parenthesisExpressions, model, path, settings))
            {
                return;
            }

            // Try to bind operation import
            if (TryBindOperationImport(identifier, parenthesisExpressions, model, path, settings))
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
        /// <param name="settings">The parser settings</param>
        internal static void CreateNextSegment(string identifier, IEdmModel model, IList<ODataSegmentTemplate> path, PathParserSettings settings)
        {
            // GET /Users/{id}
            // GET /Users({id})
            // GET /me/outlook/supportedTimeZones(TimeZoneStandard=microsoft.graph.timeZoneStandard'{timezone_format}')

            // maybe key or function parameters
            identifier = identifier.ExtractParenthesis(out string parenthesisExpressions);

            // can be "property, navproperty"
            if (TryBindPropertySegment(identifier, parenthesisExpressions, model, path, settings))
            {
                return;
            }

            // bind to type cast.
            if (TryBindTypeCastSegment(identifier, parenthesisExpressions, model, path, settings))
            {
                return;
            }

            // bound operations
            if (TryBindOperations(identifier, parenthesisExpressions, model, path, settings))
            {
                return;
            }

            // Handle Key as Segment
            if (TryBindKeySegment("(" + identifier + ")", path))
            {
                return;
            }

            throw new Exception($"Unknown kind of segment: '{identifier}', previous segment: '{path.Last().Identifier}'.");
        }

        /// <summary>
        /// Try to bind the idenfier as navigation source segment,
        /// Append it into path.
        /// </summary>
        internal static bool TryBindNavigationSource(string identifier,
            string parenthesisExpressions, // the potention parenthesis expression after identifer
            IEdmModel model,
            IList<PathSegment> path, PathParserSettings settings)
        {
            IEdmNavigationSource source = model.ResolveNavigationSource(identifier, settings.EnableCaseInsensitive);
            IEdmEntitySet entitySet = source as IEdmEntitySet;
            IEdmSingleton singleton = source as IEdmSingleton;

            if (entitySet != null)
            {
                path.Add(new EntitySetSegment(entitySet, identifier));

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
                path.Add(new SingletonSegment(singleton, identifier));

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
        /// Try to bind the idenfier as property segment,
        /// Append it into path.
        /// </summary>
        private static bool TryBindPropertySegment(string identifier, string parenthesisExpressions, IEdmModel model,
            IList<PathSegment> path,
            PathParserSettings settings)
        {
            PathSegment preSegment = path.LastOrDefault();
            if (preSegment == null || !preSegment.IsSingle)
            {
                return false;
            }

            IEdmStructuredType structuredType = preSegment.EdmType as IEdmStructuredType;
            if (structuredType == null)
            {
                return false;
            }

            IEdmProperty property = structuredType.ResolveProperty(identifier, settings.EnableCaseInsensitive);
            if (property == null)
            {
                return false;
            }

            PathSegment segment;
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
                segment = new NavigationSegment(navigationProperty, navigationSource, identifier);
            }
            else
            {
                segment = new PropertySegment((IEdmStructuralProperty)property, preSegment.NavigationSource, identifier);
            }

            path.Add(segment);

            if (parenthesisExpressions != null && !property.Type.IsCollection() && !property.Type.AsCollection().ElementType().IsEntity())
            {
                throw new Exception($"Invalid '{parenthesisExpressions}' after property '{identifier}'.");
            }

            TryBindKeySegment(parenthesisExpressions, path);
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

            path.Add(new KeySegment(retrievedkeys, targetEntityType, preSegment.NavigationSource));
            return true;
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
                if (previousEdmType == null)
                {
                    throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
                }

                switch (previousEdmType.TypeKind)
                {
                    case EdmTypeKind.Collection:
                        return ParseAtCollection(model, previous, previousEdmType, segment);

                    case EdmTypeKind.Entity:
                        return ParseAtEntity(model, previous, previousEdmType, segment);

                    case EdmTypeKind.Complex:
                        return ParseAtComplex(model, previous, previousEdmType, segment);

                    case EdmTypeKind.Primitive:
                        return ParseAtPrimitiveProperty(model, previous, previousEdmType, segment);

                    default:
                        throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
                }
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
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

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

        /// <summary>
        /// Parses the next OData path segment following a collection.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseAtCollection(IEdmModel model, ODataSegmentTemplate previous,
            IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw new ArgumentNullException(nameof(previous));
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw new ArgumentNullException(nameof(segment));
            }

            if (previousEdmType == null)
            {
                throw new ArgumentNullException(nameof(previousEdmType));
            }

            IEdmCollectionType collection = previousEdmType as IEdmCollectionType;
            //if (collection == null)
            //{
            //    throw Error.Argument(SRResources.PreviousSegmentMustBeCollectionType, previousEdmType);
            //}

            switch (collection.ElementType.Definition.TypeKind)
            {
                case EdmTypeKind.Entity:
                    return ParseAtEntityCollection(model, previous, previousEdmType, segment);

                default:
                    throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
            }
        }

        /// <summary>
        /// Parses the next OData path segment following a complex-typed segment.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseAtComplex(IEdmModel model, ODataSegmentTemplate previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            IEdmComplexType previousType = previousEdmType as IEdmComplexType;
            if (previousType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeComplexType, previousEdmType);
            }

            // look for properties
            IEdmProperty property = previousType.Properties().SingleOrDefault(p => p.Name == segment);
            if (property != null)
            {
                return new PropertyAccessPathSegment(property);
            }

            // Treating as an open property
            return new UnresolvedPathSegment(segment);
        }

        /// <summary>
        /// Parses the next OData path segment following an entity collection.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseAtEntityCollection(IEdmModel model, ODataSegmentTemplate previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (previousEdmType == null)
            {
                throw Error.InvalidOperation(SRResources.PreviousSegmentEdmTypeCannotBeNull);
            }
            IEdmCollectionType collectionType = previousEdmType as IEdmCollectionType;
            if (collectionType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeEntityCollectionType, previousEdmType);
            }
            IEdmEntityType elementType = collectionType.ElementType.Definition as IEdmEntityType;
            if (elementType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeEntityCollectionType, previousEdmType);
            }

            // look for keys first.
            if (segment.StartsWith("(", StringComparison.Ordinal) && segment.EndsWith(")", StringComparison.Ordinal))
            {
                Contract.Assert(segment.Length >= 2);
                string value = segment.Substring(1, segment.Length - 2);
                return new KeyValuePathSegment(value);
            }

            // next look for casts
            IEdmEntityType castType = model.FindDeclaredType(segment) as IEdmEntityType;
            if (castType != null)
            {
                IEdmType previousElementType = collectionType.ElementType.Definition;
                if (!castType.IsOrInheritsFrom(previousElementType) && !previousElementType.IsOrInheritsFrom(castType))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidCastInPath, castType, previousElementType));
                }
                return new CastPathSegment(castType);
            }

            // now look for bindable actions
            IEdmEntityContainer container = model.EntityContainer;
            IEdmFunctionImport procedure = container.FunctionImports().FindBindableAction(collectionType, segment);
            if (procedure != null)
            {
                return new ActionPathSegment(procedure);
            }

            throw new ODataException(Error.Format(SRResources.NoActionFoundForCollection, segment, collectionType.ElementType));
        }

        /// <summary>
        /// Parses the next OData path segment following a primitive property.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseAtPrimitiveProperty(IEdmModel model, ODataSegmentTemplate previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }

            if (segment == ODataSegmentKinds.Value)
            {
                return new ValuePathSegment();
            }

            throw new ODataException(Error.Format(SRResources.InvalidPathSegment, segment, previous));
        }

        /// <summary>
        /// Parses the next OData path segment following an entity.
        /// </summary>
        /// <param name="model">The model to use for path parsing.</param>
        /// <param name="previous">The previous path segment.</param>
        /// <param name="previousEdmType">The EDM type of the OData path up to the previous segment.</param>
        /// <param name="segment">The value of the segment to parse.</param>
        /// <returns>A parsed representation of the segment.</returns>
        protected virtual ODataSegmentTemplate ParseAtEntity(IEdmModel model, ODataSegmentTemplate previous, IEdmType previousEdmType, string segment)
        {
            if (previous == null)
            {
                throw Error.ArgumentNull("previous");
            }
            if (String.IsNullOrEmpty(segment))
            {
                throw Error.Argument(SRResources.SegmentNullOrEmpty);
            }
            IEdmEntityType previousType = previousEdmType as IEdmEntityType;
            if (previousType == null)
            {
                throw Error.Argument(SRResources.PreviousSegmentMustBeEntityType, previousEdmType);
            }

            if (segment == ODataSegmentKinds.Links)
            {
                return new LinksPathSegment();
            }

            // first look for navigation properties
            IEdmNavigationProperty navigation = previousType.NavigationProperties().SingleOrDefault(np => np.Name == segment);
            if (navigation != null)
            {
                return new NavigationPathSegment(navigation);
            }

            // next look for properties
            IEdmProperty property = previousType.Properties().SingleOrDefault(p => p.Name == segment);
            if (property != null)
            {
                return new PropertyAccessPathSegment(property);
            }

            // next look for type casts
            IEdmEntityType castType = model.FindDeclaredType(segment) as IEdmEntityType;
            if (castType != null)
            {
                if (!castType.IsOrInheritsFrom(previousType) && !previousType.IsOrInheritsFrom(castType))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidCastInPath, castType, previousType));
                }
                return new CastPathSegment(castType);
            }

            // finally look for bindable procedures
            IEdmEntityContainer container = model.EntityContainer;
            IEdmFunctionImport procedure = container.FunctionImports().FindBindableAction(previousType, segment);
            if (procedure != null)
            {
                return new ActionPathSegment(procedure);
            }

            // Treating as an open property
            return new UnresolvedPathSegment(segment);
        }
    }
}
