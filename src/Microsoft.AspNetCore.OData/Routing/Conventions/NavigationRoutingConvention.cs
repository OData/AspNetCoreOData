//-----------------------------------------------------------------------------
// <copyright file="NavigationRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions;

/// <summary>
/// Conventions for <see cref="IEdmNavigationProperty"/>.
/// Action name convention should follow this: {HttpMethodName}{NavigationPropertyName}[From{DeclaringTypeName}]
/// </summary>
public class NavigationRoutingConvention : IODataControllerActionConvention
{
    private readonly ILogger<NavigationRoutingConvention> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationRoutingConvention"/> class.
    /// </summary>
    /// <param name="logger">The injected logger.</param>
    public NavigationRoutingConvention(ILogger<NavigationRoutingConvention> logger)
    {
        _logger = logger ?? throw Error.ArgumentNull(nameof(logger));
    }

    /// <inheritdoc />
    public virtual int Order => 500;

    /// <inheritdoc />
    public virtual bool AppliesToController(ODataControllerActionContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        // structural property supports for entity set and singleton
        return context.NavigationSource != null;
    }

    /// <inheritdoc />
    public virtual bool AppliesToAction(ODataControllerActionContext context)
    {
        if (context == null)
        {
            throw Error.ArgumentNull(nameof(context));
        }

        ActionModel action = context.Action;

        IEdmNavigationSource navigationSource = context.NavigationSource;

        // filter by action parameter
        IEdmEntityType entityType = navigationSource.EntityType;
        bool hasKeyParameter = action.HasODataKeyParameter(entityType, context.Options?.RouteOptions?.EnablePropertyNameCaseInsensitive ?? false);
        if (!(context.Singleton != null ^ hasKeyParameter))
        {
            // Singleton, doesn't allow to query property with key
            // entityset, doesn't allow for non-key to query property
            return false;
        }

        // Filter by the action name.
        // The action for navigation property request should follow up {httpMethod}{PropertyName}[From{Declaring}]
        string actionName = action.ActionName;
        string method = SplitActionName(actionName, out string property, out string declared);
        if (method == null || string.IsNullOrEmpty(property))
        {
            return false;
        }

        // Find the declaring type of the property if we have the declaring type name in the action name.
        // Otherwise, it means the property is defined on the entity type of the navigation source.
        IEdmEntityType declaringEntityType = entityType;
        if (declared != null)
        {
            if (declared.Length == 0) 
            {
                // Early return for the following cases: Get|PostTo|PutTo|PatchTo{NavigationProperty}From
                return false;
            }

            declaringEntityType = entityType.FindTypeInInheritance(context.Model, declared) as IEdmEntityType;
            if (declaringEntityType == null)
            {
                return false;
            }
        }

        // Find the property, and we only care about the navigation property.
        bool enablePropertyNameCaseInsensitive = context?.Options?.RouteOptions.EnablePropertyNameCaseInsensitive ?? false;

        IEdmProperty edmProperty = declaringEntityType.FindProperty(property, enablePropertyNameCaseInsensitive);
        if (edmProperty == null || edmProperty.PropertyKind == EdmPropertyKind.Structural)
        {
            return false;
        }

        // Starts the routing template
        //IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
        //if (context.EntitySet != null)
        //{
        //    segments.Add(new EntitySetSegmentTemplate(context.EntitySet));
        //}
        //else
        //{
        //    segments.Add(new SingletonSegmentTemplate(context.Singleton));
        //}

        //if (hasKeyParameter)
        //{
        //    segments.Add(new KeySegmentTemplate(entityType));
        //}

        //if (declared != null)
        //{
        //    // It should be always single type
        //    segments.Add(new CastSegmentTemplate(declaringEntityType, entityType, navigationSource));
        //}

        IEdmNavigationProperty navigationProperty = (IEdmNavigationProperty)edmProperty;
        //IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty, segments, out _);

        //segments.Add(new NavigationSegmentTemplate(navigationProperty, targetNavigationSource));

        //ODataPathTemplate template = new ODataPathTemplate(segments);
        //action.AddSelector(method, context.Prefix, context.Model, template);

        AddSelector(method, context, action, navigationSource, declared, declaringEntityType, navigationProperty, hasKeyParameter, false);

        if (CanApplyDollarCount(navigationProperty, method, context.Options?.RouteOptions))
        {
            AddSelector(method, context, action, navigationSource, declared, declaringEntityType, navigationProperty, hasKeyParameter, true);
        }

        return true;
    }

