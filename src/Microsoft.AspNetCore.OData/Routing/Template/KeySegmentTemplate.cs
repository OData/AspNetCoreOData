// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
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
        /// Key/Value pairs:
        /// Key: entity type key name, for example ID
        /// Value: a tuple of (string, IEdmTypeReference): Item1 is the mapped name, Item2 is the key's type
        /// </summary>
        private IDictionary<string, (string, IEdmTypeReference)> _keyMappings { get; } = new Dictionary<string, (string, IEdmTypeReference)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="segment">The key segment.</param>
        public KeySegmentTemplate(KeySegment segment)
        {
            if (segment == null)
            {
                throw new ArgumentNullException(nameof(segment));
            }

            NavigationSource = segment.NavigationSource;
            EntityType = segment.EdmType as IEdmEntityType;

            var keys = BuildKeyMappings(segment.Keys);
            if (keys.Count == 1)
            {
                var first = keys.First();
                // {key}
                Literal = first.Value;
                var firstKey = EntityType.Key().First();

                _keyMappings[firstKey.Name] = (Literal, firstKey.Type);
            }
            else
            {
                // Id1={keyId1},Id2={keyId2}
                foreach (var key in keys)
                {
                    var entityKey = EntityType.Key().First(e => e.Name == key.Key);
                    _keyMappings[entityKey.Name] = (key.Value, entityKey.Type);
                }

                Literal = string.Join(",", _keyMappings.Select(a => $"{a.Key}={{{a.Value.Item1}}}"));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="entityType">The declaring type containes the key.</param>
        /// <param name="navigationSource"></param>
        public KeySegmentTemplate(IEdmEntityType entityType, IEdmNavigationSource navigationSource)
            : this(entityType, keyPrefix: "key")
        {
            NavigationSource = navigationSource;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="entityType">The declaring type containes the key.</param>
        public KeySegmentTemplate(IEdmEntityType entityType)
            : this(entityType, keyPrefix: "key")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="entityType">The declaring type containes the key.</param>
        /// <param name="keyPrefix">The prefix for the key mapping, for example, for the navigation it count be "relatedKey".</param>
        public KeySegmentTemplate(IEdmEntityType entityType, string keyPrefix)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));

            var keys = entityType.Key().ToArray();
            if (keys.Length == 1)
            {
                // {key}
                Literal = $"{{{keyPrefix}}}";
                _keyMappings[keys[0].Name] = ($"{keyPrefix}", keys[0].Type);
            }
            else
            {
                // Id1={keyId1},Id2={keyId2}
                foreach (var key in keys)
                {
                    _keyMappings[key.Name] = ($"{keyPrefix}{key.Name}", key.Type);
                }

                Literal = string.Join(",", _keyMappings.Select(a => $"{a.Key}={{{a.Value.Item1}}}"));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// </summary>
        /// <param name="keys">The input key mappings.</param>
        /// <param name="entityType">The declaring type containes the key.</param>
        /// <param name="navigationSource">The navigation source.</param>
        public KeySegmentTemplate(IDictionary<string, string> keys,
            IEdmEntityType entityType, IEdmNavigationSource navigationSource)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            NavigationSource = navigationSource ?? throw new ArgumentNullException(nameof(navigationSource));

            var entityTypeKeys = EntityType.Key();
            if (keys.Count != entityTypeKeys.Count())
            {
                throw new ODataException(string.Format(CultureInfo.CurrentCulture, SRResources.InputKeyNotMatchEntityTypeKey, keys.Count, entityTypeKeys.Count()));
            }

            if (keys.Count == 1)
            {
                KeyValuePair<string, string> key = keys.First();
                IEdmStructuralProperty keyProperty = entityTypeKeys.First();
                _keyMappings[keyProperty.Name] = (key.Value, keyProperty.Type);
                Literal = key.Value;
            }
            else
            {
                foreach (var key in keys)
                {
                    string keyName = key.Key;

                    IEdmStructuralProperty keyProperty;

                    keyProperty = entityType.Key().FirstOrDefault(k => k.Name == keyName);
                    if (keyProperty == null)
                    {
                        throw new InvalidOperationException($"Cannot find '{keyName}' key in the '{entityType.FullName()}' type.");
                    }

                    _keyMappings[keyName] = (key.Value, keyProperty.Type);
                }

                Literal = string.Join(",", _keyMappings.Select(a => $"{a.Key}={a.Value.Item1}"));
            }
        }

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
        public int Count => _keyMappings.Count;

        /// <inheritdoc />
        public override ODataSegmentKind Kind => ODataSegmentKind.Key;

        /// <inheritdoc />
        public override bool IsSingle => true;

        /// <inheritdoc />
        public override ODataPathSegment Translate(ODataTemplateTranslateContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            RouteValueDictionary routeValues = context.RouteValues;
            IDictionary<string, object> keysValues = new Dictionary<string, object>();
            foreach (var key in _keyMappings)
            {
                string keyName = key.Key;
                string templateName = key.Value.Item1;
                IEdmTypeReference edmType = key.Value.Item2;
                if (routeValues.TryGetValue(templateName, out object rawValue))
                {
                    string strValue = rawValue as string;
                    object newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, context.Model, edmType);

                    // for without FromODataUri, so update it, for example, remove the single quote for string value.
                    routeValues[templateName] = newValue;

                    // For FromODataUri
                    string prefixName = ODataParameterValue.ParameterValuePrefix + templateName;
                    routeValues[prefixName] = new ODataParameterValue(newValue, edmType);

                    keysValues[keyName] = newValue;
                }
            }

            return new KeySegment(keysValues, EntityType, NavigationSource);
        }

        internal static IDictionary<string, string> BuildKeyMappings(IEnumerable<KeyValuePair<string, object>> keys)
        {
            Contract.Assert(keys != null);

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

            foreach (KeyValuePair<string, object> key in keys)
            {
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

                if (nameInRouteData == null || !IsRouteParameter(nameInRouteData))
                {
                    throw new ODataException(
                        Error.Format(SRResources.KeyTemplateMustBeInCurlyBraces, key.Value, key.Key));
                }

                //nameInRouteData = nameInRouteData.Substring(1, nameInRouteData.Length - 2);
                //if (String.IsNullOrEmpty(nameInRouteData))
                //{
                //    throw new ODataException(
                //            Error.Format(SRResources.EmptyKeyTemplate, key.Value, key.Key));
                //}

                parameterMappings[key.Key] = nameInRouteData;
            }

            return parameterMappings;
        }

        private static bool IsRouteParameter(string parameterName)
        {
            return parameterName.StartsWith("{", StringComparison.Ordinal) &&
                    parameterName.EndsWith("}", StringComparison.Ordinal);
        }
    }
}
