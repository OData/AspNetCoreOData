// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for <see cref="IEdmAction"/>.
    /// Post ~/entity|singleton/action,  ~/entity|singleton/cast/action
    /// Post ~/entity|singleton/key/action,  ~/entity|singleton/key/cast/action
    /// </summary>
    public class ActionRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public int Order => 800;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // bound operation supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IEdmNavigationSource navigationSource = context.EntitySet == null ?
                (IEdmNavigationSource)context.Singleton :
                (IEdmNavigationSource)context.EntitySet;

            IEdmModel model = context.Model;
            string prefix = context.Prefix;
            IEdmEntityType entityType = navigationSource.EntityType();

            ActionModel action = context.Action;

            // function should have the [HttpPost]
            if (!action.Attributes.Any(a => a is HttpPostAttribute))
            {
                return false;
            }

            bool hasKeyParameter = action.HasODataKeyParameter(entityType);
            string actionName = action.ActionMethod.Name;
            IEnumerable<IEdmAction> candidates = model.SchemaElements.OfType<IEdmAction>().Where(f => f.IsBound && f.Name == actionName);
            foreach (IEdmAction edmAction in candidates)
            {
                IEdmOperationParameter bindingParameter = edmAction.Parameters.FirstOrDefault();
                if (bindingParameter == null)
                {
                    continue;
                }

                IEdmTypeReference bindingType = bindingParameter.Type;
                bool bindToNonCollection = bindingType.TypeKind() != EdmTypeKind.Collection;
                if (hasKeyParameter != bindToNonCollection)
                {
                    // if binding to collection and the action has key parameter, skip
                    // if binding to non-collection and the action hasn't key parameter, skip
                    continue;
                }

                if (!bindingType.Definition.IsEntityOrEntityCollectionType(out IEdmEntityType bindingEntityType))
                {
                    continue;
                }

                IEdmEntityType castType = null;
                if (entityType.IsOrInheritsFrom(bindingEntityType))
                {
                    // True if and only if the thisType is equivalent to or inherits from otherType.
                    castType = null;
                }
                else if (bindingEntityType.InheritsFrom(entityType))
                {
                    // True if and only if the type inherits from the potential base type.
                    castType = bindingEntityType;
                }
                else
                {
                    continue;
                }

                // Now, let's add the selector model.
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
                    segments.Add(new KeySegmentTemplate(entityType, navigationSource));
                }

                if (castType != null)
                {
                    if (context.Singleton != null || !hasKeyParameter)
                    {
                        segments.Add(new CastSegmentTemplate(castType, entityType, navigationSource));
                    }
                    else
                    {
                        segments.Add(new CastSegmentTemplate(new EdmCollectionType(castType.ToEdmTypeReference(false)),
                            new EdmCollectionType(entityType.ToEdmTypeReference(false)), navigationSource));
                    }
                }

                segments.Add(new ActionSegmentTemplate(edmAction, false));
                ODataPathTemplate template = new ODataPathTemplate(segments);
                action.AddSelector(prefix, model, template);
            }

            // in OData operationImport routing convention, all action are processed by default
            // even it's not a really edm operation import call.
            return false;
        }
    }
}
