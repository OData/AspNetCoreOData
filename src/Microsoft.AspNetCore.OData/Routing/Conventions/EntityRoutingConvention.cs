//-----------------------------------------------------------------------------
// <copyright file="EntityRoutingConvention.cs" company=".NET Foundation">
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
/// The convention for <see cref="IEdmEntitySet"/> with key.
/// It supports key in parenthesis and key as segment if it's a single key.
/// Conventions:
/// GET ~/entityset/key
/// GET ~/entityset/key/cast
/// PUT ~/entityset/key
/// PUT ~/entityset/key/cast
/// PATCH ~/entityset/key
/// PATCH ~/entityset/key/cast
/// DELETE ~/entityset/key
/// DELETE ~/entityset/key/cast
/// </summary>
public class EntityRoutingConvention : IODataControllerActionConvention
{
    /// <inheritdoc />
    public virtual int Order => 300;

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

        // if the action has no key parameter, skip it.
        if (!action.HasODataKeyParameter(entityType, context.Options?.RouteOptions?.EnablePropertyNameCaseInsensitive ?? false))
        {
            return false;
        }

        string actionName = action.ActionName;

        // We care about the action in this pattern: {HttpMethod}{EntityTypeName}
        (string httpMethod, string castTypeName) = Split(actionName);
        if (httpMethod == null)
        {
            return false;
        }

        IEdmStructuredType castType = null;
        if (castTypeName != null)
        {
            castType = entityType.FindTypeInInheritance(context.Model, castTypeName, context.Options?.RouteOptions?.EnableActionNameCaseInsensitive == true);
            if (castType == null)
            {
                return false;
            }
        }

        AddSelector(entitySet, entityType, castType, context.Prefix, context.Model, action, httpMethod, context.Options?.RouteOptions);
        return true;
    }

    // internal for unit test
    internal static (string, string) Split(string actionName)
    {
        string typeName;
        string methodName = null;
        if (actionName.StartsWith("Get", StringComparison.Ordinal))
        {
            methodName = "Get";
        }
        else if (actionName.StartsWith("Put", StringComparison.Ordinal))
        {
            methodName = "Put";
        }
        else if (actionName.StartsWith("Patch", StringComparison.Ordinal))
        {
            methodName = "Patch";
        }
        else if (actionName.StartsWith("Delete", StringComparison.Ordinal))
        {
            methodName = "Delete";
        }

        if (methodName != null)
        {
            typeName = actionName.Substring(methodName.Length);
        }
        else
        {
            return (null, null);
        }

        if (string.IsNullOrEmpty(typeName))
        {
            return (methodName, null);
        }

        return (methodName, typeName);
    }

    private static void AddSelector(IEdmEntitySet entitySet, IEdmEntityType entityType,
        IEdmStructuredType castType, string prefix, IEdmModel model, ActionModel action, string httpMethod, ODataRouteOptions options)
    {
        IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>
        {
            new EntitySetSegmentTemplate(entitySet),
            KeySegmentTemplate.CreateKeySegment(entityType, entitySet)
        };

        // If we have the type cast
        if (castType != null)
        {
            if (castType == entityType)
            {
                // If cast type is the entity type of the entity set.
                // we support two templates
                // ~/Customers({key})
                action.AddSelector(httpMethod, prefix, model, new ODataPathTemplate(segments), options);

                // ~/Customers({key})/Ns.Customer
                segments.Add(new CastSegmentTemplate(castType, entityType, entitySet));
                action.AddSelector(httpMethod, prefix, model, new ODataPathTemplate(segments), options);
            }
            else
            {
                // ~/Customers({key})/Ns.VipCustomer
                segments.Add(new CastSegmentTemplate(castType, entityType, entitySet));
                action.AddSelector(httpMethod, prefix, model, new ODataPathTemplate(segments), options);
            }
        }
        else
        {
            // ~/Customers({key})
            action.AddSelector(httpMethod, prefix, model, new ODataPathTemplate(segments), options);
        }
    }
}
