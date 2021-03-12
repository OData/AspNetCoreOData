// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Builds or modifies <see cref="ApplicationModel" /> for OData convention action discovery.
    /// </summary>
    internal class ODataDynamicApplicationModelProvider : IApplicationModelProvider
    {
        private IServiceProvider _sp;
        private readonly ILogger<ODataDynamicApplicationModelProvider> _logger;
        private IODataPathTemplateParser _templateParser;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sp"></param>
        public ODataDynamicApplicationModelProvider(IServiceProvider sp,
            IODataPathTemplateParser parser,
            ILogger<ODataDynamicApplicationModelProvider> logger)
        {
            _sp = sp;
            _templateParser = parser;
            _logger = logger;
        }

        /// <summary>
        /// Gets the order value for determining the order of execution of providers.
        /// </summary>
        public int Order => 200;

        /// <summary>
        /// Executed for the second pass of <see cref="ApplicationModel"/> built.
        /// </summary>
        /// <param name="context">The <see cref="ApplicationModelProviderContext"/>.</param>
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // apply attribute routing.

            ODataModelContext modelContext = new ODataModelContext
            {
                ServiceProvider = _sp
            };

            foreach (var controllerModel in context.Result.Controllers)
            {
                modelContext.ControllerModel = controllerModel;
                IODataModelProvider modelProviderOnController = controllerModel.Attributes.OfType<IODataModelProvider>().FirstOrDefault();

                foreach (var actionModel in controllerModel.Actions)
                {
                    IODataModelProvider modelProviderOnAction = actionModel.Attributes.OfType<IODataModelProvider>().FirstOrDefault();

                    IODataModelProvider usedProvider;
                    if (modelProviderOnAction != null)
                    {
                        usedProvider = modelProviderOnAction;
                        modelContext.ActionModel = actionModel;
                    }
                    else if (modelProviderOnController != null)
                    {
                        usedProvider = modelProviderOnController;
                        modelContext.ActionModel = null;
                    }
                    else
                    {
                        continue;
                    }

                    IEdmModel edmModel = usedProvider.GetEdmModel(modelContext);
                    string prefix = usedProvider.Prefix;
                    IServiceProvider sp = usedProvider.SeviceProvider;

                    AppliesToAction(controllerModel, actionModel, edmModel, prefix, sp);
                }
            }
        }

        /// <summary>
        /// Executed for the first pass of <see cref="ApplicationModel"/> building.
        /// </summary>
        /// <param name="context">The <see cref="ApplicationModelProviderContext"/>.</param>
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            // Nothing here.
        }

        public virtual bool AppliesToAction(ControllerModel controllerModel, ActionModel actionModel,
            IEdmModel model, string prefix, IServiceProvider sp)
        {
            if (model == null)
            {
                throw Error.ArgumentNull(nameof(model));
            }

            // Loop through all attribute routes defined on the controller.
            var controllerSelectors = controllerModel.Selectors.Where(sm => sm.AttributeRouteModel != null).ToList();
            if (controllerSelectors.Count == 0)
            {
                // If no controller route template, we still need to go through action to process the action route template.
                controllerSelectors.Add(null);
            }

            // In order to avoiding pollute the action selectors, we use a Dictionary to save the intermediate results.
            IDictionary<SelectorModel, IList<SelectorModel>> updatedSelectors = new Dictionary<SelectorModel, IList<SelectorModel>>();
            foreach (var controllerSelector in controllerSelectors)
            {
                foreach (var actionSelector in actionModel.Selectors)
                {
                    // If non attribute routing on action, let's skip it.
                    // So, at least [HttpGet("")]
                    if (actionSelector.AttributeRouteModel == null)
                    {
                        continue;
                    }

                    var combinedRouteModel = AttributeRouteModel.CombineAttributeRouteModel(controllerSelector?.AttributeRouteModel, actionSelector.AttributeRouteModel);
                    if (combinedRouteModel == null)
                    {
                        // no an attribute routing, skip it.
                        continue;
                    }

                    string newRouteTemplate = GetODataRouteTemplate(combinedRouteModel.Template, prefix);
                    if (newRouteTemplate == null)
                    {
                        continue;
                    }

                    SelectorModel newSelectorModel = CreateActionSelectorModel(prefix, model, sp, newRouteTemplate, actionSelector,
                                combinedRouteModel.Template, actionModel.ActionName, controllerModel.ControllerName);
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

        private SelectorModel CreateActionSelectorModel(string prefix, IEdmModel model, IServiceProvider sp,
            string routeTemplate, SelectorModel actionSelectorModel,
            string originalTemplate, string actionName, string controllerName)
        {
            try
            {
                // Do the uri parser, it will throw exception if the route template is not a OData path.
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

            // remove the unused metadata
            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IRouteTemplateProvider)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Apply Default OData attribute routing
        /// </summary>
        internal static string GetODataRouteTemplate(string routeTemplate, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return routeTemplate;
            }

            if (routeTemplate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                // remove the prefix
                string newTempalte = routeTemplate.Substring(prefix.Length);

                if (newTempalte.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    return newTempalte.Substring(1);
                }

                return newTempalte;
            }

            return null;
        }
    }
}
