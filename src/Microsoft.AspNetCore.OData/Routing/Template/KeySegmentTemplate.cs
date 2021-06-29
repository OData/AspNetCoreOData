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
        private readonly string _keyLiteral;

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
            KeyProperties = EntityType.Key().ToDictionary(k => k.Name, k => (IEdmProperty)k);

            KeyMappings = BuildKeyMappings(keys.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value)), entityType, KeyProperties);

            _keyLiteral = KeyMappings.Count == 1 ?
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
            KeyProperties = EntityType.Key().ToDictionary(k => k.Name, k => (IEdmProperty)k);

            KeyMappings = BuildKeyMappings(segment.Keys, EntityType, KeyProperties);

            _keyLiteral = KeyMappings.Count == 1 ?
                $"{{{KeyMappings.First().Value}}}" :
                string.Join(",", KeyMappings.Select(a => $"{a.Key}={{{a.Value}}}"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeySegmentTemplate" /> class.
        /// Typically, it's for alternate key scenario.
        /// </summary>
        /// <param name="segment">The key segment, it should be a template key segment.</param>
        /// <param name="keyProperties">The key properties, the key is the alias,
        /// the value is the property list, it could be a property from the complex property. For example: address/city.</param>
        public KeySegmentTemplate(KeySegment segment, IDictionary<string, IEdmProperty> keyProperties)
        {
            if (segment == null)
            {
                throw Error.ArgumentNull(nameof(segment));
            }

            if (keyProperties == null)
            {
                throw Error.ArgumentNull(nameof(keyProperties));
            }

            NavigationSource = segment.NavigationSource;
            EntityType = segment.EdmType as IEdmEntityType;
            KeyProperties = keyProperties;

            KeyMappings = BuildKeyMappings(segment.Keys, EntityType, keyProperties);

            _keyLiteral = string.Join(",", KeyMappings.Select(a => $"{a.Key}={{{a.Value}}}"));
        }

        /// <summary>
        /// Gets the dictionary representing the mappings from the key names in the current key segment to the 
        /// key names in route data.
        /// the key in dict could be the string used in request
        /// the value in dict could be the string used in action of controller
        /// </summary>
        public IDictionary<string, string> KeyMappings { get; }

        /// <summary>
        /// Gets the keys.
        /// The key of dictionary is the key name or alias.
        /// The value of dictionary is the key property, it could be property on entity type or sub property on complex property.
        /// </summary>
        public IDictionary<string, IEdmProperty> KeyProperties { get; }

        /// <inheritdoc />
        public IEdmNavigationSource NavigationSource { get; }

        /// <summary>
        /// Gets the entity type declaring this key.
        /// </summary>
        public IEdmEntityType EntityType { get; }

        /// <summary>
        /// Gets the key count
        /// </summary>
        public int Count => KeyMappings.Count;

        /// <inheritdoc />
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            options = options ?? ODataRouteOptions.Default;

            Contract.Assert(options.EnableKeyInParenthesis || options.EnableKeyAsSegment);

            if (options.EnableKeyInParenthesis && options.EnableKeyAsSegment)
            {
                yield return $"({_keyLiteral})";
                yield return $"/{_keyLiteral}";
            }
            else if (options.EnableKeyInParenthesis)
            {
                yield return $"({_keyLiteral})";
            }
            else if (options.EnableKeyAsSegment)
            {
                yield return $"/{_keyLiteral}";
            }
        }

        /// <inheritdoc />
        public override bool TryTranslate(ODataTemplateTranslateContext context)
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
                if (routeValues.TryGetValue(templateName, out object rawValue))
                {
                    IEdmProperty keyProperty = KeyProperties.FirstOrDefault(k => k.Key == keyName).Value;
                    Contract.Assert(keyProperty != null);

                    IEdmTypeReference edmType = keyProperty.Type;
                    string strValue = rawValue as string;
                    string newStrValue = context.GetParameterAliasOrSelf(strValue);
                    if (newStrValue != strValue)
                    {
                        updateValues[templateName] = newStrValue;
                        strValue = newStrValue;
                    }

                    // If it's key as segment and the key type is Edm.String, we support non-single quoted string.
                    // Since we can't identify key as segment and key in parenthesis easy so far,
                    // we use the key literal with "/" to test in the whole route template.
                    // Why we can't create two key segment templates, one reason is that in attribute routing template,
                    // we can't identify key as segment or key in parenthesis also.
                    if (edmType.IsString() && context.IsPartOfRouteTemplate($"/{_keyLiteral}"))
                    {
                        if (!strValue.StartsWith('\'') && !strValue.EndsWith('\''))
                        {
                            strValue = $"'{strValue}'"; // prefix and suffix single quote
                        }
                    }

                    object newValue;
                    try
                    {
                        newValue = ODataUriUtils.ConvertFromUriLiteral(strValue, ODataVersion.V4, context.Model, edmType);
                    }
                    catch (ODataException ex)
                    {
                        string message = Error.Format(SRResources.InvalidKeyInUriFound, strValue, edmType.FullName());
                        throw new ODataException(message, ex);
                    }

                    // for non FromODataUri, so update it, for example, remove the single quote for string value.
                    updateValues[templateName] = newValue;

                    // For FromODataUri, let's refactor it later.
                    string prefixName = ODataParameterValue.ParameterValuePrefix + templateName;
                    updateValues[prefixName] = new ODataParameterValue(newValue, edmType);

                    keysValues[keyName] = newValue;
                }
            }

            context.Segments.Add(new KeySegment(keysValues, EntityType, NavigationSource));
            return true;
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
            return BuildKeyMappings(keys, entityType, entityType.Key().ToDictionary(k => k.Name, k => (IEdmProperty)k));
        }

        /// <summary>
        /// Build the key mappings
        /// </summary>
        /// <param name="keys">The Uri template parsing result. for example: SSN={ssnKey}</param>
        /// <param name="entityType">The Edm entity type.</param>
        /// <param name="keyProperties">The key properties.</param>
        /// <returns>The mapping</returns>
        internal static IDictionary<string, string> BuildKeyMappings(IEnumerable<KeyValuePair<string, object>> keys,
            IEdmEntityType entityType, IDictionary<string, IEdmProperty> keyProperties)
        {
            Contract.Assert(keys != null);
            Contract.Assert(entityType != null);
            Contract.Assert(keyProperties != null);

            Dictionary<string, string> parameterMappings = new Dictionary<string, string>();

            int count = keys.Count();
            if (count != keyProperties.Count)
            {
                throw new ODataException(Error.Format(SRResources.InputKeyNotMatchEntityTypeKey, count, keyProperties.Count, entityType.FullName()));
            }

            // keys have:  SSN={ssn},Name={name}
            // the key "SSN" or "Name" is the alias in alternate keys
            foreach (KeyValuePair<string, object> key in keys)
            {
                string keyName = key.Key;

                // key name is case-sensitive
                if (!keyProperties.ContainsKey(key.Key))
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
