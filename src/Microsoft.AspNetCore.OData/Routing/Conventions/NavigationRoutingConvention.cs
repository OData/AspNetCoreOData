// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using System.Diagnostics.Contracts;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// Conventions for <see cref="IEdmNavigationProperty"/>.
    /// Action name convention should follow up: {HttpMethodName}{NavigationPropertyName}[From{DeclaringTypeName}]
    /// </summary>
    public class NavigationRoutingConvention : IODataControllerActionConvention
    {
        private readonly ILogger<NavigationRoutingConvention> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        public NavigationRoutingConvention(ILogger<NavigationRoutingConvention> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            string message = $"About page visited at {DateTime.UtcNow.ToLongTimeString()}";
            _logger.LogTrace(message);
        }

        /// <inheritdoc />
        public virtual int Order => 500;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            // structural property supports for entity set and singleton
            return context?.EntitySet != null || context?.Singleton != null;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.EntitySet == null && context.Singleton == null)
            {
                return false;
            }

            ActionModel action = context.Action;

            // Filter by the action name.
            // The action for navigation property request should follow up {httpMethod}{PropertyName}[From{Declaring}]
            string actionName = action.ActionMethod.Name;
            string method = SplitActionName(actionName, out string property, out string declared);
            if (method == null || string.IsNullOrEmpty(property))
            {
                return false;
            }

            IEdmNavigationSource navigationSource = context.EntitySet == null ?
                (IEdmNavigationSource)context.Singleton :
                (IEdmNavigationSource)context.EntitySet;

            // filter by action parameter
            IEdmEntityType entityType = navigationSource.EntityType();
            bool hasKeyParameter = action.HasODataKeyParameter(entityType);
            if (!(context.Singleton != null ^ hasKeyParameter))
            {
                // Singleton, doesn't allow to query property with key
                // entityset, doesn't allow for non-key to query property
                return false;
            }

            // Find the declaring type of the property if we have the declaring type name in the action name.
            // eitherwise, it means the property is defined on the entity type of the navigation source.
            IEdmEntityType declaringEntityType = entityType;
            if (declared != null)
            {
                declaringEntityType = entityType.FindTypeInInheritance(context.Model, declared) as IEdmEntityType;
                if (declaringEntityType == null)
                {
                    return false;
                }
            }

            // Find the property, and we only care about the navigation property.
            IEdmProperty edmProperty = declaringEntityType.FindProperty(property);
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

            AddSelector(method, context.Prefix, context.Model, action, navigationSource, declared, declaringEntityType, navigationProperty, hasKeyParameter, false);

            if (CanApplyDollarCount(navigationProperty, method))
            {
                AddSelector(method, context.Prefix, context.Model, action, navigationSource, declared, declaringEntityType, navigationProperty, hasKeyParameter, true);
            }

            return true;
        }

        private void AddSelector(string httpMethod, string prefix, IEdmModel model, ActionModel action,
            IEdmNavigationSource navigationSource, string declared, IEdmEntityType declaringEntityType,
            IEdmNavigationProperty navigationProperty, bool hasKey, bool dollarCount)
        {
            IEdmEntitySet entitySet = navigationSource as IEdmEntitySet;
            IEdmEntityType entityType = navigationSource.EntityType();

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
                segments.Add(new KeySegmentTemplate(entityType));
            }

            if (declared != null)
            {
                // It should be always single type
                segments.Add(new CastSegmentTemplate(declaringEntityType, entityType, navigationSource));
            }

            IEdmNavigationSource targetNavigationSource = navigationSource.FindNavigationTarget(navigationProperty, segments, out _);

            segments.Add(new NavigationSegmentTemplate(navigationProperty, targetNavigationSource));

            if (dollarCount)
            {
                segments.Add(CountSegmentTemplate.Instance);
            }

            ODataPathTemplate template = new ODataPathTemplate(segments);
            action.AddSelector(httpMethod, prefix, model, template);

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

        // OData spec: To request only the number of items of a collection of entities or items of a collection-valued property,
        // the client issues a GET request with /$count appended to the resource path of the collection.
        private static bool CanApplyDollarCount(IEdmNavigationProperty edmProperty, string method)
        {
            Contract.Assert(edmProperty != null);

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
                string message = action.DisplayName + ": " + template.Template;
                _addedODataSelector(logger, message, null);
            }
        }
    }
}
