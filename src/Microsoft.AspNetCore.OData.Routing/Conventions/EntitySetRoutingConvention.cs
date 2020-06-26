// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{

    /// <summary>
    /// 
    /// </summary>
    public class EntitySetEndpointConvention : IODataControllerActionConvention
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual int Order => 200;

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

            if (context.EntitySet == null)
            {
                return false;
            }

            IEdmEntitySet entitySet = context.EntitySet;

            if (action.Parameters.Count != 0)
            {
                // TODO: improve here to accept other parameters, for example ODataQueryOptions<T>
                return false;
            }

            string actionName = action.ActionMethod.Name;

            if (actionName == "Get" ||
                actionName == $"Get{entitySet.Name}")
            {
                ODataPathTemplate template = new ODataPathTemplate(new EntitySetSegmentTemplate(entitySet));
                action.AddSelector(context.Prefix, context.Model, template);

                // $count
                template = new ODataPathTemplate(new EntitySetSegmentTemplate(entitySet), CountSegmentTemplate.Instance);
                action.AddSelector(context.Prefix, context.Model, template);
                return true;
            }
            else if (actionName == "Post" ||
                actionName == $"Post{entitySet.EntityType().Name}")
            {
                ODataPathTemplate template = new ODataPathTemplate(new EntitySetSegmentTemplate(entitySet));
                action.AddSelector(context.Prefix, context.Model, template);
                return true;
            }
            else
            {
                // process the derive type (cast)
                // search all derived types
            }

            return false;
        }
    }
}
