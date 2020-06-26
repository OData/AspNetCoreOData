// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// 
    /// </summary>
    public class EntityEndpointConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public int Order => 300;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            return context?.EntitySet != null;
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
            if (context.EntitySet == null || action.Parameters.Count < 1)
            {
                // At lease one parameter for the key.
                return false;
            }

            IEdmEntitySet entitySet = context.EntitySet;
            var entityType = entitySet.EntityType();
            var entityTypeName = entitySet.EntityType().Name;
            var keys = entitySet.EntityType().Key().ToArray();

            string actionName = action.ActionMethod.Name;
            if ((actionName == "Get" ||
                actionName == $"Get{entityTypeName}" ||
                actionName == "Put" ||
                actionName == $"Put{entityTypeName}" ||
                actionName == "Patch" ||
                actionName == $"Patch{entityTypeName}" ||
                actionName == "Delete" ||
                actionName == $"Delete{entityTypeName}") &&
                keys.Length == action.Parameters.Count)
            {
                ODataPathTemplate template = new ODataPathTemplate(
                    new EntitySetSegmentTemplate(entitySet),
                    new KeySegmentTemplate(entityType)
                    );

                // support key in parenthesis
                action.AddSelector(context.Prefix, context.Model, template);

                // support key as segment
                ODataPathTemplate newTemplate = template.Clone();
                newTemplate.KeyAsSegment = true;
                action.AddSelector(context.Prefix, context.Model, newTemplate);
                return true;
            }

            return false;
        }
    }
}
