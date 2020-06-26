// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class RefRoutingConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual int Order => 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // bound operation supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // use the cached
            Debug.Assert(context.Singleton != null);
            Debug.Assert(context.Action != null);
            ActionModel action = context.Action;

            string singletonName = context.Singleton.Name;
            string prefix = context.Prefix;
            IEdmModel model = context.Model;

            string actionMethodName = action.ActionMethod.Name;
            if (IsSupportedActionName(actionMethodName, singletonName))
            {
                ODataPathTemplate template = new ODataPathTemplate(new SingletonSegmentTemplate(context.Singleton));
                action.AddSelector(context.Prefix, context.Model, template);

                // processed
                return true;
            }

            // type cast
            // Get{SingletonName}From{EntityTypeName} or GetFrom{EntityTypeName}
            int index = actionMethodName.IndexOf("From", StringComparison.Ordinal);
            if (index == -1)
            {
                return false;
            }

            string actionPrefix = actionMethodName.Substring(0, index);
            if (IsSupportedActionName(actionPrefix, singletonName))
            {
                IEdmEntityType entityType = context.Singleton.EntityType();
                string castTypeName = actionMethodName.Substring(index + 4);

                // Shall we cast to base type and the type itself? I think yes.
                IEdmEntityType baseType = entityType;
                while (baseType != null)
                {
                    if (baseType.Name == castTypeName)
                    {
                        ODataPathTemplate template = new ODataPathTemplate(new SingletonSegmentTemplate(context.Singleton),
                            new CastSegmentTemplate(baseType));
                        action.AddSelector(context.Prefix, context.Model, template);

                        return true;
                    }

                    baseType = baseType.BaseEntityType();
                }

                // shall we cast to derived type
                IEdmEntityType castType = model.FindAllDerivedTypes(entityType).OfType<IEdmEntityType>().FirstOrDefault(c => c.Name == castTypeName);
                if (castType != null)
                {
                    ODataPathTemplate template = new ODataPathTemplate(new SingletonSegmentTemplate(context.Singleton),
                        new CastSegmentTemplate(castType));
                    action.AddSelector(context.Prefix, context.Model, template);

                    return true;
                }
            }

            return false;
        }

        private static bool IsSupportedActionName(string actionName, string singletonName)
        {
            return actionName == "Get" || actionName == $"Get{singletonName}" ||
                actionName == "Put" || actionName == $"Put{singletonName}" ||
                actionName == "Patch" || actionName == $"Patch{singletonName}";
        }
    }
}
