// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Template
{
    /// <summary>
    /// Represents a template that could match a key segment.
    /// </summary>
    public class KeySegmentTemplate : ODataSegmentTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="keys">The input key mappings, the key string is case-sensitive, the value string should wrapper with { and }.</param>
        /// <param name="entityType">The declaring type containes the key.</param>
        /// <param name="navigationSource">The navigation source. It could be null.</param>
        public KeySegmentTemplate(IDictionary<string, string> keys, IEdmEntityType entityType, IEdmNavigationSource navigationSource)
        {
            if (keys == null)
            {
                throw Error.ArgumentNull(nameof(keys));
            }

            EntityType = entityType ?? throw Error.ArgumentNull(nameof(entityType));
            NavigationSource = navigationSource;

            KeyMappings = BuildKeyMappings(keys.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)), entityType);

            Literal = KeyMappings.Count == 1 ?
                $"{{{KeyMappings.First().Value}}}" :
                string.Join(",", KeyMappings.Select(a => $"{a.Key}={{{a.Value}}}"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The key segment, it should be a template key segment.</param>
        public KeySegmentTemplate(KeySegment segment)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            NavigationSource = segment.NavigationSource;
            EntityType = segment.EdmType as IEdmEntityType;

            KeyMappings = BuildKeyMappings(segment.Keys, EntityType);

            Literal = KeyMappings.Count == 1 ?
                $"{{{KeyMappings.First().Value}}}" :
                string.Join(",", KeyMappings.Select(a => $"{a.Key}={{{a.Value}}}"));
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the key names in the current key segment to the 
        /// key names in route data.
        /// the key in dict could be the string used in request
        /// the value in dict could be the string used in action of controller
        /// </summary>
        public IDictionary<string, string> KeyMappings { get; }

        /// <inheritdoc />
        public override string Literal { get; }

        /// <inheritdoc />
        public override IEdmType EdmType => EntityType;

        /// <inheritdoc />
        public override IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the entity type declaring this key.
        /// </summary>
        public IEdmEntityType EntityType { get; }

        /// <summary>
        /// Gets the key count
        /// </summary>
        public int Count => KeyMappings.Count;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Key;

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            RouteValueDictionary routeValues = context.RouteValues;
            RouteValueDictionary updateValues = context.UpdatedValues;

            IDictionary<string, object> keysValues = new Dictionary<string, object>();
            foreach (var key in KeyMappings)
            {
                string keyName = key.Key;
                string templateName = key.Value;

                IEdmProperty keyProperty = EntityType.Key().FirstOrDefault(k => k.Name == keyName);
                Contract.Assert(keyProperty != null);

                IEdmTypeReference edmType = keyProperty.Type;
                if (routeValues.TryGetValue(templateName, out object rawValue))
                {
                    string strValue = rawValue as string;
                    string newStrValue = context.GetParameterAliasOrSelf(strValue);
                    if (newStrValue != strValue)
                    {
                        updateValues[templateName] = newStrValue;
                        strValue = newStrValue;
                    }

                    object newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, context.Model, edmType);

                    // for non FromODataUri, so update it, for example, remove the single quote for string value.
                    updateValues[templateName] = newValue;

                    // For FromODataUri, let's refactor it later.
                    string prefixName = ODataParameterValue.ParameterValuePrefix + templateName;
                    updateValues[prefixName] = new ODataParameterValue(newValue, edmType);

                    keysValues[keyName] = newValue;
                }
            }

            return new KeySegment(keysValues, EntityType, NavigationSource);
        }

        /// <summary>
        /// Create <see cref="KeySegmentTemplate"/> based on the given entity type and navigation source.
        /// </summary>
        /// <param name="entityType">The given entity type.</param>
        /// <param name="navigationSource">The given navigation source.</param>
        /// <param name="keyPrefix">The prefix used before key template.</param>
        /// <returns>The built <see cref="KeySegmentTemplate"/>.</returns>
        internal static KeySegmentTemplate CreateKeySegment(IEdmEntityType entityType, IEdmNavigationSource navigationSource, string keyPrefix = "key")
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull(nameof(entityType));
            }

            IDictionary<string, string> keyTemplates = new Dictionary<string, string>();
            var keys = entityType.Key().ToArray();
            if (keys.Length == 1)
            {
                // Id={key}
                keyTemplates[keys[0].Name] = $"{{{keyPrefix}}}";
            }
            else
            {
                // Id1={keyId1},Id2={keyId2}
                foreach (var key in keys)
                {
                    keyTemplates[key.Name] = $"{{{keyPrefix}{key.Name}}}";
                }
            }

            return new KeySegmentTemplate(keyTemplates, entityType, navigationSource);
        }

        internal static IDictionary<string, string> BuildKeyMappings(IEnumerable<KeyValuePair<string, object>> keys, IEdmEntityType entityType)
        {
            Contract.Assert(keys != null);
            Contract.Assert(entityType != null);

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

            int count = keys.Count();
            ISet<string> entityTypeKeys = entityType.Key().Select(c => c.Name).ToHashSet();
            if (count != entityTypeKeys.Count)
            {
                throw new ODataException(Error.Format(SRResources.InputKeyNotMatchEntityTypeKey, count, entityTypeKeys.Count));
            }

            foreach (KeyValuePair<string, object> key in keys)
            {
                string keyName = key.Key;

                // key name is case-sensitive
                if (!entityTypeKeys.Contains(key.Key))
                {
                    throw new ODataException(Error.Format(SRResources.CannotFindKeyInEntityType, keyName, entityType.FullName()));
                }

                string nameInRouteData;

                UriTemplateExpression uriTemplateExpression = key.Value as UriTemplateExpression;
                if (uriTemplateExpression != null)
                {
                    nameInRouteData = uriTemplateExpression.LiteralText.Trim();
                }
                else
                {
                    // just for easy construct the key segment template
                    // it must start with "{" and end with "}"
                    nameInRouteData = key.Value as string;
                }

                if (nameInRouteData == null || !nameInRouteData.IsValidTemplateLiteral())
                {
                    throw new ODataException(Error.Format(SRResources.KeyTemplateMustBeInCurlyBraces, key.Value, key.Key));
                }

                nameInRouteData = nameInRouteData.Substring(1, nameInRouteData.Length - 2);
                if (string.IsNullOrEmpty(nameInRouteData))
                {
                    throw new ODataException(Error.Format(SRResources.EmptyKeyTemplate, key.Value, key.Key));
                }

                parameterMappings[key.Key] = nameInRouteData;
            }

            return parameterMappings;
        }
    }
}
