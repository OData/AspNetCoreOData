// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// The convention for <see cref="IEdmOperationImport"/>.
    /// Get ~/functionimport(....)
    /// Post ~/actionimport
    /// </summary>
    public class OperationImportRoutingConvention : IODataControllerActionConvention
    {
        /// <inheritdoc />
        public virtual int Order => 900;

        /// <inheritdoc />
        public virtual bool AppliesToController(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            // By convention, we look for the controller name as "ODataOperationImportController"
            // Each operation import will be handled by the same action name in this controller.
            return context.Controller?.ControllerName == "ODataOperationImport";
        }

        /// <inheritdoc />
        public virtual bool AppliesToAction(ODataControllerActionContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            ActionModel action = context.Action;
            IEdmModel model = context.Model;

            // By convention, we use the operation import name as the action name in the controller
            string actionMethodName = action.ActionMethod.Name;

            var edmOperationImports = model.ResolveOperationImports(actionMethodName, enableCaseInsensitive: true);
            if (!edmOperationImports.Any())
            {
                return true;
            }

            (var actionImports, var functionImports) = edmOperationImports.Split();

            // That's not allowed to have an action import and function import with the same name.
            if (actionImports.Count > 0 && functionImports.Count > 0)
            {
                throw new ODataException($"Action import and function import '{actionMethodName}' MUST be unique within an entity container");
            }
            else if (actionImports.Count > 0 && context.Action.Attributes.Any(a => a is HttpPostAttribute))
            {
                if (actionImports.Count != 1)
                {
                    throw new ODataException($"Found multiple action imports with same '{actionMethodName}' name within an entity container.");
                }

                IEdmActionImport actionImport = actionImports[0];

                IEdmEntitySetBase targetSet;
                actionImport.TryGetStaticEntitySet(model, out targetSet);

                // TODO:
                // 1. shall we check the [HttpPost] attribute, or does the ASP.NET Core have the default?
                // 2) shall we check the action has "ODataActionParameters" parameter type?
                ODataPathTemplate template = new ODataPathTemplate(new ActionImportSegmentTemplate(actionImport, targetSet));
                action.AddSelector("Post", context.Prefix, context.Model, template, context.Options?.RouteOptions);
                return true;
            }
            else if (functionImports.Count > 0 && context.Action.Attributes.Any(a => a is HttpGetAttribute))
            {
                IEdmFunctionImport functionImport = FindFunctionImport(functionImports, action);
                if (functionImport == null)
                {
                    return false;
                }

                IEdmEntitySetBase targetSet;
                functionImport.TryGetStaticEntitySet(model, out targetSet);

                // TODO: 
                // 1) shall we check the [HttpGet] attribute, or does the ASP.NET Core have the default?
                ODataPathTemplate template = new ODataPathTemplate(new FunctionImportSegmentTemplate(functionImport, targetSet));
                action.AddSelector("Get", context.Prefix, context.Model, template, context.Options?.RouteOptions);
                return true;
            }
            else
            {
                // doesn't find an operation, return true means to skip the remaining conventions.
                return false;
            }
        }

        private static IEdmFunctionImport FindFunctionImport(IList<IEdmFunctionImport> functionImports, ActionModel action)
        {
            foreach (var functionImport in functionImports)
            {
                if (functionImport.Function.IsBound)
                {
                    continue;
                }

                bool match = true;
                foreach (var parameter in functionImport.Function.Parameters)
                {
                    if (!action.Parameters.Any(p => p.ParameterName == parameter.Name))
                    {
                        // if any parameter is not in the action parameters, skip this.
                        match = false;
                        break;
                    }

                    // TODO: shall we check each parameter has the [FromODataUri] attribute?
                }

                if (match)
                {
                    return functionImport;
                }
            }

            return null;
        }
    }
}
