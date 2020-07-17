// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for an odata template string.
    /// It looks for the <see cref="ODataRoutePrefixAttribute"/> on controller
    /// and <see cref="ODataRouteAttribute"/> on action.
    /// </summary>
    public class AttributeRoutingConvention : IODataControllerActionConvention
    {
        private readonly ILogger<AttributeRoutingConvention> _logger;
        private IODataPathTemplateParser _templateParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="logger">The registered logger.</param>
        /// <param name="parser">The registered parser.</param>
        public AttributeRoutingConvention(ILogger<AttributeRoutingConvention> logger,
            IODataPathTemplateParser parser)
        {
            _logger = logger;
            _templateParser = parser;
        }

        /// <inheritdoc />
        public virtual int Order => -100;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.RoutePrefixes = GetODataRoutePrefixes(context.Prefix, context.Controller);

            foreach (var routePrefix in GetODataRoutePrefixes(context.Prefix, context.Controller))
            {
                foreach (var action in context.Controller.Actions)
                {
                    var routeAttributes = action.Attributes.OfType<ODataRouteAttribute>();

                    foreach (ODataRouteAttribute routeAttribute in routeAttributes)
                    {
                        // If we have the route name setting, make sure we pass the attribute with the same route name.
                        if (!string.IsNullOrEmpty(routeAttribute.ModelName) && routeAttribute.ModelName != context.Prefix)
                        {
                            continue;
                        }

                        try
                        {
                            string routeTemplate = GetODataPathTemplateString(routePrefix, routeAttribute.PathTemplate);
                            ODataPathTemplate pathTemplate = _templateParser.Parse(context.Model, routeTemplate);
                            action.AddSelector(context.Prefix, context.Model, pathTemplate);
                        }
                        catch(ODataException ex)
                        {
                            string warning = String.Format(CultureInfo.CurrentCulture, SRResources.InvalidODataRouteOnAction,
                                routeAttribute.PathTemplate, action.ActionMethod.Name, context.Controller.ControllerName, ex.Message);

                            _logger.LogWarning(warning);
                        }
                    }
                }
            }

            // Returns false to make sure we don't want to call the AppliesToAction for this convention.
            return false;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ActionModel action = context.Action;

            // If we have the route name setting, make sure we pass the attribute with the same route name.
            var routeAttributes = action.Attributes.OfType<ODataRouteAttribute>()
                .Where(r => string.IsNullOrEmpty(r.ModelName) || r.ModelName != context.Prefix);
            if (!routeAttributes.Any())
            {
                return false;
            }

            foreach (ODataRouteAttribute routeAttribute in routeAttributes)
            {
                IEnumerable<string> routePrefixes = context.RoutePrefixes;
                if (routePrefixes == null)
                {
                    routePrefixes = new string[] { null };
                }

                foreach (var routePrefix in routePrefixes)
                {
                    try
                    {
                        string routeTemplate = GetODataPathTemplateString(routePrefix, routeAttribute.PathTemplate);
                        ODataPathTemplate pathTemplate = _templateParser.Parse(context.Model, routeTemplate);
                        action.AddSelector(context.Prefix, context.Model, pathTemplate);
                    }
                    catch (ODataException ex)
                    {
                        string warning = String.Format(CultureInfo.CurrentCulture, SRResources.InvalidODataRouteOnAction,
                            routeAttribute.PathTemplate, action.ActionMethod.Name, context.Controller.ControllerName, ex.Message);

                        _logger.LogWarning(warning);
                    }
                }
            }

            // we should let this action go to other conventions.
            return false;
        }

        /// <summary>
        /// Gets the route prefix on the controller.
        /// </summary>
        /// <param name="modelName">The model name.</param>
        /// <param name="controller">The controller.</param>
        /// <returns>The prefix string list.</returns>
        private IEnumerable<string> GetODataRoutePrefixes(string modelName, ControllerModel controller)
        {
            Contract.Assert(controller != null);

            var prefixAttributes = controller.Attributes.OfType<ODataRoutePrefixAttribute>();
            if (!prefixAttributes.Any())
            {
                yield return null;
            }

            foreach (ODataRoutePrefixAttribute prefixAttribute in prefixAttributes)
            {
                if (!string.IsNullOrEmpty(prefixAttribute.ModelName) && prefixAttribute.ModelName != modelName)
                {
                    continue;
                }

                string prefix = prefixAttribute.RoutePrefix;
                if (prefix != null && prefix.StartsWith("/", StringComparison.Ordinal))
                {
                    _logger.LogWarning($"The OData route prefix '{prefix}' on the controller '{controller.ControllerName}' starts with a '/'. Route prefixes cannot start with a '/'.");

                    // So skip it? or let's remove the "/" and let it go?
                    // continue;
                    prefix = prefix.TrimStart('/');
                }

                if (prefix != null && prefix.EndsWith("/", StringComparison.Ordinal))
                {
                    prefix = prefix.TrimEnd('/');
                }

                yield return prefix;
            }
        }

        private string GetODataPathTemplateString(string routePrefix, string pathTemplate)
        {
            if (routePrefix != null && !pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                if (String.IsNullOrEmpty(pathTemplate))
                {
                    pathTemplate = routePrefix;
                }
                else if (pathTemplate.StartsWith("(", StringComparison.Ordinal))
                {
                    // We don't need '/' when the pathTemplate starts with a key segment.
                    pathTemplate = routePrefix + pathTemplate;
                }
                else
                {
                    pathTemplate = routePrefix + "/" + pathTemplate;
                }
            }

            if (pathTemplate.StartsWith("/", StringComparison.Ordinal))
            {
                pathTemplate = pathTemplate.TrimStart('/');
            }

            return pathTemplate;
        }
    }
}
