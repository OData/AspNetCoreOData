// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
        private readonly ODataRoutingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoutingApplicationModelProvider" /> class.
        /// </summary>
        /// <param name="conventions">The registered OData routing conventions.</param>
        /// <param name="options">The registered OData routing options.</param>
        public ODataRoutingApplicationModelProvider(
            IEnumerable<IODataControllerActionConvention> conventions,
            IOptions<ODataRoutingOptions> options)
        {
            _options = options.Value;

            if (!_options.EnableAttributeRouting)
            {
                _controllerActionConventions = conventions.Where(c => !(c is AttributeRoutingConvention)).OrderBy(p => p.Order).ToArray();
            }
            else
            {
                _controllerActionConventions = conventions.OrderBy(p => p.Order).ToArray();
            }
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
            var routes = _options.Models;
            foreach (var route in routes)
            {
                IEdmModel model = route.Value;
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
                    if (!CanApply(route.Key, controller))
                    {
                        continue;
                    }

                    ODataControllerActionContext odataContext = BuildContext(route.Key, model, controller);

                    // consider to replace the Linq with others?
                    IODataControllerActionConvention[] conventions =
                        _controllerActionConventions.Where(c => c.AppliesToController(odataContext)).ToArray();

                    if (conventions.Length > 0)
                    {
                        foreach (var action in controller.Actions.Where(a => !a.IsNonODataAction()))
                        {
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

        private static ODataControllerActionContext BuildContext(string prefix, IEdmModel model, ControllerModel controller)
        {
            // The reason why to create a context is that:
            // We don't need to call te FindEntitySet or FindSingleton before every convention.
            // So, for a controller, we try to call "FindEntitySet" or "FindSingleton" once.
            string controllerName = controller.ControllerName;

            IEdmEntitySet entitySet = model.EntityContainer.FindEntitySet(controllerName);
            if (entitySet != null)
            {
                return new ODataControllerActionContext(prefix, model, controller, entitySet);
            }

            IEdmSingleton singleton = model.EntityContainer.FindSingleton(controllerName);
            if (singleton != null)
            {
                return new ODataControllerActionContext(prefix, model, controller, singleton);
            }

            return new ODataControllerActionContext(prefix, model, controller);
        }

        private static bool CanApply(string prefix, ControllerModel controller)
        {
            ODataModelAttribute odataModel = controller.GetAttribute<ODataModelAttribute>();
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
