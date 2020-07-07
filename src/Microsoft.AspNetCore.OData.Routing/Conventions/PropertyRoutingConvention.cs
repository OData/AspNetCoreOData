// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for the property access.
    /// Get|PostTo|PutTo|PatchTo|DeleteTo ~/entityset/key/property
    /// GET|PostTo|PutTo|PatchTo|DeleteTo ~/singleton/property
    /// Get|PostTo|PutTo|PatchTo|DeleteTo ~/entityset/key/cast/property/
    /// GET|PostTo|PutTo|PatchTo|DeleteTo ~/singleton/cast/property
    /// Get|PostTo|PutTo|PatchTo|DeleteTo ~/entityset/key/property/cast
    /// GET|PostTo|PutTo|PatchTo|DeleteTo ~/singleton/property/cast
    /// Get|PostTo|PutTo|PatchTo|DeleteTo ~/entityset/key/cast/property/cast
    /// GET|PostTo|PutTo|PatchTo|DeleteTo ~/singleton/cast/property/cast
    /// GET ~/entityset/key/property/$value
    /// GET ~/entityset/key/cast/property/$value
    /// GET ~/singleton/property/$value
    /// GET ~/singleton/cast/property/$value
    /// GET ~/entityset/key/property/$count
    /// GET ~/entityset/key/cast/property/$count
    /// GET ~/singleton/property/$count
    /// GET ~/singleton/cast/property/$count
    /// </summary>
    public class PropertyRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public virtual int Order => 400;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // structural property supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <inheritdoc />
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

            string method = SplitActionName(actionName, out string property, out string cast, out string declared);
            if (method == null || string.IsNullOrEmpty(property))
            {
                return false;
            }

            // filter by action parameter
            IEdmEntityType entityType = navigationSource.EntityType();
            bool hasKeyParameter = action.HasODataKeyParameter(entityType);
            if (!(context.Singleton != null ^ hasKeyParameter))
            {
                // Singleton, doesn't allow to query property with key
                // entityset, doesn't allow for non-key to query property
                return false;
            }

            // Find the declaring type of the property if we have the declaring type name in the action name.
            // eitherwise, it means the property is defined on the entity type of the navigation source.
            IEdmEntityType declaringEntityType = entityType;
            if (declared != null)
            {
                declaringEntityType = entityType.FindTypeInInheritance(context.Model, declared) as IEdmEntityType;
                if (declaringEntityType == null)
                {
                    return false;
                }
            }

            IEdmProperty edmProperty = declaringEntityType.FindProperty(property);
            if (edmProperty == null || edmProperty.PropertyKind != EdmPropertyKind.Structural)
            {
                return false;
            }

            IEdmComplexType castType = null;
            if (cast != null)
            {
                IEdmTypeReference propertyElementType = edmProperty.Type.ElementType();
                if (propertyElementType.IsComplex())
                {
                    IEdmComplexType complexType = propertyElementType.AsComplex().ComplexDefinition();
                    castType = complexType.FindTypeInInheritance(context.Model, cast) as IEdmComplexType;
                    if (castType == null)
                    {
                        return false;
                    }
                }
                else
                {
                    // only support complex type cast, (TODO: maybe consider to support Edm.PrimitiveType cast)
                    return false;
                }

            }

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

                castComplexType = propertyType.ToStructuredType().FindTypeInInheritance(context.Model, cast);
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
            if (declaringEntityType != null && declaringEntityType != entityType)
            {
                segments.Add(new CastSegmentTemplate(declaringEntityType));
            }

            segments.Add(new PropertySegmentTemplate((IEdmStructuralProperty)edmProperty));

            if (castComplexType != null)
            {
                segments.Add(new CastSegmentTemplate(castComplexType));
            }

            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector(context.Prefix, context.Model, template);
            return true;

            //else
            //{
            //// map to a static action like:  <method>Property(int key, string property)From<...>
            //if (property == "Property" && cast == null)
            //{
            //    if (action.Parameters.Any(p => p.ParameterInfo.Name == "property" && p.ParameterType == typeof(string)))
            //    {
            //        // we find a static method mapping for all property
            //        // we find a action route
            //        IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();

            //        if (context.EntitySet != null)
            //        {
            //            segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
            //        }
            //        else
            //        {
            //            segments.Add(new SingletonSegmentTemplate(context.Singleton));
            //        }

            //        if (hasKeyParameter)
            //        {
            //            segments.Add(new KeySegmentTemplate(entityType));
            //        }
            //        if (declaringEntityType != null)
            //        {
            //            segments.Add(new CastSegmentTemplate(declaringEntityType));
            //        }

            //        segments.Add(new PropertyCatchAllSegmentTemplate(entityType));

            //        ODataPathTemplate template = new ODataPathTemplate(segments);
            //        action.AddSelector(context.Prefix, context.Model, template);
            //        return true;
            //    }
            //}
            //}
        }

        private static string SplitActionName(string actionName, out string property, out string cast, out string declared)
        {
            string method = null;
            string text = "";
            // Get{PropertyName}Of<cast>From<declaring>: GetCityOfSubAddressFromVipCustomer
            foreach (var prefix in new[] { "Get", "PostTo", "PutTo", "PatchTo", "DeleteTo" })
            {
                if (actionName.StartsWith(prefix, StringComparison.Ordinal))
                {
                    method = prefix;
                    text = actionName.Substring(prefix.Length);
                    break;
                }
            }

            property = null;
            cast = null;
            declared = null;
            if (method == null)
            {
                return null;
            }

            int index = text.IndexOf("Of", StringComparison.Ordinal);
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
