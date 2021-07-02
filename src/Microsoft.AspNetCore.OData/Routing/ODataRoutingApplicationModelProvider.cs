// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing
{
    /// <summary>
    /// Builds or modifies <see cref="ApplicationModel" /> for OData convention action discovery.
    /// </summary>
    internal class ODataRoutingApplicationModelProvider : IApplicationModelProvider
    {
        private readonly IODataControllerActionConvention[] _controllerActionConventions;
        private readonly ODataOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingApplicationModelProvider" /> class.
        /// </summary>
        /// <param name="options">The registered OData options.</param>
        public ODataRoutingApplicationModelProvider(IOptions<ODataOptions> options)
        {
            _options = options.Value;
            _controllerActionConventions = _options.Conventions
                .Where(c => c.GetType() != typeof(AttributeRoutingConvention)).OrderBy(p => p.Order).ToArray();
        }

        /// <summary>
        /// Gets the order value for determining the order of execution of providers.
        /// </summary>
        public int Order => 100;

        /// <summary>
        /// Executed for the second pass of <see cref="ApplicationModel"/> built.
        /// </summary>
        /// <param name="context">The <see cref="ApplicationModelProviderContext"/>.</param>
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // apply attribute routing.
            if (_options.EnableAttributeRouting)
            {
                ApplyAttributeRouting(context.Result.Controllers);
            }

            // apply non-attribute convention routing.
            foreach (var route in _options.RouteComponents)
            {
                IEdmModel model = route.Value.EdmModel;
                if (model == null || model.EntityContainer == null)
                {
                    continue;
                }

                foreach (var controller in context.Result.Controllers)
                {
                    // Skip the controller with [NonODataController] attribute decorated.
                    if (controller.HasAttribute<NonODataControllerAttribute>())
                    {
                        continue;
                    }

                    // Apply to ODataModelAttribute
                    if (!CanApply(route.Key, () => controller.GetAttribute<ODataModelAttribute>()))
                    {
                        continue;
                    }

                    ODataControllerActionContext odataContext = new ODataControllerActionContext(route.Key, model, controller);

                    odataContext.NavigationSource = model.ResolveNavigationSource(controller.ControllerName,
                        _options.RouteOptions.EnableControllerNameCaseInsensitive);

                    odataContext.Options = _options;

                    IODataControllerActionConvention[] conventions =
                        _controllerActionConventions.Where(c => c.AppliesToController(odataContext)).ToArray();

                    if (conventions.Length > 0)
                    {
                        foreach (var action in controller.Actions.Where(a => !a.IsNonODataAction()))
                        {
                            if (!CanApply(route.Key, () => action.GetAttribute<ODataModelAttribute>()))
                            {
                                continue;
                            }

                            // Reset the action on the context.
                            odataContext.Action = action;

                            foreach (var convention in conventions)
                            {
                                if (convention.AppliesToAction(odataContext))
                                {
                                    break;
                                }
                            }
                        }
                    }
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

        /// <summary>
        /// Apply Default OData attribute routing
        /// </summary>
        /// <param name="controllers">The controller models</param>
        internal void ApplyAttributeRouting(IList<ControllerModel> controllers)
        {
            AttributeRoutingConvention attributeRouting = _options.Conventions.OfType<AttributeRoutingConvention>().FirstOrDefault();
            if (attributeRouting == null)
            {
                return;
            }

            ODataControllerActionContext controllerActionContext = new ODataControllerActionContext
            {
                Options = _options
            };

            foreach (var controllerModel in controllers.Where(c => !c.IsNonODataController()))
            {
                controllerActionContext.Controller = controllerModel;

                foreach (var actionModel in controllerModel.Actions.Where(a => !a.IsNonODataAction()))
                {
                    controllerActionContext.Action = actionModel;

                    attributeRouting.AppliesToAction(controllerActionContext);
                }
            }
        }

        internal static bool CanApply(string prefix, Func<ODataModelAttribute> func)
        {
            ODataModelAttribute odataModel = func?.Invoke();
            if (odataModel == null)
            {
                return true; // apply to all model
            }
            else if (prefix == odataModel.Model)
            {
                return true;
            }

            return false;
        }
    }
}
