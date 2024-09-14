//-----------------------------------------------------------------------------
// <copyright file="EntitySetRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions;

/// <summary>
/// The convention for <see cref="IEdmEntitySet"/>.
/// Conventions:
/// GET ~/entityset
/// GET ~/entityset/$count
/// GET ~/entityset/cast
/// GET ~/entityset/cast/$count
/// POST ~/entityset
/// POST ~/entityset/cast
/// PATCH ~/entityset ==> Delta resource set patch
/// </summary>
public class EntitySetRoutingConvention : IODataControllerActionConvention
{
    /// <inheritdoc />
    public virtual int Order => 100;

    /// <inheritdoc />
    public virtual bool AppliesToController(ODataControllerActionContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        return context.EntitySet != null;
    }

    /// <inheritdoc />
    public virtual bool AppliesToAction(ODataControllerActionContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        ActionModel action = context.Action;
        IEdmEntitySet entitySet = context.EntitySet;
        IEdmEntityType entityType = entitySet.EntityType;

        // if the action has key parameter, skip it.
        if (action.HasODataKeyParameter(entityType, context.Options?.RouteOptions?.EnablePropertyNameCaseInsensitive ?? false))
        {
            return false;
        }

        string actionName = action.ActionName;

        // 1. Without type case
        if (ProcessEntitySetAction(actionName, entitySet, null, context, action))
        {
            return true;
        }

        // 2. process the derived type (cast) by searching all derived types
        // GetFrom{EntityTypeName} or Get{EntitySet}From{EntityTypeName}
        int index = actionName.IndexOf("From", StringComparison.Ordinal);
        if (index == -1)
        {
            return false;
        }

        string castTypeName = actionName.Substring(index + 4); // + 4 means to skip the "From"

        if (castTypeName.Length == 0)
        {
            // Early return for the following cases:
            // - Get|Post|PatchFrom
            // - Get|Patch{EntitySet}From
            // - Post{EntityType}From
            return false;
        }

        IEdmStructuredType castType = entityType.FindTypeInInheritance(context.Model, castTypeName, context.Options?.RouteOptions?.EnableActionNameCaseInsensitive == true);
        if (castType == null)
        {
            return false;
        }

        string actionPrefix = actionName.Substring(0, index);
        return ProcessEntitySetAction(actionPrefix, entitySet, castType, context, action);
    }

    private bool ProcessEntitySetAction(string actionName, IEdmEntitySet entitySet, IEdmStructuredType castType,
        ODataControllerActionContext context, ActionModel action)
    {
        StringComparison actionNameComparison = context.Options?.RouteOptions?.EnableActionNameCaseInsensitive == true ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                 
        if (actionName.Equals("Get", actionNameComparison) || actionName.Equals($"Get{entitySet.Name}", actionNameComparison))
        {
            IEdmCollectionType castCollectionType = null;
            if (castType != null)
            {
                castCollectionType = castType.ToCollection(true);
            }

            IEdmCollectionType entityCollectionType = entitySet.EntityType.ToCollection(true);

            // GET ~/Customers or GET ~/Customers/Ns.VipCustomer
            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
            {
                new EntitySetSegmentTemplate(entitySet)
            };

            if (castType != null)
            {
                segments.Add(new CastSegmentTemplate(castCollectionType, entityCollectionType, entitySet));
            }

            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector("Get", context.Prefix, context.Model, template, context.Options?.RouteOptions);

            if (CanApplyDollarCount(entitySet, context.Options?.RouteOptions))
            {
                // GET ~/Customers/$count or GET ~/Customers/Ns.VipCustomer/$count
                segments = new List<ODataSegmentTemplate>
                {
                    new EntitySetSegmentTemplate(entitySet)
                };

                if (castType != null)
                {
                    segments.Add(new CastSegmentTemplate(castCollectionType, entityCollectionType, entitySet));
                }

                segments.Add(CountSegmentTemplate.Instance);

                template = new ODataPathTemplate(segments);
                action.AddSelector("Get", context.Prefix, context.Model, template, context.Options?.RouteOptions);
            }

            return true;
        }
        else if (actionName.Equals("Post", actionNameComparison) || actionName.Equals($"Post{entitySet.EntityType.Name}", actionNameComparison))
        {
            // POST ~/Customers
            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
            {
                new EntitySetSegmentTemplate(entitySet)
            };

            if (castType != null)
            {
                IEdmCollectionType castCollectionType = castType.ToCollection(true);
                IEdmCollectionType entityCollectionType = entitySet.EntityType.ToCollection(true);
                segments.Add(new CastSegmentTemplate(castCollectionType, entityCollectionType, entitySet));
            }
            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector("Post", context.Prefix, context.Model, template, context.Options?.RouteOptions);
            return true;
        }
        else if (actionName.Equals("Patch", actionNameComparison) || actionName.Equals($"Patch{entitySet.Name}", actionNameComparison))
        {
            // PATCH ~/Patch  , ~/PatchCustomers
            IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
            {
                new EntitySetSegmentTemplate(entitySet)
            };

            if (castType != null)
            {
                IEdmCollectionType castCollectionType = castType.ToCollection(true);
                IEdmCollectionType entityCollectionType = entitySet.EntityType.ToCollection(true);
                segments.Add(new CastSegmentTemplate(castCollectionType, entityCollectionType, entitySet));
            }

            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector("Patch", context.Prefix, context.Model, template, context.Options?.RouteOptions);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tests whether to apply $count on the <see cref="IEdmEntitySet"/>.
    /// </summary>
    /// <param name="entitySet">The entity set to test.</param>
    /// <param name="routeOptions">The route options.</param>
    /// <returns>True/false to identify whether to apply $count.</returns>
    protected virtual bool CanApplyDollarCount(IEdmEntitySet entitySet, ODataRouteOptions routeOptions)
        => routeOptions != null ? routeOptions.EnableDollarCountRouting : false;
}
