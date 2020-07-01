// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataControllerActionConvention"/> that handles entity reference manipulations.
    /// </summary>
    public class RefRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public virtual int Order => 1000;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // $ref supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <inheritdoc />
        public bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.Action != null);

            ActionModel action = context.Action;
            string actionMethodName = action.ActionMethod.Name;

            // CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            string method = SplitRefActionName(actionMethodName, out string property, out string declaring);
            if (method == null)
            {
                return false;
            }

            // Action parameter should have a (string navigationProperty) parameter
            if (!action.HasParameter<string>("navigationProperty"))
            {
                return false;
            }

            IEdmEntityType entityType;
            if (context.EntitySet != null)
            {
                entityType = context.EntitySet.EntityType();
            }
            else
            {
                entityType = context.Singleton.EntityType();
            }

            // For entity set, we should have the key parameter
            // For Singleton, we should not have the key parameter
            bool hasODataKeyParameter = action.HasODataKeyParameter(entityType);
            if ((context.EntitySet != null && !hasODataKeyParameter) ||
                (context.Singleton != null && hasODataKeyParameter))
            {
                return false;
            }

            // Find the navigation property declaring type
            IEdmStructuredType declaringType = entityType;
            if (declaring != null)
            {
                declaringType = entityType.FindTypeInInheritance(context.Model, declaring);
                if (declaringType == null)
                {
                    return false;
                }
            }

            // Find the navigation property if have
            IEdmNavigationProperty navigationProperty = null;
            if (property != null)
            {
                navigationProperty = declaringType.DeclaredNavigationProperties().FirstOrDefault(p => p.Name == property);
                if (navigationProperty == null)
                {
                    return false;
                }
            }

            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
            if (context.EntitySet != null)
            {
                segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                segments.Add(new KeySegmentTemplate(entityType));
            }
            else
            {
                segments.Add(new SingletonSegmentTemplate(context.Singleton));
            }

            if (entityType != declaringType)
            {
                segments.Add(new CastSegmentTemplate(declaringType));
            }

            if (navigationProperty != null)
            {
                segments.Add(new NavigationSegmentTemplate(navigationProperty));
            }
            else
            {
                //TODO: Add the navigation template segment template
            }

            IEdmEntityType navigationPropertyType = navigationProperty.Type.AsEntity().EntityDefinition();
            bool hasNavigationPropertyKeyParameter = action.HasODataKeyParameter(navigationPropertyType, "relatedKey");
            if (hasNavigationPropertyKeyParameter)
            {
                segments.Add(new KeySegmentTemplate(navigationPropertyType, "relatedKey"));
            }

            segments.Add(new RefSegmentTemplate(navigationProperty));

            // TODO: support key as segment?
            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector(context.Prefix, context.Model, template);

            // processed
            return true;
        }

        internal static string SplitRefActionName(string actionName, out string property, out string declaring)
        {
            string method;
            property = null;
            declaring = null;
            string remaining;

            // CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            if (actionName.StartsWith("CreateRef", StringComparison.Ordinal))
            {
                method = "CreateRef";
                remaining = actionName.Substring(9);
            }
            else if (actionName.StartsWith("GetRef", StringComparison.Ordinal))
            {
                method = "GetRef";
                remaining = actionName.Substring(6);
            }
            else if (actionName.StartsWith("DeleteRef", StringComparison.Ordinal))
            {
                method = "DeleteRef";
                remaining = actionName.Substring(9);
            }
            else
            {
                return null;
            }

            if (string.IsNullOrEmpty(remaining))
            {
                return method;
            }

            if (remaining.StartsWith("To", StringComparison.OrdinalIgnoreCase))
            {
                remaining = remaining.Substring(2);
            }
            else
            {
                return null;
            }

            int index = remaining.IndexOf("From", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                property = remaining.Substring(0, index);
                declaring = remaining.Substring(index + 4);
            }
            else
            {
                property = remaining;
            }

            return method;
        }
    }
}
