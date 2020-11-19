// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for <see cref="IEdmSingleton"/>.
    /// The Conventions:
    /// Get|Put|Patch ~/singleton
    /// Get|Put|Patch ~/singleton/cast
    /// </summary>
    public class SingletonRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public virtual int Order => 200;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Singleton != null;
        }

        /// <inheritdoc />
        public bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.Singleton != null);
            Debug.Assert(context.Action != null);

            ActionModel action = context.Action;
            string singletonName = context.Singleton.Name;

            string actionMethodName = action.ActionMethod.Name;
            if (IsSupportedActionName(actionMethodName, singletonName, out string httpMethod))
            {
                // ~/Me
                ODataPathTemplate template = new ODataPathTemplate(new SingletonSegmentTemplate(context.Singleton));
                action.AddSelector(httpMethod, context.Prefix, context.Model, template);

                // processed
                return true;
            }

            // type cast
            // GetFrom{EntityTypeName} or Get{SingletonName}From{EntityTypeName}
            int index = actionMethodName.IndexOf("From", StringComparison.Ordinal);
            if (index == -1)
            {
                return false;
            }

            string actionPrefix = actionMethodName.Substring(0, index);
            if (IsSupportedActionName(actionPrefix, singletonName, out httpMethod))
            {
                string castTypeName = actionMethodName.Substring(index + 4);
                IEdmEntityType entityType = context.Singleton.EntityType();

                // Shall we cast to base type and the type itself? I think yes.
                IEdmStructuredType castType = entityType.FindTypeInInheritance(context.Model, castTypeName);
                if (castType != null)
                {
                    // ~/Me/Namespace.TypeCast
                    ODataPathTemplate template = new ODataPathTemplate(
                        new SingletonSegmentTemplate(context.Singleton),
                        new CastSegmentTemplate(castType, entityType, context.Singleton));

                    action.AddSelector(httpMethod, context.Prefix, context.Model, template);
                    return true;
                }
            }

            return false;
        }

        private static bool IsSupportedActionName(string actionName, string singletonName, out string httpMethod)
        {
            if (actionName == "Get" || actionName == $"Get{singletonName}")
            {
                httpMethod = "Get";
                return true;
            }
            else if (actionName == "Put" || actionName == $"Put{singletonName}")
            {
                httpMethod = "put";
                return true;
            }
            else if (actionName == "Patch" || actionName == $"Patch{singletonName}")
            {
                httpMethod = "patch";
                return true;
            }

            httpMethod = "";
            return false;
        }
    }
}
