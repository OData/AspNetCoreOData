//-----------------------------------------------------------------------------
// <copyright file="PropertyRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Edm;
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
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // structural property supports for entity set and singleton
            return context.NavigationSource != null;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            IEdmNavigationSource navigationSource = context.NavigationSource;
            if (navigationSource == null)
            {
                return false;
            }

            ActionModel action = context.Action;
            string actionName = action.ActionName;

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
            // either wise, it means the property is defined on the entity type of the navigation source.
            IEdmEntityType declaringEntityType = entityType;
            if (declared != null)
            {
                if (declared.Length == 0)
                {
                    // Early return for the following cases:
                    // - Get|PostTo|PutTo|PatchTo|DeleteTo{PropertyName}From
                    // - Get|PostTo|PutTo|PatchTo|DeleteTo{PropertyName}Of{Cast}From
                    return false;
                }

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

            if (!CanApply(edmProperty, method))
            {
                return false;
            }

            IEdmComplexType castType;
            // Only process structural property
            IEdmStructuredType castComplexType = null;
            if (cast != null)
            {
                if (cast.Length == 0)
                {
                    // Avoid unnecessary call to FindTypeInheritance
                    // Cases handled: Get|PostTo|PutTo|PatchTo|DeleteTo{PropertyName}Of
                    return false;
                }

                IEdmType propertyElementType = edmProperty.Type.Definition.AsElementType();
                if (propertyElementType.TypeKind == EdmTypeKind.Complex)
                {
                    IEdmComplexType complexType = (IEdmComplexType)propertyElementType;
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

            AddSelector(method, context, action, navigationSource, (IEdmStructuralProperty)edmProperty, castComplexType, declaringEntityType, false, false);

            if (CanApplyDollarCount(edmProperty, method))
            {
                AddSelector(method, context, action, navigationSource, (IEdmStructuralProperty)edmProperty, castComplexType, declaringEntityType, false, true);
            }

            if (CanApplyDollarValue(edmProperty, method))
            {
                AddSelector(method, context, action, navigationSource, (IEdmStructuralProperty)edmProperty, castComplexType, declaringEntityType, true, false);
            }

            return true;
        }

        private static void AddSelector(string httpMethod, ODataControllerActionContext context, ActionModel action,
            IEdmNavigationSource navigationSource,
            IEdmStructuralProperty edmProperty,
            IEdmType cast, IEdmEntityType declaringType, bool dollarValue, bool dollarCount)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
            if (entitySet != null)
            {
                segments.Add(new EntitySetSegmentTemplate(entitySet));
                segments.Add(KeySegmentTemplate.CreateKeySegment(entityType, navigationSource));
            }
            else
            {
                segments.Add(new SingletonSegmentTemplate(navigationSource as IEdmSingleton));
            }

            if (declaringType != null && declaringType != entityType)
            {
                segments.Add(new CastSegmentTemplate(declaringType, entityType, navigationSource));
            }

            segments.Add(new PropertySegmentTemplate(edmProperty));

            if (cast != null)
            {
                if (edmProperty.Type.IsCollection())
                {
                    cast = new EdmCollectionType(cast.ToEdmTypeReference(edmProperty.Type.IsNullable));
                }

                // TODO: maybe create the collection type for the collection????
                segments.Add(new CastSegmentTemplate(cast, edmProperty.Type.Definition, navigationSource));
            }

            if (dollarValue)
            {
                segments.Add(new ValueSegmentTemplate(edmProperty.Type.Definition));
            }

            if (dollarCount)
            {
                segments.Add(CountSegmentTemplate.Instance);
            }

            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector(httpMethod.NormalizeHttpMethod(), context.Prefix, context.Model, template, context.Options?.RouteOptions);
        }

        // Split the property such as "GetCityOfSubAddressFromVipCustomer"
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
            int index = text.IndexOf("From", StringComparison.Ordinal);
            if (index > 0)
            {
                declared = text.Substring(index + 4);
                return text.Substring(0, index);
            }

            return text;
        }

        private static bool CanApply(IEdmProperty edmProperty, string method)
        {
            Contract.Assert(edmProperty != null);

            bool isCollection = edmProperty.Type.IsCollection();

            // OData Spec: PATCH is not supported for collection properties.
            if (isCollection && method == "PatchTo")
            {
                return false;
            }

            // Allow post only to collection properties
            if (!isCollection && method == "PostTo")
            {
                return false;
            }

            // OData spec: A successful DELETE request to the edit URL for a structural property, ... sets the property to null.
            // The request body is ignored and should be empty.
            // DELETE request to a non-nullable value MUST fail and the service respond with 400 Bad Request or other appropriate error.
            if (!edmProperty.Type.IsNullable && method == "DeleteTo")
            {
                return false;
            }

            return true;
        }

        // OData spec: To retrieve the raw value of a primitive type property, the client sends a GET request to the property value URL.
        // So, let's apply $value for the "Get" and non-collection primitive property
        private static bool CanApplyDollarValue(IEdmProperty edmProperty, string method)
        {
            Contract.Assert(edmProperty != null);

            return method == "Get" && !edmProperty.Type.IsCollection() && (edmProperty.Type.IsPrimitive() || edmProperty.Type.IsEnum());
        }

        // OData spec: To request only the number of items of a collection of entities or items of a collection-valued property,
        // the client issues a GET request with /$count appended to the resource path of the collection.
        private static bool CanApplyDollarCount(IEdmProperty edmProperty, string method)
        {
            Contract.Assert(edmProperty != null);

            return method == "Get" && edmProperty.Type.IsCollection();
        }
    }
}
