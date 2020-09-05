// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.OData.Edm;
using System.Diagnostics.Contracts;
using Microsoft.OData;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Parser
{
    /// <summary>
    /// Exposes the ability to parse an OData path template as an <see cref="ODataPathTemplate"/>.
    /// </summary>
    public class DefaultODataPathTemplateParser : IODataPathTemplateParser
    {
        /// <summary>
        /// Parse the string like "/users/{id}/contactFolders/{contactFolderId}/contacts"
        /// to segments
        /// </summary>
        /// <param name="model">the Edm model.</param>
        /// <param name="odataPath">the setting.</param>
        /// <param name="requestProvider">The service provider.</param>
        /// <returns>Null or <see cref="ODataPathTemplate"/>.</returns>
        public virtual ODataPathTemplate Parse(IEdmModel model, string odataPath, IServiceProvider requestProvider)
        {
            if (model == null || string.IsNullOrEmpty(odataPath))
            {
                return null;
            }

            ODataUriParser uriParser;
            if (requestProvider == null)
            {
                uriParser = new ODataUriParser(model, new Uri(odataPath, UriKind.Relative));
            }
            else
            {
                uriParser = new ODataUriParser(model, new Uri(odataPath, UriKind.Relative), requestProvider);
            }

            uriParser.EnableUriTemplateParsing = true;

            uriParser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash; // support key in paraenthese and key as segment.

            ODataPath path = uriParser.ParsePath();

            return Templatify(path);
        }

        private static ODataPathTemplate Templatify(ODataPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            ODataPathSegmentTemplateTranslator translator = new ODataPathSegmentTemplateTranslator();

            var templates = path.WalkWith(translator);

            return new ODataPathTemplate(templates);
        }

        /// <summary>
        /// Parse the string like "/users/{id}/contactFolders/{contactFolderId}/contacts"
        /// to segments
        /// </summary>
        /// <param name="model">the Edm model.</param>
        /// <param name="odataPath">the setting.</param>
        /// <returns>Null or <see cref="ODataPathTemplate"/>.</returns>
        public virtual ODataPathTemplate Parse(IEdmModel model, string odataPath)
        {
            if (model == null || string.IsNullOrEmpty(odataPath))
            {
                return null;
            }

            string[] items = odataPath.Split('/');
            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
            foreach (string item in items)
            {
                string trimedItem = item.Trim();
                if (string.IsNullOrEmpty(trimedItem))
                {
                    // skip //
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
        internal static void CreateFirstSegment(string identifier, IEdmModel model, IList<ODataSegmentTemplate> path)
        {
            Contract.Assert(model != null);
            Contract.Assert(path != null);

            if (IdentifierIs("$metadata", identifier))
            {
                path.Add(MetadataSegmentTemplate.Instance);
                return;
            }

            // The identifier maybe include the key, for example: users({id})
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

            throw new ODataException($"Unknown kind of first segment: '{identifier}'");
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
        /// Try to bind the identifier as operation import (function import or action import) segment,
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

                //IEdmNavigationSource navigationSource = null;
                //if (preSegment.NavigationSource != null)
                //{
                //    IEdmPathExpression bindingPath;
                //    navigationSource = preSegment.NavigationSource.FindNavigationTarget(navigationProperty, path, out bindingPath);
                //}

                // Relationship between TargetMultiplicity and navigation property:
                //  1) EdmMultiplicity.Many <=> collection navigation property
                //  2) EdmMultiplicity.ZeroOrOne <=> nullable singleton navigation property
                //  3) EdmMultiplicity.One <=> non-nullable singleton navigation property
                segment = new NavigationSegmentTemplate(navigationProperty/*, navigationSource, identifier*/);
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

            path.Add(new KeySegmentTemplate(retrievedkeys, targetEntityType, preSegment.NavigationSource));
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
            
            CastSegmentTemplate typeCast = new CastSegmentTemplate(actualTypeOfTheTypeSegment, preSegment.EdmType, preSegment.NavigationSource);
            path.Add(typeCast);

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

            IEdmType bindingType = preSegment.EdmType;

            // operation
            parenthesisExpressions.ExtractKeyValuePairs(out IDictionary<string, string> parameters, out string remaining);
            IList<string> parameterNames = parameters == null ? null : parameters.Keys.ToList();

            IEdmOperation operation = OperationHelper.ResolveOperations(identifier, parameterNames, bindingType, model, true);
            if (operation != null)
            {
                IEdmEntitySetBase targetset = null;
                if (operation.ReturnType != null)
                {
                    IEdmNavigationSource source = preSegment == null ? null : preSegment.NavigationSource;
                    targetset = operation.GetTargetEntitySet(source, model);
                }

                if (operation.IsFunction())
                {
                    path.Add(new FunctionSegmentTemplate((IEdmFunction)operation, targetset));
                }
                else
                {
                    path.Add(new ActionSegmentTemplate((IEdmAction)operation));
                }

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

            return false;
        }

        /// <summary>
        /// Check whether identifiers matches according to case in sensitive option.
        /// </summary>
        /// <param name="expected">The expected identifier.</param>
        /// <param name="identifier">Identifier to be evaluated.</param>
        /// <returns>Whether the identifier matches.</returns>
        private static bool IdentifierIs(string expected, string identifier)
        {
            return string.Equals(expected, identifier, StringComparison.OrdinalIgnoreCase);
        }
    }
}
