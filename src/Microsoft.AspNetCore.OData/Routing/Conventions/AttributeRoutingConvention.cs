//-----------------------------------------------------------------------------
// <copyright file="AttributeRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for an OData template string.
    /// It looks for the <see cref="ODataAttributeRoutingAttribute"/> on controller
    /// and <see cref="ODataAttributeRoutingAttribute"/> or other Http Verb attribute, for example <see cref="HttpGetAttribute"/> on action.
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
            return true;
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // Be noted, the validation checks (non OData controller, non OData action) are done before calling this method.
            ControllerModel controllerModel = context.Controller;
            ActionModel actionModel = context.Action;

            bool isODataController = controllerModel.Attributes.Any(a => a is ODataAttributeRoutingAttribute);
            bool isODataAction = actionModel.Attributes.Any(a => a is ODataAttributeRoutingAttribute);

            // At least one of controller or action has "ODataRoutingAttribute"
            // The best way is to derive your controller from ODataController.
            if (!isODataController && !isODataAction)
            {
                return false;
            }

            // TODO: Which one is better? input from context or inject from constructor?
            IEnumerable<string> prefixes = context.Options.RouteComponents.Keys;

            // Loop through all attribute routes defined on the controller.
            var controllerSelectors = controllerModel.Selectors.Where(sm => sm.AttributeRouteModel != null).ToList();
            if (controllerSelectors.Count == 0)
            {
                // If no controller route template, we still need to go through action to process the action route template.
                controllerSelectors.Add(null);
            }

            // In order to avoiding polluting the action selectors, we use a Dictionary to save the intermediate results.
            IDictionary<SelectorModel, IList<SelectorModel>> updatedSelectors = new Dictionary<SelectorModel, IList<SelectorModel>>();
            foreach (var actionSelector in actionModel.Selectors)
            {
                if (actionSelector.AttributeRouteModel != null && actionSelector.AttributeRouteModel.IsAbsoluteTemplate)
                {
                    ProcessAttributeModel(actionSelector.AttributeRouteModel, prefixes, context, actionSelector, actionModel, controllerModel, updatedSelectors);
                }
                else
                {
                    foreach (var controllerSelector in controllerSelectors)
                    {
                        var combinedRouteModel = AttributeRouteModel.CombineAttributeRouteModel(controllerSelector?.AttributeRouteModel, actionSelector.AttributeRouteModel);
                        ProcessAttributeModel(combinedRouteModel, prefixes, context, actionSelector, actionModel, controllerModel, updatedSelectors);
                    }
                }
            }

            // remove the old one.
            foreach (var selector in updatedSelectors)
            {
                actionModel.Selectors.Remove(selector.Key);
            }

            // add new one.
            foreach (var selector in updatedSelectors)
            {
                foreach (var newSelector in selector.Value)
                {
                    actionModel.Selectors.Add(newSelector);
                }
            }

            // let's just return false to let this action go to other conventions.
            return false;
        }

        private void ProcessAttributeModel(AttributeRouteModel attributeRouteModel, IEnumerable<string> prefixes,
            ODataControllerActionContext context, SelectorModel actionSelector, ActionModel actionModel, ControllerModel controllerModel,
            IDictionary<SelectorModel, IList<SelectorModel>> updatedSelectors)
        {
            if (attributeRouteModel == null)
            {
                // not an attribute routing, skip it.
                return;
            }

            string prefix = FindRelatedODataPrefix(attributeRouteModel.Template, prefixes, out string newRouteTemplate);
            if (prefix == null)
            {
                return;
            }

            IEdmModel model = context.Options.RouteComponents[prefix].EdmModel;
            IServiceProvider sp = context.Options.RouteComponents[prefix].ServiceProvider;

            SelectorModel newSelectorModel = CreateActionSelectorModel(prefix, model, sp, newRouteTemplate, actionSelector,
                        attributeRouteModel.Template, actionModel.ActionName, controllerModel.ControllerName);
            if (newSelectorModel != null)
            {
                IList<SelectorModel> selectors;
                if (!updatedSelectors.TryGetValue(actionSelector, out selectors))
                {
                    selectors = new List<SelectorModel>();
                    updatedSelectors[actionSelector] = selectors;
                }

                selectors.Add(newSelectorModel);
            }
        }

        private SelectorModel CreateActionSelectorModel(string prefix, IEdmModel model, IServiceProvider sp,
            string routeTemplate, SelectorModel actionSelectorModel,
            string originalTemplate, string actionName, string controllerName)
        {
            try
            {
                // Due the uri parser, it will throw exception if the route template is not a OData path.
                ODataPathTemplate pathTemplate = _templateParser.Parse(model, routeTemplate, sp);
                if (pathTemplate != null)
                {
                    // Create a new selector model?
                    SelectorModel newSelectorModel = new SelectorModel(actionSelectorModel);
                    // Shall we remove any certain attributes/metadata?
                    ClearMetadata(newSelectorModel);

                    // Add OData routing metadata
                    ODataRoutingMetadata odataMetadata = new ODataRoutingMetadata(prefix, model, pathTemplate);
                    newSelectorModel.EndpointMetadata.Add(odataMetadata);

                    // replace the attribute routing template using absolute routing template to avoid appending any controller route template
                    newSelectorModel.AttributeRouteModel = new AttributeRouteModel()
                    {
                        Template = $"/{originalTemplate}" // add a "/" to make sure it's absolute template, don't combine with controller
                    };

                    return newSelectorModel;
                }

                return null;
            }
            catch (ODataException ex)
            {
                // use the logger to log the wrong odata attribute template. Shall we log the others?
                string warning = string.Format(CultureInfo.CurrentCulture, SRResources.InvalidODataRouteOnAction,
                    originalTemplate, actionName, controllerName, ex.Message);

                // Whether we throw exception or mark it as warning is a design pattern.
                // throw new ODataException(warning);
                _logger.LogWarning(warning);
                return null;
            }
        }

        private static void ClearMetadata(SelectorModel selectorModel)
        {
            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is IRouteTemplateProvider)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            //for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            //{
            //    if (selectorModel.ActionConstraints[i] is HttpMethodActionConstraint)
            //    {
            //        selectorModel.ActionConstraints.RemoveAt(i);
            //    }
            //}

            // remove the unused metadata
            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IRouteTemplateProvider)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            //for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            //{
            //    if (selectorModel.EndpointMetadata[i] is IHttpMethodMetadata)
            //    {
            //        selectorModel.EndpointMetadata.RemoveAt(i);
            //    }
            //}
        }

        private static string FindRelatedODataPrefix(string routeTemplate, IEnumerable<string> prefixes, out string newRouteTemplate)
        {
            if (routeTemplate.StartsWith('/'))
            {
                routeTemplate = routeTemplate.Substring(1);
            }
            else if (routeTemplate.StartsWith("~/", StringComparison.Ordinal))
            {
                routeTemplate = routeTemplate.Substring(2);
            }

            // the input route template could be:
            // #1) odata/Customers/{key}
            // #2) orders({key})
            // So, #1 matches the "odata" prefix route
            //     #2 matches the non-odata prefix route
            // Since #1 and #2 can be considered starting with "",
            // In order to avoiding ambiguous, let's compare non-empty route prefix first,
            // If no match, then compare empty route prefix.
            string emptyPrefix = null;
            foreach (var prefix in prefixes)
            {
                if (string.IsNullOrEmpty(prefix))
                {
                    emptyPrefix = prefix;
                    continue;
                }
                else if (routeTemplate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    // we hit: "odata/Customers/{key}" scenario, let's remove the "odata" route prefix
                    newRouteTemplate = routeTemplate.Substring(prefix.Length);

                    // why do like this: because the input route template could be "odata", after remove the prefix, it's empty string.
                    if (newRouteTemplate.StartsWith("/", StringComparison.Ordinal))
                    {
                        newRouteTemplate = newRouteTemplate.Substring(1);
                    }

                    return prefix;
                }
            }

            // we are here because no non-empty prefix matches.
            if (emptyPrefix != null)
            {
                // So, if we have empty prefix route, it could match all OData route template.
                newRouteTemplate = routeTemplate;
                return emptyPrefix;
            }

            newRouteTemplate = null;
            return null;
        }
    }
}
