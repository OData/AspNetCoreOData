// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
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
                throw Error.ArgumentNull(nameof(context));
            }

            Debug.Assert(context.Action != null);

            ActionModel action = context.Action;
            string actionMethodName = action.ActionMethod.Name;

            // Need to refactor the following
            // for example:  CreateRef( with the navigation property parameter) should for all navigation properties
            // CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            string method = SplitRefActionName(actionMethodName, out string httpMethod, out string property, out string declaring);
            if (method == null)
            {
                return false;
            }

            // Action parameter should have a (string navigationProperty) parameter
            if (!action.HasParameter<string>("navigationProperty"))
            {
                return false;
            }

            IEdmNavigationSource navigationSource;
            IEdmEntityType entityType;
            if (context.EntitySet != null)
            {
                entityType = context.EntitySet.EntityType();
                navigationSource = context.EntitySet;
            }
            else
            {
                entityType = context.Singleton.EntityType();
                navigationSource = context.Singleton;
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
            }

            if (navigationProperty == null)
            {
                return false;
            }

            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
            if (context.EntitySet != null)
            {
                segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
                segments.Add(KeySegmentTemplate.CreateKeySegment(entityType, context.EntitySet));
            }
            else
            {
                segments.Add(new SingletonSegmentTemplate(context.Singleton));
            }

            if (entityType != declaringType)
            {
                segments.Add(new CastSegmentTemplate(declaringType, entityType, navigationSource));
            }

            IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty, segments, out _);

            if (navigationProperty != null)
            {
                segments.Add(new NavigationSegmentTemplate(navigationProperty, targetNavigationSource));
            }
            else
            {
                //TODO: Add the navigation template segment template,
                // Or add the template for all navigation properties? 
                return false;
            }

            IEdmEntityType navigationPropertyType = navigationProperty.Type.GetElementTypeOrSelf().AsEntity().EntityDefinition();
            bool hasNavigationPropertyKeyParameter = action.HasODataKeyParameter(navigationPropertyType, "relatedKey");
            if (hasNavigationPropertyKeyParameter)
            {
                segments.Add(KeySegmentTemplate.CreateKeySegment(navigationPropertyType, targetNavigationSource, "relatedKey"));
            }

            segments.Add(new RefSegmentTemplate(navigationProperty, targetNavigationSource));

            // TODO: support key as segment?
            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector(httpMethod, context.Prefix, context.Model, template, context.RouteOptions);

            // processed
            return true;
        }

        internal static string SplitRefActionName(string actionName, out string httpMethod, out string property, out string declaring)
        {
            string method;
            httpMethod = null;
            property = null;
            declaring = null;
            string remaining;

            // CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            if (actionName.StartsWith("CreateRef", StringComparison.Ordinal))
            {
                method = "CreateRef";
                httpMethod = "post,patch";
                remaining = actionName.Substring(9);
            }
            else if (actionName.StartsWith("GetRef", StringComparison.Ordinal))
            {
                method = "GetRef";
                httpMethod = "get";
                remaining = actionName.Substring(6);
            }
            else if (actionName.StartsWith("DeleteRef", StringComparison.Ordinal))
            {
                method = "DeleteRef";
                httpMethod = "delete";
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
