// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class NavigationRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual int Order => 500;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // structural property supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ActionModel action = context.Action;

            if (context.EntitySet == null && context.Singleton == null)
            {
                return false;
            }
            IEdmNavigationSource navigationSource = context.EntitySet == null ?
                (IEdmNavigationSource)context.Singleton :
                (IEdmNavigationSource)context.EntitySet;

            string actionName = action.ActionMethod.Name;

            string method = Split(actionName, out string property, out string cast, out string declared);
            if (method == null || string.IsNullOrEmpty(property))
            {
                return false;
            }

            IEdmEntityType entityType = navigationSource.EntityType();
            IEdmModel model = context.Model;
            string prefix = context.Prefix;
            IEdmEntityType declaredEntityType = null;
            if (declared != null)
            {
                declaredEntityType = entityType.FindTypeInInheritance(model, declared) as IEdmEntityType;
                if (declaredEntityType == null)
                {
                    return false;
                }

                if (declaredEntityType == entityType)
                {
                    declaredEntityType = null;
                }
            }

            bool hasKeyParameter = HasKeyParameter(entityType, action);
            IEdmSingleton singleton = navigationSource as IEdmSingleton;
            if (singleton != null && hasKeyParameter)
            {
                // Singleton, doesn't allow to query property with key
                return false;
            }

            if (singleton == null && !hasKeyParameter)
            {
                // in entityset, doesn't allow for non-key to query property
                return false;
            }

            IEdmProperty edmProperty = entityType.FindProperty(property);
            if (edmProperty != null && edmProperty.PropertyKind == EdmPropertyKind.Structural)
            {
                // only process structural property
                IEdmStructuredType castComplexType = null;
                if (cast != null)
                {
                    IEdmTypeReference propertyType = edmProperty.Type;
                    if (propertyType.IsCollection())
                    {
                        propertyType = propertyType.AsCollection().ElementType();
                    }
                    if (!propertyType.IsComplex())
                    {
                        return false;
                    }

                    castComplexType = propertyType.ToStructuredType().FindTypeInInheritance(model, cast);
                    if (castComplexType == null)
                    {
                        return false;
                    }
                }

                IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();

                if (context.EntitySet != null)
                {
                    segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                }
                else
                {
                    segments.Add(new SingletonSegmentTemplate(context.Singleton));
                }

                if (hasKeyParameter)
                {
                    segments.Add(new KeySegmentTemplate(entityType));
                }
                if (declaredEntityType != null && declaredEntityType != entityType)
                {
                    segments.Add(new CastSegmentTemplate(declaredEntityType));
                }

                segments.Add(new PropertySegmentTemplate((IEdmStructuralProperty)edmProperty));

                ODataPathTemplate template = new ODataPathTemplate(segments);
                action.AddSelector(prefix, model, template);
                return true;
            }
            else
            {
                // map to a static action like:  <method>Property(int key, string property)From<...>
                if (property == "Property" && cast == null)
                {
                    if (action.Parameters.Any(p => p.ParameterInfo.Name == "property" && p.ParameterType == typeof(string)))
                    {
                        // we find a static method mapping for all property
                        // we find a action route
                        IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();

                        if (context.EntitySet != null)
                        {
                            segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                        }
                        else
                        {
                            segments.Add(new SingletonSegmentTemplate(context.Singleton));
                        }

                        if (hasKeyParameter)
                        {
                            segments.Add(new KeySegmentTemplate(entityType));
                        }
                        if (declaredEntityType != null)
                        {
                            segments.Add(new CastSegmentTemplate(declaredEntityType));
                        }

                        segments.Add(new PropertySegmentTemplate((string)null/*entityType*/));

                        ODataPathTemplate template = new ODataPathTemplate(segments);
                        action.AddSelector(prefix, model, template);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasKeyParameter(IEdmEntityType entityType, ActionModel action)
        {
            var keys = entityType.Key().ToArray();
            if (keys.Length == 1)
            {
                return action.Parameters.Any(p => p.ParameterInfo.Name == "key");
            }
            else
            {
                foreach (var key in keys)
                {
                    string keyName = $"key{key.Name}";
                    if (!action.Parameters.Any(p => p.ParameterInfo.Name == keyName))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private static string Split(string actionName, out string property, out string cast, out string declared)
        {
            string method = null;
            property = null;
            cast = null;
            declared = null;

            string text;
            // Get{PropertyName}Of<cast>From<declard>
            if (actionName.StartsWith("Get", StringComparison.OrdinalIgnoreCase))
            {
                method = "Get";
                text = actionName.Substring(3);
            }
            else if (actionName.StartsWith("PutTo", StringComparison.OrdinalIgnoreCase))
            {
                method = "PutTo";
                text = actionName.Substring(5);
            }
            else if (actionName.StartsWith("PatchTo", StringComparison.OrdinalIgnoreCase))
            {
                method = "PatchTo";
                text = actionName.Substring(7);
            }
            else if (actionName.StartsWith("DeleteTo", StringComparison.OrdinalIgnoreCase))
            {
                method = "DeleteTo";
                text = actionName.Substring(8);
            }
            else
            {
                return null;
            }

            int index = text.IndexOf("Of", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                property = text.Substring(0, index);
                text = text.Substring(index + 2);
                cast = Match(text, out declared);
            }
            else
            {
                property = Match(text, out declared);
            }

            return method;
        }

        private static string Match(string text, out string declared)
        {
            declared = null;
            int index = text.IndexOf("From");
            if (index > 0)
            {
                declared = text.Substring(index + 4);
                return text.Substring(0, index);
            }

            return text;
        }
    }
}
