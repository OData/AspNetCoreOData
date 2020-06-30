// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for <see cref="IEdmEntitySet"/>.
    /// </summary>
    public class EntitySetEndpointConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public virtual int Order => 200;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            return context?.EntitySet != null;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Debug.Assert(context.EntitySet != null);
            Debug.Assert(context.Action != null);

            ActionModel action = context.Action;
            IEdmEntitySet entitySet = context.EntitySet;
            IEdmEntityType entityType = entitySet.EntityType();

            // if the action has key parameter, skip it.
            if (action.HasODataKeyParameter(entityType))
            {
                return false;
            }

            string actionName = action.ActionMethod.Name;

            // 1. Without type case
            if (ProcessEntitySetAction(actionName, entitySet, null, context.Prefix, context.Model, action))
            {
                return true;
            }

            // 2. process the derive type (cast) by searching all derived types
            // GetFrom{EntityTypeName} or Get{EntitySet}From{EntityTypeName}
            int index = actionName.IndexOf("From", StringComparison.Ordinal);
            if (index == -1)
            {
                return false;
            }

            string castTypeName = actionName.Substring(index + 4); // + 4 means to skip the "From"
            IEdmStructuredType castType = entityType.FindTypeInInheritance(context.Model, castTypeName);
            if (castType == null)
            {
                return false;
            }

            string actionPrefix = actionName.Substring(0, index);
            return ProcessEntitySetAction(actionPrefix, entitySet, castType, context.Prefix, context.Model, action);
        }

        private static bool ProcessEntitySetAction(string actionName,
            IEdmEntitySet entitySet, IEdmStructuredType castType,
            string prefix, IEdmModel model,
            ActionModel action)
        {
            if (actionName == "Get" || actionName == $"Get{entitySet.Name}")
            {
                // GET ~/Customers or GET ~/Customers/Ns.VipCustomer
                IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
                {
                    new EntitySetSegmentTemplate(entitySet)
                };
                if (castType != null)
                {
                    segments.Add(new CastSegmentTemplate(castType));
                }
                ODataPathTemplate template = new ODataPathTemplate(segments);
                action.AddSelector(prefix, model, template);

                // GET ~/Customers/$count or GET ~/Customers/Ns.VipCustomer/$count
                segments = new List<ODataSegmentTemplate>
                {
                    new EntitySetSegmentTemplate(entitySet)
                };
                if (castType != null)
                {
                    segments.Add(new CastSegmentTemplate(castType));
                }
                segments.Add(CountSegmentTemplate.Instance);

                template = new ODataPathTemplate(segments);
                action.AddSelector(prefix, model, template);
                return true;
            }
            else if (actionName == "Post" || actionName == $"Post{entitySet.EntityType().Name}")
            {
                // POST ~/Customers
                IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
                {
                    new EntitySetSegmentTemplate(entitySet)
                };
                if (castType != null)
                {
                    segments.Add(new CastSegmentTemplate(castType));
                }
                ODataPathTemplate template = new ODataPathTemplate(segments);
                action.AddSelector(prefix, model, template);
                return true;
            }

            return false;
        }
    }
}