    private void AddSelector(string httpMethod, ODataControllerActionContext context, ActionModel action,
        IEdmNavigationSource navigationSource, string declared, IEdmEntityType declaringEntityType,
        IEdmNavigationProperty navigationProperty, bool hasKey, bool dollarCount)
    {
        IEdmEntitySet entitySet = context.EntitySet;
        IEdmEntityType entityType = navigationSource.EntityType;

        // Starts the routing template
        IList<ODataSegmentTemplate> segments = new List<ODataSegmentTemplate>();
        if (entitySet != null)
        {
            segments.Add(new EntitySetSegmentTemplate(entitySet));
        }
        else
        {
            segments.Add(new SingletonSegmentTemplate(navigationSource as IEdmSingleton));
        }

        if (hasKey)
        {
            segments.Add(KeySegmentTemplate.CreateKeySegment(entityType, navigationSource));
        }

        if (declared != null)
        {
            // It should be always single type
            if (entityType != declaringEntityType)
            {
                segments.Add(new CastSegmentTemplate(declaringEntityType, entityType, navigationSource));
            }
        }

        IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty, segments, out _);

        segments.Add(new NavigationSegmentTemplate(navigationProperty, targetNavigationSource));

        if (dollarCount)
        {
            segments.Add(CountSegmentTemplate.Instance);
        }

        ODataPathTemplate template = new ODataPathTemplate(segments);
        action.AddSelector(httpMethod.NormalizeHttpMethod(), context.Prefix, context.Model, template, context.Options?.RouteOptions);

        Log.AddedODataSelector(_logger, action, template);
    }

    /// <summary>
    /// split action using navigation action name convention.
    /// For example: PostToOrdersFromVipOrder
    /// => Method Name: PostTo
    /// => property : Orders
    /// => declaring: VipOrder
    /// </summary>
    /// <param name="actionName">The input action name.</param>
    /// <param name="property">The property name (out).</param>
    /// <param name="declaring">The declaring name (out).</param>
    /// <returns>The http method name or null.</returns>
    internal static string SplitActionName(string actionName, out string property, out string declaring)
    {
        string method = null;
        string text = null;

        // HttpMethodName{NavigationPropertyName}From<declaring>
        foreach (var prefix in new[] { "Get", "PostTo", "PutTo", "PatchTo" })
        {
            if (actionName.StartsWith(prefix, StringComparison.Ordinal))
            {
                method = prefix;
                text = actionName.Substring(prefix.Length);
                break;
            }
        }

        property = null;
        declaring = null;
        if (method == null)
        {
            return null;
        }

        int index = text.IndexOf("From", StringComparison.Ordinal);
        if (index > 0)
        {
            property = text.Substring(0, index);
            declaring = text.Substring(index + 4);
        }
        else
        {
            property = text;
        }

        return method;
    }

    /// <summary>
    /// OData spec: To request only the number of items of a collection of entities or items of a collection-valued property,
    /// the client issues a GET request with /$count appended to the resource path of the collection.
    /// </summary>
    /// <param name="edmProperty">The property to test.</param>
    /// <param name="method">The http method.</param>
    /// <param name="routeOptions">The route options.</param>
    /// <returns>True/false to identify whether to apply $count.</returns>
    protected virtual bool CanApplyDollarCount(IEdmNavigationProperty edmProperty, string method, ODataRouteOptions routeOptions)
    {
        if(edmProperty == null)
        {
            throw Error.ArgumentNull(nameof(edmProperty));
        }

        if (routeOptions != null && !routeOptions.EnableDollarCountRouting)
        {
            return false;
        }

        return method == "Get" && edmProperty.Type.IsCollection();
    }

    private static class Log
    {
        private static readonly Action<ILogger, string, Exception> _addedODataSelector = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, "AddODataNavigationConvention"),
            "Added OData Convention '{ConventionMessage}'");

        public static void AddedODataSelector(ILogger logger, ActionModel action, ODataPathTemplate template)
        {
            string message = action.DisplayName + ": " + template.GetTemplates().FirstOrDefault();
            _addedODataSelector(logger, message, null);
        }
    }
}
